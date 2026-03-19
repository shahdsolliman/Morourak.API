using MediatR;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.DomainServices;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Entities;

namespace Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;

public sealed class GetMyAppointmentDetailsQueryHandler
    : IRequestHandler<GetMyAppointmentDetailsQuery, AppointmentDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyAppointmentDetailsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppointmentDetailsDto?> Handle(
        GetMyAppointmentDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var appointmentRepo = _unitOfWork.Repository<Morourak.Domain.Entities.Appointment>();

        var appointment = await appointmentRepo.GetAsync(
            a => a.Id == request.AppointmentId && a.CitizenNationalId == request.NationalId,
            a => a.Governorate!,
            a => a.TrafficUnit!);

        if (appointment == null)
            return null;

        return new AppointmentDetailsDto
        {
            AppointmentId = appointment.Id,
            ServiceName = appointment.Type.ToString(),
            Status = appointment.Status.ToString(),
            Date = appointment.Date.ToString("yyyy-MM-dd"),

            StartTime = appointment.StartTime.ToString("HH:mm"),
            EndTime = appointment.EndTime.ToString("HH:mm"),

            GovernorateName = appointment.Governorate?.Name ?? string.Empty,
            TrafficUnitName = appointment.TrafficUnit?.Name ?? string.Empty,

            RequestNumberRelated = appointment.RequestNumber,
            AssignedToUserId = ResolveAssignedToUserId(appointment.Type)
        };
    }

    private static string ResolveAssignedToUserId(AppointmentType appointmentType)
    {
        return appointmentType switch
        {
            AppointmentType.Medical => "DOCTOR",
            AppointmentType.Technical => "INSPECTOR",
            AppointmentType.Driving => "EXAMINATOR",
            _ => "STAFF"
        };
    }
}

