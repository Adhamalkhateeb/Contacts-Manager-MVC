using ContactsManager.Application.Features.Countries.Queries.GetCountries;
using ContactsManager.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContactsManager.Web.Filters.ActionFilters;

public class PersonPostFilterFactory : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<PersonsPostActionFilter>();
    }
}

public class PersonsPostActionFilter(IMediator mediator, ILogger<PersonsPostActionFilter> logger)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var controller = context.Controller as PersonsController;
        if (controller != null)
        {
            if (!controller.ModelState.IsValid)
            {
                PopulateErrors(controller);

                await PopulateCountriesAsync(controller, context.HttpContext.RequestAborted);

                if (context.ActionArguments.TryGetValue("request", out var personRequest))
                    context.Result = controller.View(personRequest);

                return;
            }
        }

        var executedContext = await next();

        if (
            controller != null
            && !controller.ModelState.IsValid
            && executedContext.Result is ViewResult
        )
        {
            PopulateErrors(controller);
            await PopulateCountriesAsync(controller, context.HttpContext.RequestAborted);
        }
    }

    private async Task PopulateCountriesAsync(
        PersonsController controller,
        CancellationToken cancellationToken
    )
    {
        var countriesResult = await mediator.Send(new GetCountriesQuery(), cancellationToken);

        controller.ViewBag.Countries = countriesResult.Match(
            countries =>
                countries
                    .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                    .ToList(),
            _ => new List<SelectListItem>()
        );
    }

    private void PopulateErrors(PersonsController controller)
    {
        var errors = controller
            .ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        logger.LogWarning("Validation errors occurred: {@Errors}", errors);

        controller.ViewBag.Errors = errors;
    }
}
