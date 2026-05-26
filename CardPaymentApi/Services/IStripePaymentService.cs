using CardPaymentApi.Models;

namespace CardPaymentApi.Services;

public interface IStripePaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentIntentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentIntentId);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<RefundResponse> RefundAsync(RefundRequest request);

    // SetupIntent methods for saving cards
    Task<SetupIntentResponse> CreateSetupIntentAsync(CreateSetupIntentRequest request);
    Task<SetupIntentResponse> ConfirmSetupIntentAsync(ConfirmSetupIntentRequest request);

    // PaymentMethod methods
    Task<PaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);
    Task<CustomerPaymentMethodsResponse> GetCustomerPaymentMethodsAsync(string customerId);
}
