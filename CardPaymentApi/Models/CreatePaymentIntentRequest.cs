namespace CardPaymentApi.Models;

public class CreatePaymentIntentRequest
{
    public long Amount { get; set; }           // in smallest currency unit (e.g. cents)
    public string Currency { get; set; } = "usd";
    public string? CustomerId { get; set; }    // optional saved Stripe customer
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
