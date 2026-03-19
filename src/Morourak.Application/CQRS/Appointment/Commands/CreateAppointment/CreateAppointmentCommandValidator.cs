using FluentValidation;

namespace Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    private static readonly TimeOnly WorkStart = new(9, 0);
    private static readonly TimeOnly WorkEnd = new(14, 0);

    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("رقم الهوية مطلوب.")
            .MaximumLength(32).WithMessage("رقم الهوية غير صحيح.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("نوع الخدمة مطلوب.");

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly)).WithMessage("التاريخ مطلوب.")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("لا يمكن عرض مواعيد لتاريخ سابق.");

        RuleFor(x => x.Time)
            .NotEqual(default(TimeOnly)).WithMessage("الوقت مطلوب.")
            .Must(t => t >= WorkStart && t < WorkEnd)
            .WithMessage("الموعد خارج ساعات العمل الرسمية.");

        RuleFor(x => x.GovernorateId)
            .GreaterThan(0).WithMessage("المحافظة مطلوبة.");

        RuleFor(x => x.TrafficUnitId)
            .GreaterThan(0).WithMessage("وحدة المرور مطلوبة.");
    }
}

