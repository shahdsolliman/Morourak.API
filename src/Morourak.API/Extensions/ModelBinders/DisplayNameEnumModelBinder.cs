using Microsoft.AspNetCore.Mvc.ModelBinding;
using Morourak.API.Extensions.EnumParsing;

namespace Morourak.API.Extensions.ModelBinders;

public class DisplayNameEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var raw = valueResult.FirstValue;
        var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
        var enumType = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;

        if (string.IsNullOrWhiteSpace(raw))
        {
            if (isNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "هذا الحقل مطلوب.");
            return Task.CompletedTask;
        }

        if (!EnumDisplayNameParser.TryParse(enumType, raw!, out var parsed))
        {
            var allowed = EnumDisplayNameParser.GetAllowedValuesForError(enumType);
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"قيمة غير صحيحة. القيم المسموحة: {allowed}.");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(parsed);
        return Task.CompletedTask;
    }
}
