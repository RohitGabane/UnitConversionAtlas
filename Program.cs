using UnitConversionApi.Application;
using UnitConversionApi.Domain;
using UnitConversionApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUnitCatalog, UnitCatalog>();
builder.Services.AddSingleton<IConversionService, ConversionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", (IHostEnvironment environment) => environment.IsDevelopment()
    ? Results.Redirect("/swagger")
    : Results.Ok(new
    {
        name = "Unit Conversion API",
        convert = "POST /api/conversions",
        units = "GET /api/units"
    }));

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Allows ASP.NET Core integration tests to create the application in memory.
public partial class Program { }
