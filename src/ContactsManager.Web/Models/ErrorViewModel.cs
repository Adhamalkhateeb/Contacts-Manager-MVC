namespace ContactsManager.Web.Models;

public sealed record ErrorViewModel
{
    public int StatusCode { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string IconClass { get; init; } = "fa-solid fa-triangle-exclamation";
    public string VariantClass { get; init; } = string.Empty;

    public static ErrorViewModel CreateDefault(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = "400 - Bad Request",
                Title = "Invalid request",
                Message =
                    "The request could not be processed. Please review your input and try again.",
                IconClass = "fa-solid fa-circle-exclamation",
            },
            StatusCodes.Status401Unauthorized => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ErrorCode = "401 - Unauthorized",
                Title = "Authentication required",
                Message = "You need to sign in before accessing this resource.",
                IconClass = "fa-solid fa-user-lock",
            },
            StatusCodes.Status403Forbidden => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ErrorCode = "403 - Forbidden",
                Title = "Access denied",
                Message = "You do not have permission to access this resource.",
                IconClass = "fa-solid fa-lock",
                VariantClass = "forbidden",
            },
            StatusCodes.Status404NotFound => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status404NotFound,
                ErrorCode = "404 - Not Found",
                Title = "Resource not found",
                Message = "The requested resource could not be found.",
                IconClass = "fa-solid fa-magnifying-glass",
                VariantClass = "not-found",
            },
            StatusCodes.Status409Conflict => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status409Conflict,
                ErrorCode = "409 - Conflict",
                Title = "Conflict detected",
                Message =
                    "The operation could not be completed because it conflicts with existing data.",
                IconClass = "fa-solid fa-code-compare",
            },
            _ => new ErrorViewModel
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "500 - Internal Server Error",
                Title = "Something went wrong",
                Message = "An unexpected error occurred while processing your request.",
                IconClass = "fa-solid fa-triangle-exclamation",
            },
        };
    }
}
