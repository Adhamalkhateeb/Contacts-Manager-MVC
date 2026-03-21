using System;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Infrastructure.Data;
using ContactsManager.Infrastructure.Data.Interceptors;
using ContactsManager.Infrastructure.Identity;
using ContactsManager.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ContactsManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddSingleton(TimeProvider.System);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        if (!environment.IsEnvironment("Test"))
        {
            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseSqlServer(connectionString);
                }
            );
        }

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IPersonExportService, PersonExportService>();
        services.AddScoped<ICountryImportService, CountryImportService>();

        services.AddScoped<IIdentityService, IdentityService>();

        services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

        return services;
    }
}
