using Morourak.Application.DTOs;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestNumberGenerator _generator;
    private readonly ICurrentUserService _currentUser;

    public ServiceRequestService(
        IUnitOfWork unitOfWork,
        IRequestNumberGenerator generator,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _generator = generator;
        _currentUser = currentUser;
    }

    private ServiceRequestDto ToDto(ServiceRequest request)
    {
        return new ServiceRequestDto
        {
            RequestNumber = request.RequestNumber,
            CitizenNationalId = request.CitizenNationalId,
            ServiceType = request.ServiceType.ToString(),
            Status = request.Status.ToString(),
            SubmittedAt = request.SubmittedAt,
            LastUpdatedAt = request.LastUpdatedAt,
            ReferenceId = request.ReferenceId,
            PaymentStatus = request.PaymentStatus.ToString(),
            PaymentTransactionId = request.PaymentTransactionId,
            PaymentAmount = request.PaymentAmount,
            PaymentTimestamp = request.PaymentTimestamp
        };
    }
    public async Task<IReadOnlyList<ServiceRequestDto>> GetCitizenRequestsAsync()
    {
        var nationalId = _currentUser.NationalId
            ?? throw new AppEx.ValidationException("??? ?????? ??? ????? ?? ??? ??????.", "AUTH_MISSING_NATIONAL_ID");

        var requests = await _unitOfWork.Repository<ServiceRequest>()
            .FindAsync(x => x.CitizenNationalId == nationalId);

        return requests.Select(ToDto).ToList();
    }

    public async Task<ServiceRequestDto?> GetByRequestNumberAsync(string requestNumber)
    {
        var request = await _unitOfWork.Repository<ServiceRequest>()
            .GetAsync(x => x.RequestNumber == requestNumber);

        return request == null ? null : ToDto(request);
    }

    public async Task<ServiceRequestDto> UpdateStatusAsync(string requestNumber, RequestStatus status)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();

        var request = await repo.GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("????? ??? ?????.", "REQUEST_NOT_FOUND");

        request.Status = status;
        request.LastUpdatedAt = DateTime.UtcNow;

        repo.Update(request);
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }

    public async Task<ServiceRequest> CreateAsync(
    ServiceType serviceType,
    int referenceId,
    RequestStatus status,
    string citizenNationalId)
    {
        if (string.IsNullOrWhiteSpace(citizenNationalId))
            citizenNationalId = _currentUser.NationalId
                ?? throw new AppEx.ValidationException("??? ?????? ??? ?????.", "AUTH_MISSING_NATIONAL_ID");

        var request = new ServiceRequest
        {
            RequestNumber = await _generator.GenerateAsync(serviceType), 
            CitizenNationalId = citizenNationalId,
            ServiceType = serviceType,
            Status = status,
            ReferenceId = referenceId,
            PaymentStatus = PaymentStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
        await _unitOfWork.CommitAsync();

        return request;
    }

    public async Task<ServiceRequestDto> MarkAsPaidAsync(string requestNumber, string transactionId, decimal amount)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();

        var request = await repo.GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("????? ??? ?????.", "REQUEST_NOT_FOUND");

        request.PaymentStatus = PaymentStatus.Paid;
        request.PaymentTransactionId = transactionId;
        request.PaymentAmount = amount;
        request.PaymentTimestamp = DateTime.UtcNow;
        request.Status = RequestStatus.ReadyForProcessing;
        request.LastUpdatedAt = DateTime.UtcNow;

        repo.Update(request);
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }
}
