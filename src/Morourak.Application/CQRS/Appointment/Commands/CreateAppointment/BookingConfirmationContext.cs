using AppointmentEntity = Morourak.Domain.Entities.Appointment;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;

public sealed class BookingConfirmationContext
{
    public BookingConfirmationContext(
        AppointmentEntity appointment,
        ServiceRequest serviceRequest,
        Governorate governorate,
        TrafficUnit trafficUnit,
        AppointmentType appointmentType,
        string assignedToUserId)
    {
        Appointment = appointment;
        ServiceRequest = serviceRequest;
        Governorate = governorate;
        TrafficUnit = trafficUnit;
        AppointmentType = appointmentType;
        AssignedToUserId = assignedToUserId;
    }

    public AppointmentEntity Appointment { get; }
    public ServiceRequest ServiceRequest { get; }
    public Governorate Governorate { get; }
    public TrafficUnit TrafficUnit { get; }

    public AppointmentType AppointmentType { get; }
    public string AssignedToUserId { get; }
}

