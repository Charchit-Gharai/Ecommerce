using ECommerce.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ECommerce.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InsufficientStockException ex)
        {
            await WriteErrorAsync(context,
                StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context,
                StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteErrorAsync(context,
                StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorAsync(context,
                StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entries = ex.Entries.Select(entry =>
            {
                var primaryKey = entry.Metadata.FindPrimaryKey();
                var keyValues = primaryKey is null
                    ? "primary key unavailable"
                    : string.Join(", ", primaryKey.Properties.Select(property =>
                        $"{property.Name}={entry.Property(property.Name).CurrentValue}"));

                return $"{entry.Metadata.ClrType.Name} " +
                       $"(state: {entry.State}; {keyValues})";
            });

            logger.LogError(ex,
                "Database concurrency conflict for {Path}. Affected entries: {Entries}",
                context.Request.Path,
                string.Join("; ", entries));

            await WriteErrorAsync(context,
                StatusCodes.Status409Conflict,
                "The data changed while processing your request. Refresh and try again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unhandled API error for {Path}", context.Request.Path);

            await WriteErrorAsync(context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { success = false, message }));
    }
}