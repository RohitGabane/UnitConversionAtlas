using UnitConversionApi.Application;
using UnitConversionApi.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUnitCatalog, UnitCatalog>();
builder.Services.AddSingleton<IConversionService, ConversionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
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

app.MapControllers();

app.Run();
