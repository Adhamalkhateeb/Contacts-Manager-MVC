using ContactsManager.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;

public class PersonsPostActionFilter(ICountriesService countriesService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        if (context.Controller is PersonsController controller)
        {
            if (!controller.ModelState.IsValid)
            {
                var countries = await countriesService.GetAllAsync();

                controller.ViewBag.Countries = countries
                    .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                    .ToList();

                controller.ViewBag.Errors = controller
                    .ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var personRequest = context.ActionArguments["request"];

                context.Result = controller.View(personRequest);
                return;
            }
        }
        await next();
    }
}
