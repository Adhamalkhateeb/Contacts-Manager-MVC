using System;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Common.Behaviors;

/// <summary>
/// Logs unhandled exceptions but allows them to propagate
/// Note: Handlers should use Result pattern to return errors instead of throwing exceptions
/// </summary>
public class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        try
        {
            return await next(ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request {Name} was cancelled", typeof(TRequest).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception for Request {Name}", typeof(TRequest).Name);
            throw;
        }
    }
}
