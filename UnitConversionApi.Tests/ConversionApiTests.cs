using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UnitConversionApi.Application;
using UnitConversionApi.Models;
using Xunit;

namespace UnitConversionApi.Tests;

public sealed class ConversionApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData(100, "km", "mi", 62.1371)]
    [InlineData(25, "C", "F", 77)]
    [InlineData(10, "lb", "kg", 4.5359237)]
    [InlineData(12.5, "m", "m", 12.5)]
    [InlineData(1, "KM", "m", 1_000)]
    public async Task Convert_returns_expected_value(
        double value,
        string fromUnit,
        string toUnit,
        double expected)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversions",
            new ConversionRequest(value, fromUnit, toUnit));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        Assert.NotNull(body);
        Assert.Equal(expected, body.Result, precision: 4);
    }

    [Fact]
    public async Task Unknown_unit_returns_problem_details()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversions",
            new ConversionRequest(1, "foo", "m"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("foo", problem.Detail);
    }

    [Fact]
    public async Task Different_categories_return_bad_request()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversions",
            new ConversionRequest(1, "m", "kg"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Missing_value_is_rejected()
    {
        using var content = JsonContent.Create(new { fromUnit = "m", toUnit = "cm" });
        var response = await _client.PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Non_finite_json_number_is_rejected()
    {
        using var content = new StringContent(
            "{\"value\":1e309,\"fromUnit\":\"m\",\"toUnit\":\"cm\"}",
            System.Text.Encoding.UTF8,
            "application/json");
        var response = await _client.PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Overflowing_conversion_returns_unprocessable_entity()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversions",
            new ConversionRequest(1e308, "km", "m"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Conversion result out of range", problem.Title);
    }

    [Fact]
    public async Task Units_endpoint_lists_required_categories()
    {
        var response = await _client.GetAsync("/api/units");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("length", out _));
        Assert.True(body.RootElement.TryGetProperty("temperature", out _));
        Assert.True(body.RootElement.TryGetProperty("mass", out _));
    }

    [Fact]
    public async Task Swagger_ui_is_available_in_development()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Swagger UI", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Unexpected_exception_returns_safe_server_error()
    {
        using var factory = new ThrowingConversionFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/conversions",
            new ConversionRequest(1, "m", "cm"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("unexpected error", problem.Title, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal secret", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ThrowingConversionFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IConversionService>();
                services.AddSingleton<IConversionService, ThrowingConversionService>();
            });
        }
    }

    private sealed class ThrowingConversionService : IConversionService
    {
        public double Convert(double value, string fromUnit, string toUnit) =>
            throw new InvalidOperationException("internal secret");
    }
}
