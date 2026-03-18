using ContactsManager;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggingConfig) =>
    {
        loggingConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
    }
);

builder.Services.AddControllersWithViews();
builder.Services.AddServices();

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDatabase(builder.Configuration);
    RotativaConfiguration.Setup("wwwroot", "Rotativa");
}

ExcelPackage.License.SetNonCommercialPersonal("Adham Fawzy");

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }
