using CardPaymentApi.Models;
using CardPaymentApi.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CardPaymentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IStripePaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IStripePaymentService paymentService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Creates a PaymentIntent for a credit or debit card charge.
    /// The returned client_secret is used by Stripe.js on the frontend to collect card details.
    /// </summary>
    [HttpPost("intent")]
    [ProducesResponseType(typeof(PaymentIntentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than zero." });

        var result = await _paymentService.CreatePaymentIntentAsync(request);
        return CreatedAtAction(nameof(GetPaymentStatus), new { paymentIntentId = result.Id }, result);
    }

    /// <summary>
    /// Confirms a PaymentIntent with a PaymentMethod ID obtained from Stripe.js.
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(PaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        var result = await _paymentService.ConfirmPaymentAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Returns the current status of a PaymentIntent.
    /// </summary>
    [HttpGet("{paymentIntentId}")]
    [ProducesResponseType(typeof(PaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentStatus(string paymentIntentId)
    {
        var result = await _paymentService.GetPaymentStatusAsync(paymentIntentId);
        return Ok(result);
    }

    /// <summary>
    /// Creates a Stripe Customer that can be reused across payments.
    /// </summary>
    [HttpPost("customer")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required." });

        var result = await _paymentService.CreateCustomerAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Issues a full or partial refund for a completed payment.
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentIntentId))
            return BadRequest(new { error = "PaymentIntentId is required." });

        var result = await _paymentService.RefundAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Stripe webhook endpoint — receives and processes events (payment_intent.succeeded, etc.).
    /// Configure the endpoint URL in the Stripe Dashboard and set Stripe:WebhookSecret in config.
    /// </summary>
    [HttpPost("webhook")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook()
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogWarning("Stripe:WebhookSecret is not configured — skipping signature verification.");
        }

        string json;
        using (var reader = new StreamReader(Request.Body))
            json = await reader.ReadToEndAsync();

        Event stripeEvent;
        try
        {
            stripeEvent = string.IsNullOrEmpty(webhookSecret)
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed.");
            return BadRequest(new { error = ex.Message });
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                var succeeded = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogInformation("PaymentIntent {Id} succeeded. Amount: {Amount} {Currency}",
                    succeeded?.Id, succeeded?.AmountReceived, succeeded?.Currency);
                break;

            case EventTypes.PaymentIntentPaymentFailed:
                var failed = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogWarning("PaymentIntent {Id} failed: {Message}",
                    failed?.Id, failed?.LastPaymentError?.Message);
                break;

            case EventTypes.ChargeRefunded:
                var refunded = stripeEvent.Data.Object as Charge;
                _logger.LogInformation("Charge {Id} refunded.", refunded?.Id);
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }

        return Ok();
    }
}
