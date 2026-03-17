using Morourak.Application.Common;
using Morourak.Application.DTOs.Paymob;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Violations;
using Microsoft.Extensions.Logging;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Morourak.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayMobService _payMobService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDrivingLicenseService _drivingService;
    private readonly IVehicleLicenseService _vehicleService;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IPayMobService payMobService,
        ICurrentUserService currentUser,
        IDrivingLicenseService drivingService,
        IVehicleLicenseService vehicleService,
        ILogger<PaymentService> logger,
        UserManager<ApplicationUser> userManager
       )
    {
        _unitOfWork = unitOfWork;
        _payMobService = payMobService;
        _currentUser = currentUser;
        _drivingService = drivingService;
        _vehicleService = vehicleService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<PaymobPaymentResponse> CreatePaymentAsync(PaymentCreateRequest dto)
    {
        var nationalId = _currentUser.NationalId;
        if (string.IsNullOrEmpty(nationalId))
            throw new AppEx.UnauthorizedException("المستخدم غير مصرح له.");

        // ── IDEMPOTENCY GUARD ─────────────────────────────────────────────────
        // Before hitting Paymob, check whether a non-Failed Payment already
        // exists for this service request. This protects against:
        //   1. Flutter double-tap on the Pay button
        //   2. Network retry that fires the request twice
        // A Failed payment is allowed to be re-created (user may retry).
        // ─────────────────────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(dto.ServiceRequestNumber))
        {
            var existing = await _unitOfWork.Repository<Payment>().GetAsync(
                p => p.ServiceRequestNumber  == dto.ServiceRequestNumber
                  && p.CitizenNationalId     == nationalId
                  && p.Status               != PaymentStatus.Failed);

            if (existing != null)
            {
                _logger.LogWarning(
                    "IDEMPOTENCY_HIT: Payment {MerchantOrderId} already exists for ServiceRequest {ServiceRequestNumber}. Returning cached response.",
                    existing.MerchantOrderId, dto.ServiceRequestNumber);

                // Reconstruct the response from the stored record so the client
                // gets the same data it would have received on first creation.
                return new PaymobPaymentResponse
                {
                    PaymentId      = existing.Id,
                    MerchantOrderId = existing.MerchantOrderId,
                    PaymobOrderId  = existing.PaymobOrderId ?? string.Empty,
                    PaymentToken   = string.Empty,  // token is short-lived; client uses PaymentUrl
                    PaymentUrl     = $"https://accept.paymob.com/api/acceptance/iframes/0?payment_token=EXPIRED"
                };
            }
        }

        decimal totalAmount = await CalculateFeesAsync(dto.ServiceRequestNumber, dto.ViolationIds);

        if (totalAmount <= 0)
            throw new AppEx.ValidationException("المبلغ الإجمالي يجب أن يكون أكبر من صفر.", "INVALID_AMOUNT");

        // Format: MOR-{yyyyMMdd}-{Guid:N:16} (Total 32 chars)
        var merchantOrderId = $"MOR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32);

        _logger.LogInformation(
            "PAYMENT_CREATE_ATTEMPT: MerchantOrderId={MerchantOrderId}, NationalId={NationalId}, Amount={Amount} EGP",
            merchantOrderId, nationalId, totalAmount);


        var citizen = await _unitOfWork.Repository<CitizenRegistry>()
                .GetAsync(c => c.NationalId == _currentUser.NationalId);

        if (citizen == null)
            throw new AppEx.NotFoundException($"لم يتم العثور على المواطن بالرقم القومي {_currentUser.NationalId}.");

        var user = await _userManager.FindByNameAsync(_currentUser.NationalId); // ApplicationUser

        var email = user?.Email ?? $"{citizen.NationalId}@morourak.gov.eg";

        var response = await _payMobService.InitiatePaymentAsync(
            totalAmount,
            merchantOrderId,
            citizen.FirstName,
            citizen.LastName,
            email,                    
            citizen.MobileNumber,     
            "Egypt",                  
            "Cairo",                  
            "NA",                     
            "NA"                      
        );

        // 2. Create Payment record
        var payment = new Payment
        {
            MerchantOrderId = merchantOrderId,
            PaymobOrderId = response.PaymobOrderId,
            Amount = totalAmount,
            Status = PaymentStatus.Pending,
            CitizenNationalId = nationalId,
            ServiceRequestNumber = dto.ServiceRequestNumber,
            CreatedAt = DateTime.UtcNow,
            // Do not set a temporary TransactionId here to avoid unique constraint conflicts
            // if the payment is recreated or retried before finalization.
            TransactionId = null 
        };

        // Add Service Request Item (Batch fetch later if multiple, but here it's single)
        if (!string.IsNullOrEmpty(dto.ServiceRequestNumber))
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == dto.ServiceRequestNumber);
            if (request != null)
            {
                payment.PaymentItems.Add(new PaymentItem { Description = $"رسوم {request.ServiceType}", Amount = request.TotalAmount });
            }
        }

        // Optimized Batch fetch for Violations
        if (dto.ViolationIds != null && dto.ViolationIds.Any())
        {
            var violations = await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => dto.ViolationIds.Contains(v.Id));

            foreach (var v in violations)
            {
                var unpaid = v.FineAmount - v.PaidAmount;
                if (unpaid > 0)
                {
                    payment.PaymentItems.Add(new PaymentItem { Description = $"مخالفة رقم {v.ViolationNumber}", Amount = unpaid });
                    payment.PaymentViolations.Add(new PaymentViolation { TrafficViolationId = v.Id, AmountPaid = unpaid });
                }
            }
        }

        await _unitOfWork.Repository<Payment>().AddAsync(payment);
        await _unitOfWork.CommitAsync();

        response.PaymentId = payment.Id;

        return response;
    }

    public async Task<PaymentReceiptDto> GetReceiptAsync(string merchantOrderId)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetAsync(x => x.MerchantOrderId == merchantOrderId, 
                p => p.PaymentViolations, 
                p => p.PaymentItems);
        
        if (payment == null || payment.Status != PaymentStatus.Paid)
            throw new AppEx.ValidationException("الإيصال غير متوفر أو العملية لم تكتمل.", "RECEIPT_NOT_FOUND");

        var citizen = await _unitOfWork.Repository<CitizenRegistry>()
            .GetAsync(c => c.NationalId == payment.CitizenNationalId);

        return new PaymentReceiptDto
        {
            TransactionId = payment.TransactionId ?? "N/A",
            MerchantOrderId = payment.MerchantOrderId,
            TotalAmount = payment.Amount,
            PaidAt = payment.PaidAt ?? DateTime.UtcNow,
            CitizenName = citizen != null ? $"{citizen.FirstName} {citizen.LastName}" : "مواطن",
            Items = payment.PaymentItems.Select(i => new ReceiptItemDto 
            { 
                Description = i.Description, 
                Amount = i.Amount 
            }).ToList()
        };
    }

    public async Task<bool> FinalizePaymentAsync(string paymobOrderId, string transactionId, bool success, string? merchantOrderId = null)
    {
        _logger.LogInformation("Finalizing payment. MerchantOrderId: {MerchantOrderId}, PaymobOrderId: {PaymobOrderId}. Success: {Success}", merchantOrderId ?? "N/A", paymobOrderId, success);

        Payment? payment = null;

        if (!string.IsNullOrEmpty(merchantOrderId))
        {
            payment = await _unitOfWork.Repository<Payment>()
                .GetAsync(x => x.MerchantOrderId == merchantOrderId, p => p.PaymentViolations);
        }

        if (payment == null)
        {
            payment = await _unitOfWork.Repository<Payment>()
                .GetAsync(x => x.PaymobOrderId == paymobOrderId, p => p.PaymentViolations);
        }

        if (payment == null)
        {
            _logger.LogWarning("Payment record not found. MerchantOrderId: {MerchantOrderId}, PaymobOrderId: {PaymobOrderId}", merchantOrderId ?? "N/A", paymobOrderId);
            return false;
        }
        
        if (payment.Status == PaymentStatus.Paid)
        {
            // ── IDEMPOTENCY: duplicate webhook ────────────────────────────────
            // Paymob may deliver the same webhook more than once (at-least-once
            // delivery). We must silently acknowledge it (return 200) but NEVER
            // mutate state a second time. Log at Warning so this is easily
            // queryable in production when investigating payment discrepancies.
            // ─────────────────────────────────────────────────────────────────
            _logger.LogWarning(
                "DUPLICATE_WEBHOOK_IGNORED: Payment {MerchantOrderId} (TransactionId={TransactionId}) is already Paid. No state mutation performed.",
                payment.MerchantOrderId, payment.TransactionId);
            return true;
        }

        // Idempotency guard: if a Failed payment is retried with the same transactionId
        // (e.g., Paymob sends the webhook twice), skip re-setting TransactionId to avoid
        // a unique constraint violation and just update the status.
        bool transactionAlreadySet = payment.TransactionId == transactionId
            && !string.IsNullOrEmpty(transactionId);

        if (!transactionAlreadySet)
            payment.TransactionId = transactionId;
        
        // Use atomic transaction for finalization
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (success)
            {
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;

                // Update Service Request
                if (!string.IsNullOrEmpty(payment.ServiceRequestNumber))
                {
                    var request = await _unitOfWork.Repository<ServiceRequest>()
                        .GetAsync(x => x.RequestNumber == payment.ServiceRequestNumber);

                    if (request != null && request.PaymentStatus != PaymentStatus.Paid)
                    {
                        request.PaymentStatus = PaymentStatus.Paid;
                        request.PaymentTransactionId = transactionId;
                        request.PaymentAmount = payment.Amount;
                        request.PaymentTimestamp = DateTime.UtcNow;
                        request.Status = RequestStatus.ReadyForProcessing;
                        request.LastUpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Repository<ServiceRequest>().Update(request);

                        // License completion logic (Internal CommitAsync should be removed in these methods)
                        if (request.ServiceType.ToString().Contains("DrivingLicense"))
                            await _drivingService.CompleteIssuanceAsync(request.RequestNumber);
                        else if (request.ServiceType.ToString().Contains("VehicleLicense"))
                            await _vehicleService.CompleteIssuanceAsync(request.RequestNumber);

                        request.Status = RequestStatus.Completed; // Move to Completed ONLY after everything succeeds
                        _unitOfWork.Repository<ServiceRequest>().Update(request);
                    }
                }

                // Update Violations
                if (payment.PaymentViolations.Any())
                {
                    var violationIds = payment.PaymentViolations.Select(pv => pv.TrafficViolationId).ToList();
                    var violations = await _unitOfWork.Repository<TrafficViolation>()
                        .FindAsync(v => violationIds.Contains(v.Id));

                    foreach (var v in violations)
                    {
                        if (v.Status != ViolationStatus.Paid)
                        {
                            var paymentViolation = payment.PaymentViolations
                                .FirstOrDefault(pv => pv.TrafficViolationId == v.Id);
                            var amountPaid = paymentViolation?.AmountPaid ?? (v.FineAmount - v.PaidAmount);

                            v.PaidAmount += amountPaid;
                            if (v.PaidAmount >= v.FineAmount)
                            {
                                v.PaidAmount = v.FineAmount;
                                v.Status = ViolationStatus.Paid;
                            }
                            v.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.Repository<TrafficViolation>().Update(v);
                        }
                    }
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            // Commit all changes in one go
            await _unitOfWork.CommitAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Payment finalization complete. MerchantOrderId: {MerchantOrderId}, TransactionId: {TransactionId}, Success: {Success}",
                payment.MerchantOrderId, transactionId, success);

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Critical error during payment finalization for MerchantOrderId: {MerchantOrderId}", payment.MerchantOrderId);
            throw; // Re-throw to let the controller handle it
        }
    }

    public async Task<decimal> CalculateFeesAsync(string? requestNumber, List<int>? violationIds)
    {
        decimal total = 0;
        
        if (!string.IsNullOrEmpty(requestNumber))
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == requestNumber);
            if (request != null) total += request.TotalAmount;
        }
        
        if (violationIds != null && violationIds.Any())
        {
            var violations = await _unitOfWork.Repository<TrafficViolation>()
                .FindAsync(v => violationIds.Contains(v.Id));
            
            // Only include violations that still have an outstanding balance.
            total += violations
                .Where(v => v.Status != ViolationStatus.Paid && v.FineAmount > v.PaidAmount)
                .Sum(v => v.FineAmount - v.PaidAmount);
        }

        return total;
    }

    public async Task<PaymentStatus> GetStatusAsync(string merchantOrderId)
    {
        var payment = await _unitOfWork.Repository<Payment>().GetAsync(p => p.MerchantOrderId == merchantOrderId);
        return payment?.Status ?? PaymentStatus.Failed;
    }
}
