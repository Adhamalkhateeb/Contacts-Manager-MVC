using ContactsManager.Application;
using ContactsManager.Infrastructure;
using ContactsManager.Web.Middlewares;
using ContactsManager.Web.StartupExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggingConfig) =>
    {
        loggingConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
    }
);

builder
    .Services.AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebPresentationLayer();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(name: "default", pattern: "{controller=Persons}/{action=Index}/{id?}");
app.MapControllers();

app.Run();

partial class Program { }
