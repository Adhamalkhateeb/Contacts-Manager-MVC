using Microsoft.AspNetCore.Mvc.Filters;

public class PersonsAlwaysRunResultFilter : IAsyncAlwaysRunResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next
    )
    {
        if (context.Filters.OfType<SkipFilterAttribute>().Any())
            return;
        await next();
    }
}
