using Stripe;
using System.Net;
using System.Text.Json;

namespace CardPaymentApi.Middleware;

public class StripeExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StripeExceptionMiddleware> _logger;

    public StripeExceptionMiddleware(RequestDelegate next, ILogger<StripeExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error: {Code} — {Message}", ex.StripeError?.Code, ex.Message);
            context.Response.StatusCode = (int)MapStripeErrorToHttpStatus(ex);
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new
            {
                error = ex.StripeError?.Code ?? "stripe_error",
                message = ex.Message,
                declineCode = ex.StripeError?.DeclineCode,
            });
            await context.Response.WriteAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "internal_error", message = "An unexpected error occurred." }));
        }
    }

    private static HttpStatusCode MapStripeErrorToHttpStatus(StripeException ex) =>
        ex.StripeError?.Type switch
        {
            "card_error" => HttpStatusCode.PaymentRequired,
            "invalid_request_error" => HttpStatusCode.BadRequest,
            "authentication_error" => HttpStatusCode.Unauthorized,
            "rate_limit_error" => (HttpStatusCode)429,
            _ => HttpStatusCode.BadGateway,
        };
}
