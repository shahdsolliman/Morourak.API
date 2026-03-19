using FluentValidation;

namespace Morourak.Application.CQRS.Appointment.Queries.GetMyAppointments;

public sealed class GetMyAppointmentDetailsQueryValidator
    : AbstractValidator<GetMyAppointmentDetailsQuery>
{
    public GetMyAppointmentDetailsQueryValidator()
    {
        RuleFor(x => x.AppointmentId)
            .GreaterThan(0).WithMessage("معرف الموعد غير صحيح.");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("رقم الهوية مطلوب.")
            .MaximumLength(32).WithMessage("رقم الهوية غير صحيح.");
    }
}

