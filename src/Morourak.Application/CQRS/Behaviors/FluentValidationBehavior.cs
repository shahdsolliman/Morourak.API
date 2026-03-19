using FluentValidation;
using FluentValidation.Results;
using MediatR;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.CQRS.Behaviors;

public sealed class FluentValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var validationContext = new ValidationContext<TRequest>(request);

        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(validationContext, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return await next();

        var details = failures
            .Select(f => new AppEx.ErrorDetail
            {
                Field = string.IsNullOrWhiteSpace(f.PropertyName) ? "request" : f.PropertyName,
                Error = f.ErrorMessage
            })
            .ToList();

        throw new AppEx.ValidationException(
            "بيانات غير صالحة.",
            "VALIDATION_ERROR",
            details);
    }
}

