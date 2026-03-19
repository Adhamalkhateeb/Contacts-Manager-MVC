using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class FeatureDisableResourceFilter(
    ILogger<FeatureDisableResourceFilter> logger,
    bool isDisabled
) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next
    )
    {
        logger.LogInformation(
            "{FilterName}.{MethodName} - before",
            nameof(FeatureDisableResourceFilter),
            nameof(OnResourceExecutionAsync)
        );

        if (isDisabled)
            context.Result = new NotFoundResult();

        await next();

        logger.LogInformation(
            "{FilterName}.{MethodName} - after",
            nameof(FeatureDisableResourceFilter),
            nameof(OnResourceExecutionAsync)
        );
    }
}
