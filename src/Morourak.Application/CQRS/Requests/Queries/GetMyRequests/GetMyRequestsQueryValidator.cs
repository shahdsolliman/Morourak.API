using FluentValidation;
using Morourak.Application.DTOs.Common;

namespace Morourak.Application.CQRS.Requests.Queries.GetMyRequests;

public sealed class GetMyRequestsQueryValidator : AbstractValidator<GetMyRequestsQuery>
{
    public GetMyRequestsQueryValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("رقم الهوية مطلوب.")
            .MaximumLength(32).WithMessage("رقم الهوية غير صحيح.");

        RuleFor(x => x.Pagination)
            .NotNull().WithMessage("بيانات التصفّح مطلوبة.");

        RuleFor(x => x.Pagination!.PageNumber)
            .GreaterThan(0).WithMessage("رقم الصفحة غير صالح.");

        RuleFor(x => x.Pagination!.PageSize)
            .InclusiveBetween(1, PaginationParams.MaxPageSize)
            .WithMessage($"حجم الصفحة يجب أن يكون بين 1 و{PaginationParams.MaxPageSize}.");
    }
}

