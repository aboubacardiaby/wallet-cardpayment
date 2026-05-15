using CardPaymentApi.Models;

namespace CardPaymentApi.Services;

public interface IStripePaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentIntentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentIntentId);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<RefundResponse> RefundAsync(RefundRequest request);
}
