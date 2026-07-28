using ContactsManager.Web.Filters.ActionFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
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
        services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            options.Filters.Add(new AuthorizeFilter(policy));
        });

        services.AddTransient<PersonsPostActionFilter>();
        try
        {
            RotativaConfiguration.Setup("wwwroot", "Rotativa");
        }
        catch
        {
            // wkhtmltopdf may be unavailable in test environments; PDF endpoints will be unavailable.
        }
        ExcelPackage.License.SetNonCommercialPersonal("Adham Fawzy");

        return services;
    }
}
