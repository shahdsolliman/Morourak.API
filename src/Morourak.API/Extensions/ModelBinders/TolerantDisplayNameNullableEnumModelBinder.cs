using Microsoft.AspNetCore.Mvc.ModelBinding;
using Morourak.API.Extensions.EnumParsing;

namespace Morourak.API.Extensions.ModelBinders;

/// <summary>
/// Binds nullable enum values from query/form/route using:
/// - Arabic <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute.Name"/>
/// - Enum member name (case-insensitive)
/// - Numeric values (only if defined)
///
/// If parsing fails, it returns <c>null</c> without adding a model-state error
/// to preserve backward compatibility for historically free-form string inputs.
/// </summary>
public sealed class TolerantDisplayNameNullableEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var modelType = bindingContext.ModelType;
        var enumType = Nullable.GetUnderlyingType(modelType);
        if (enumType == null || !enumType.IsEnum)
            return Task.CompletedTask;

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var raw = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(raw))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (EnumDisplayNameParser.TryParse(enumType, raw, out var parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }

        // Backward-compat: invalid values should not fail the request; treat as "not specified".
        bindingContext.Result = ModelBindingResult.Success(null);
        return Task.CompletedTask;
    }
}

