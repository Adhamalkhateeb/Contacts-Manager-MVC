using Entities;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Repositories;
using RepositoriesContract;
using Rotativa.AspNetCore;
using Serilog;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggingConfig) =>
    {
        loggingConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
    }
);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var constr = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(constr))
        {
            throw new InvalidOperationException();
        }

        options.UseSqlServer(constr);
    });

    RotativaConfiguration.Setup("wwwroot", "Rotativa");
}

ExcelPackage.License.SetNonCommercialPersonal("Adham Fawzy");

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }
