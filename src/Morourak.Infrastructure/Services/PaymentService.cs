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
using Microsoft.Extensions.Options;
using Morourak.Infrastructure.Settings;
using AutoMapper;

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
    private readonly PaymentSettings _paymentSettings;
    private readonly IMapper _mapper;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IPayMobService payMobService,
        ICurrentUserService currentUser,
        IDrivingLicenseService drivingService,
        IVehicleLicenseService vehicleService,
        ILogger<PaymentService> logger,
        UserManager<ApplicationUser> userManager,
        IOptions<PaymentSettings> paymentSettings,
        IMapper mapper
       )
    {
        _unitOfWork = unitOfWork;
        _payMobService = payMobService;
        _currentUser = currentUser;
        _drivingService = drivingService;
        _vehicleService = vehicleService;
        _logger = logger;
        _userManager = userManager;
        _paymentSettings = paymentSettings.Value;
        _mapper = mapper;
    }

    public async Task<PaymobPaymentResponse> CreatePaymentAsync(PaymentCreateRequest dto)
    {
        var nationalId = _currentUser.NationalId;
        if (string.IsNullOrEmpty(nationalId))
            throw new AppEx.UnauthorizedException("المستخدم غير مصرح له.");

        if (!string.IsNullOrEmpty(dto.ServiceRequestNumber))
        {
            // Only block if the payment is already SUCCESSFUL (Paid).
            // If it's Pending or Failed, allow creating a new attempt to get a fresh token/URL.
            var existingPaid = await _unitOfWork.Repository<Payment>().GetAsync(
                p => p.ServiceRequest != null && p.ServiceRequest.RequestNumber == dto.ServiceRequestNumber
                  && p.CitizenNationalId     == nationalId
                  && p.Status               == PaymentStatus.Paid);

            if (existingPaid != null)
            {
                throw new AppEx.ValidationException("هذا الطلب تم دفعه بنجاح بالفعل.", "ALREADY_PAID");
            }
        }

        decimal totalAmount = await CalculateFeesAsync(dto.ServiceRequestNumber, dto.ViolationIds);

        if (totalAmount <= 0)
            throw new AppEx.ValidationException("المبلغ الإجمالي يجب أن يكون أكبر من صفر.", "INVALID_AMOUNT");

        var merchantOrderId = $"MOR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32);

        _logger.LogInformation(
            "PAYMENT_CREATE_ATTEMPT: MerchantOrderId={MerchantOrderId}, NationalId={NationalId}, Amount={Amount} EGP",
            merchantOrderId, nationalId, totalAmount);

        var citizen = await _unitOfWork.Repository<CitizenRegistry>()
                .GetAsync(c => c.NationalId == _currentUser.NationalId);

        if (citizen == null)
            throw new AppEx.NotFoundException($"لم يتم العثور على المواطن بالرقم القومي {_currentUser.NationalId}.");

        var user = await _userManager.FindByNameAsync(_currentUser.NationalId);

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

        var payment = new Payment
        {
            MerchantOrderId = merchantOrderId,
            PaymobOrderId = response.PaymobOrderId,
            Amount = totalAmount,
            Status = PaymentStatus.Pending,
            CitizenNationalId = nationalId,
            CreatedAt = DateTime.UtcNow,
            TransactionId = null 
        };

        if (!string.IsNullOrEmpty(dto.ServiceRequestNumber))
        {
            var request = await _unitOfWork.Repository<ServiceRequest>().GetAsync(r => r.RequestNumber == dto.ServiceRequestNumber);
            if (request != null)
            {
                payment.ServiceRequestId = request.Id;
                payment.PaymentItems.Add(new PaymentItem { Description = $"رسوم {request.ServiceType}", Amount = request.TotalAmount });
            }
        }

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

        var receipt = _mapper.Map<PaymentReceiptDto>(payment);
        receipt.CitizenName = citizen != null ? $"{citizen.FirstName} {citizen.LastName}" : "مواطن";
        
        return receipt;
    }

    public async Task<bool> FinalizePaymentAsync(string paymobOrderId, string transactionId, bool success, string? merchantOrderId = null)
    {
        return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
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
                _logger.LogWarning(
                    "DUPLICATE_WEBHOOK_IGNORED: Payment {MerchantOrderId} (TransactionId={TransactionId}) is already Paid. No state mutation performed.",
                    payment.MerchantOrderId, payment.TransactionId);
                return true;
            }

            bool transactionAlreadySet = payment.TransactionId == transactionId
                && !string.IsNullOrEmpty(transactionId);

            if (!transactionAlreadySet)
                payment.TransactionId = transactionId;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Clear tracking at the START to avoid conflicts with previous operations in the same scope
                _unitOfWork.ClearTracking();

                if (success)
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;

                    if (payment.ServiceRequestId.HasValue)
                    {
                        var request = await _unitOfWork.Repository<ServiceRequest>()
                            .GetByIdAsync(payment.ServiceRequestId.Value);

                        if (request != null && request.PaymentStatus != PaymentStatus.Paid)
                        {
                            request.PaymentStatus = PaymentStatus.Paid;
                            request.PaymentTransactionId = transactionId;
                            request.PaymentAmount = payment.Amount;
                            request.PaymentTimestamp = DateTime.UtcNow;
                            request.Status = RequestStatus.ReadyForProcessing;
                            request.LastUpdatedAt = DateTime.UtcNow;

                            _unitOfWork.Repository<ServiceRequest>().Update(request);

                            if (IsDrivingLicenseService(request.ServiceType))
                                await _drivingService.CompleteIssuanceAsync(request.RequestNumber);
                            else if (IsVehicleLicenseService(request.ServiceType))
                                await _vehicleService.CompleteIssuanceAsync(request.RequestNumber);

                            request.Status = RequestStatus.Completed; 
                            _unitOfWork.Repository<ServiceRequest>().Update(request);
                        }
                    }

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
                payment.PaidAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);
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
                throw; 
            }
        });
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
            
            total += violations
                .Where(v => v.Status != ViolationStatus.Paid && v.FineAmount > v.PaidAmount)
                .Sum(v => v.FineAmount - v.PaidAmount);
        }

        return total;
    }

    public async Task<PaymentStatus> GetStatusAsync(string merchantOrderId)
    {
        _logger.LogInformation("POLLING_STATUS: MerchantOrderId={MerchantOrderId}", merchantOrderId);

        var payment = await _unitOfWork.Repository<Payment>().GetAsync(p => p.MerchantOrderId == merchantOrderId);
        
        if (payment == null)
        {
            _logger.LogWarning("POLLING_STATUS_NOT_FOUND: MerchantOrderId={MerchantOrderId}", merchantOrderId);
            return PaymentStatus.Failed;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogInformation("POLLING_STATUS_FINALIZED: MerchantOrderId={MerchantOrderId}, Status={Status}", merchantOrderId, payment.Status);
            return payment.Status;
        }

        if (!string.IsNullOrEmpty(payment.PaymobOrderId))
        {
            try
            {
                var paymobResult = await CheckPaymentWithPaymobAsync(merchantOrderId);
                
                if (paymobResult.Status == PaymentStatus.Paid)
                {
                    _logger.LogInformation("POLLING_SUCCESS: Payment {MerchantOrderId} confirmed as Paid by Paymob polling.", merchantOrderId);
                    
                    await FinalizePaymentAsync(
                        payment.PaymobOrderId, 
                        paymobResult.TransactionId ?? "POLLING_TX", 
                        true, 
                        merchantOrderId);
                    
                    return PaymentStatus.Paid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status with Paymob during polling for MerchantOrderId: {MerchantOrderId}", merchantOrderId);
            }
        }

        return payment.Status;
    }

    public async Task<PaymentStatusResult> CheckPaymentWithPaymobAsync(string merchantOrderId)
    {
        var payment = await _unitOfWork.Repository<Payment>().GetAsync(p => p.MerchantOrderId == merchantOrderId);
        if (payment == null || string.IsNullOrEmpty(payment.PaymobOrderId))
        {
            throw new AppEx.NotFoundException($"Payment not found for MerchantOrderId: {merchantOrderId}");
        }

        _logger.LogInformation("PAYMOB_CHECK: Calling Paymob API for OrderId={PaymobOrderId}", payment.PaymobOrderId);
        
        var result = await _payMobService.CheckPaymentStatusAsync(payment.PaymobOrderId);
        
        _logger.LogInformation("PAYMOB_CHECK_RESULT: OrderId={PaymobOrderId}, Status={Status}, Amount={Amount}", 
            payment.PaymobOrderId, result.Status, result.Amount);

        return result;
    }

    public async Task MarkAsPaidForDemo(string merchantOrderId)
    {
        var payment = await _unitOfWork.Repository<Payment>().GetAsync(p => p.MerchantOrderId == merchantOrderId);
        
        if (payment != null && payment.Status == PaymentStatus.Pending)
        {
            _logger.LogInformation("DEMO_MODE_FORCED_SUCCESS: Marking Payment {MerchantOrderId} as Paid (DemoMode={IsDemo}).", 
                merchantOrderId, _paymentSettings.DemoMode);
            
            await FinalizePaymentAsync(
                payment.PaymobOrderId ?? "DEMO", 
                "DEMO_TX_" + Guid.NewGuid().ToString("N").Substring(0, 8), 
                true, 
                merchantOrderId);
        }
    }

    private static bool IsDrivingLicenseService(ServiceType serviceType)
        => serviceType is
            ServiceType.DrivingLicenseIssue or
            ServiceType.DrivingLicenseRenewal or
            ServiceType.DrivingLicenseReplacementLost or
            ServiceType.DrivingLicenseReplacementDamaged or
            ServiceType.DrivingLicenseUpgrade;

    private static bool IsVehicleLicenseService(ServiceType serviceType)
        => serviceType is
            ServiceType.VehicleLicenseIssue or
            ServiceType.VehicleLicenseRenewal or
            ServiceType.VehicleLicenseReplacementLost or
            ServiceType.VehicleLicenseReplacementDamaged;
}
