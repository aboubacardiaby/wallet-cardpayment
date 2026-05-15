namespace CardPaymentApi.Models;

public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
}
