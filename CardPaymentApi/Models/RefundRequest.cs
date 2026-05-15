namespace CardPaymentApi.Models;

public class RefundRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public long? Amount { get; set; }  // null means full refund
    public string? Reason { get; set; }
}
