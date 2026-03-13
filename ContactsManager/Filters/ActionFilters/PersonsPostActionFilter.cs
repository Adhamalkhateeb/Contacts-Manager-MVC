using ContactsManager.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog.Core;
using ServiceContracts;

public class PersonPostFilterFactoryAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var filter = serviceProvider.GetRequiredService<PersonsPostActionFilter>();

        return filter;
    }
}

public class PersonsPostActionFilter(
    ICountriesService countriesService,
    ILogger<PersonsPostActionFilter> logger
) : IAsyncActionFilter, IOrderedFilter
{
    public int Order { get; }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        if (context.Controller is PersonsController controller)
        {
            if (!controller.ModelState.IsValid)
            {
                var errors = controller
                    .ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                logger.LogWarning("Invalid Person POST request. Errors: {@Errors}", errors);

                var countries = await countriesService.GetAllAsync();

                controller.ViewBag.Countries = countries
                    .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                    .ToList();

                controller.ViewBag.Errors = errors;

                if (context.ActionArguments.TryGetValue("request", out var personRequest))
                {
                    context.Result = controller.View(personRequest);
                }

                return;
            }
        }
        await next();
    }
}
