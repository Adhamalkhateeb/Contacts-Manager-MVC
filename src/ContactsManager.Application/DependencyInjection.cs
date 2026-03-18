using System.Reflection;
using ContactsManager.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        return services;
    }
}
