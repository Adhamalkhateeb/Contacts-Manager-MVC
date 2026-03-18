using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoriesContract;
using ServiceContracts;
using Services;

public static class ConfigureServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<IPersonsRepository, PersonsRepository>();
        services.AddScoped<ICountryCommandService, CountryCommandService>();
        services.AddScoped<ICountryQueryService, CountryQueryService>();
        services.AddScoped<IPersonCommandService, PersonCommandService>();
        services.AddScoped<IPersonQueryService, PersonQueryService>();
        services.AddTransient<PersonsPostActionFilter>();
        services.AddTransient<HandleExceptionFilter>();

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var constr = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(constr))
            {
                throw new InvalidOperationException();
            }

            options.UseSqlServer(constr);
        });

        return services;
    }
}
