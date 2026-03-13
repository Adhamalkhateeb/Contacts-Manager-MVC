using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class HandleExceptionFilterFactory : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<HandleExceptionFilter>();
    }
}

public class HandleExceptionFilter(
    ILogger<HandleExceptionFilter> logger,
    IHostEnvironment environment
) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        logger.LogError(context.Exception, "Unhandled exception occurred");

        if (environment.IsDevelopment() || environment.IsStaging())
        {
            context.Result = new ContentResult
            {
                Content = context.Exception.ToString(),
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
        else
        {
            context.Result = new ObjectResult(new { message = "An unexpected error occurred." })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }

        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
