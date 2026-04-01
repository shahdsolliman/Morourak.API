using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Morourak.API.Extensions.ModelBinders;

public class DisplayNameEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var modelType = context.Metadata.ModelType;
        var enumType = Nullable.GetUnderlyingType(modelType) ?? modelType;

        if (!enumType.IsEnum)
            return null;

        return new DisplayNameEnumModelBinder();
    }
}

