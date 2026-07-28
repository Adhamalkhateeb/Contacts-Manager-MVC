using ContactsManager.Application;
using ContactsManager.Infrastructure;
using ContactsManager.Infrastructure.Identity;
using ContactsManager.Web.StartupExtensions;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Host.UseSerilog(
        (context, services, loggingConfig) =>
        {
            loggingConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
        }
    );
}

builder
    .Services.AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddWebPresentationLayer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    await RoleSeeder.SeedAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await AdminSeeder.SeedAsync(userManager, seederLogger);
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseHsts();
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

if (!app.Environment.IsEnvironment("Test"))
    app.UseSerilogRequestLogging();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Users}/{action=Index}/{id?}"
);
app.MapControllerRoute(name: "default", pattern: "{controller=Persons}/{action=Index}/{id?}");
app.MapControllers();

app.Run();

public partial class Program { }
