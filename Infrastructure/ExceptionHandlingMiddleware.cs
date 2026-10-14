using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Domain;

namespace UnitConversionApi.Infrastructure;

/// <summary>Converts expected domain errors and unexpected failures into consistent HTTP responses.</summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request was cancelled by the client: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(exception, "An exception occurred after the response started.");
                throw;
            }

            var (statusCode, title, detail) = exception switch
            {
                UnknownUnitException or IncompatibleUnitsException =>
                    (StatusCodes.Status400BadRequest, "Invalid conversion", exception.Message),
                ConversionOutOfRangeException =>
                    (StatusCodes.Status422UnprocessableEntity, "Conversion result out of range", exception.Message),
                _ =>
                    (StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred",
                        "The server could not complete the request.")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled error processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                logger.LogInformation("Rejected request {Method} {Path}: {Message}",
                    context.Request.Method, context.Request.Path, exception.Message);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
        }
        finally
        {
            stopwatch.Stop();
            if (context.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("HTTP {Method} {Path} was cancelled after {ElapsedMilliseconds} ms",
                    context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
