using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Appointments;
using Morourak.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Morourak.Application.Services.Licenses
{
    public class DrivingLicenseResultService : IDrivingLicenseResultService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DrivingLicenseResultService> _logger;

        public DrivingLicenseResultService(IUnitOfWork unitOfWork, ILogger<DrivingLicenseResultService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes)
        {
            _logger.LogInformation("Submitting appointment result for Application: {AppId}, Type: {Type}, Passed: {Passed}", applicationId, type, passed);

            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = (await repo.FindAsync(a => a.ApplicationId == applicationId && a.Type == type))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found for Application: {AppId}, Type: {Type}", applicationId, type);
                throw new ValidationException($"موعد {type} غير موجود.", "APPOINTMENT_NOT_FOUND");
            }

            appointment.Status = passed ? AppointmentStatus.Passed : AppointmentStatus.Failed;
            repo.Update(appointment);

            var applicationRepo = _unitOfWork.Repository<DrivingLicenseApplication>();
            var application = await applicationRepo.GetAsync(a => a.Id == applicationId);

            if (application == null) throw new ValidationException("الطلب غير موجود.", "APPLICATION_NOT_FOUND");

            if (type == AppointmentType.Medical) application.MedicalExaminationPassed = passed;
            if (type == AppointmentType.Driving) application.DrivingTestPassed = passed;

            applicationRepo.Update(application);
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully updated results for Application: {AppId}", applicationId);
        }
    }
}
