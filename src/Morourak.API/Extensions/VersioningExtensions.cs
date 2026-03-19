using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Morourak.API.Extensions;

public class ApiVersionConvention : IControllerModelConvention
{
    private readonly AttributeRouteModel _centralPrefix;

    public ApiVersionConvention(IRouteTemplateProvider routeTemplateProvider)
    {
        _centralPrefix = new AttributeRouteModel(routeTemplateProvider);
    }

    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix, selector.AttributeRouteModel);
            }
            else
            {
                selector.AttributeRouteModel = _centralPrefix;
            }
        }
    }
}

public static class VersioningExtensions
{
    public static void AddGlobalApiVersioning(this MvcOptions options, string prefix)
    {
        options.Conventions.Insert(0, (IApplicationModelConvention)new ApiVersionConvention(new RouteAttribute(prefix)));
    }
}
