using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Common.Results.Abstractions;
using FluentValidation;
using MediatR;

namespace ContactsManager.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures.ConvertAll(error =>
            Error.Validation(error.PropertyName, error.ErrorMessage)
        );

        return (dynamic)errors;
    }
}
