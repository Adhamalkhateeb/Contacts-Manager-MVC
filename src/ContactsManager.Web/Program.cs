using ContactsManager.Application;
using ContactsManager.Infrastructure;
using ContactsManager.Web.StartupExtensions;
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

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

if (!app.Environment.IsEnvironment("Test"))
{
    app.UseSerilogRequestLogging();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(name: "default", pattern: "{controller=Persons}/{action=Index}/{id?}");
app.MapControllers();

app.Run();

public partial class Program { }
