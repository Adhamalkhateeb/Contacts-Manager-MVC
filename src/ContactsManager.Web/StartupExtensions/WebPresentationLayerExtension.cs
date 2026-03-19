using ContactsManager.Web.Filters.ActionFilters;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Rotativa.AspNetCore;

namespace ContactsManager.Web.StartupExtensions;

/// <summary>
/// Extension methods for configuring the Web presentation layer services
/// </summary>
public static class WebPresentationLayerExtension
{
    public static IServiceCollection AddWebPresentationLayer(this IServiceCollection services)
    {
        services.AddTransient<PersonsPostActionFilter>();
        services.AddTransient<HandleExceptionFilter>();
        RotativaConfiguration.Setup("wwwroot", "Rotativa");
        ExcelPackage.License.SetNonCommercialPersonal("Adham Fawzy");

        return services;
    }
}
