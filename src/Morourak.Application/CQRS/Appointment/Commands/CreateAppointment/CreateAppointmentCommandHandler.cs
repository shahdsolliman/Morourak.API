using AutoMapper;
using MediatR;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.Interfaces.DomainServices;

namespace Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandHandler
    : IRequestHandler<CreateAppointmentCommand, BookingConfirmationDto>
{
    private readonly IAppointmentDomainService _appointmentDomainService;
    private readonly IMapper _mapper;

    public CreateAppointmentCommandHandler(
        IAppointmentDomainService appointmentDomainService,
        IMapper mapper)
    {
        _appointmentDomainService = appointmentDomainService;
        _mapper = mapper;
    }

    public async Task<BookingConfirmationDto> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var context = await _appointmentDomainService.ConfirmBookingAsync(
            nationalId: request.NationalId,
            serviceType: request.ServiceType,
            date: request.Date,
            time: request.Time,
            governorateId: request.GovernorateId,
            trafficUnitId: request.TrafficUnitId,
            cancellationToken: cancellationToken);

        return _mapper.Map<BookingConfirmationDto>(context);
    }
}

