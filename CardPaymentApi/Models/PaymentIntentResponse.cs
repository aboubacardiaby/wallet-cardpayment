namespace CardPaymentApi.Models;

public class PaymentIntentResponse
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
}

public class CustomerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class RefundResponse
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class PaymentStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long AmountReceived { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public string? CustomerId { get; set; }
    public DateTime Created { get; set; }
}
