using CardPaymentApi.Models;
using Stripe;

namespace CardPaymentApi.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly PaymentIntentService _paymentIntentService;
    private readonly CustomerService _customerService;
    private readonly RefundService _refundService;
    private readonly SetupIntentService _setupIntentService;
    private readonly PaymentMethodService _paymentMethodService;

    public StripePaymentService(IConfiguration configuration)
    {
        var apiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        StripeConfiguration.ApiKey = apiKey;

        _paymentIntentService = new PaymentIntentService();
        _customerService = new CustomerService();
        _refundService = new RefundService();
        _setupIntentService = new SetupIntentService();
        _paymentMethodService = new PaymentMethodService();
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = request.Amount,
            Currency = request.Currency,
            Customer = request.CustomerId,
            Description = request.Description,
            // card and debit card both go through the "card" payment method type
            PaymentMethodTypes = ["card"],
            Metadata = request.Metadata,
        };

        var intent = await _paymentIntentService.CreateAsync(options);

        return MapToResponse(intent);
    }

    public async Task<PaymentIntentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        var options = new PaymentIntentConfirmOptions
        {
            PaymentMethod = request.PaymentMethodId,
        };

        var intent = await _paymentIntentService.ConfirmAsync(request.PaymentIntentId, options);

        return MapToResponse(intent);
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentIntentId)
    {
        var intent = await _paymentIntentService.GetAsync(paymentIntentId);

        return new PaymentStatusResponse
        {
            Id = intent.Id,
            Status = intent.Status,
            Amount = intent.Amount,
            AmountReceived = intent.AmountReceived,
            Currency = intent.Currency,
            PaymentMethodId = intent.PaymentMethodId,
            CustomerId = intent.CustomerId,
            Created = intent.Created,
        };
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var options = new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.Name,
            Phone = request.Phone,
            Metadata = request.Metadata,
        };

        var customer = await _customerService.CreateAsync(options);

        return new CustomerResponse
        {
            Id = customer.Id,
            Email = customer.Email,
            Name = customer.Name,
        };
    }

    public async Task<RefundResponse> RefundAsync(RefundRequest request)
    {
        // Resolve the charge id from the payment intent
        var intent = await _paymentIntentService.GetAsync(request.PaymentIntentId);
        var chargeId = intent.LatestChargeId
            ?? throw new InvalidOperationException("No charge found on the payment intent.");

        var validReasons = new HashSet<string> { "duplicate", "fraudulent", "requested_by_customer" };
        var options = new RefundCreateOptions
        {
            Charge = chargeId,
            Amount = request.Amount,
            Reason = (request.Reason != null && validReasons.Contains(request.Reason))
                ? request.Reason
                : null,
        };

        var refund = await _refundService.CreateAsync(options);

        return new RefundResponse
        {
            Id = refund.Id,
            Status = refund.Status,
            Amount = refund.Amount,
            Currency = refund.Currency,
        };
    }

    private static PaymentIntentResponse MapToResponse(PaymentIntent intent) => new()
    {
        Id = intent.Id,
        ClientSecret = intent.ClientSecret,
        Status = intent.Status,
        Amount = intent.Amount,
        Currency = intent.Currency,
        CustomerId = intent.CustomerId,
    };

    public async Task<SetupIntentResponse> CreateSetupIntentAsync(CreateSetupIntentRequest request)
    {
        var options = new SetupIntentCreateOptions
        {
            Customer = request.CustomerId,
            PaymentMethodTypes = ["card"],
        };

        var setupIntent = await _setupIntentService.CreateAsync(options);

        return new SetupIntentResponse
        {
            Id = setupIntent.Id,
            ClientSecret = setupIntent.ClientSecret,
            Status = setupIntent.Status,
            CustomerId = setupIntent.CustomerId,
            PaymentMethodId = setupIntent.PaymentMethodId,
        };
    }

    public async Task<SetupIntentResponse> ConfirmSetupIntentAsync(ConfirmSetupIntentRequest request)
    {
        var options = new SetupIntentConfirmOptions
        {
            PaymentMethod = request.PaymentMethodId,
        };

        var setupIntent = await _setupIntentService.ConfirmAsync(request.SetupIntentId, options);

        return new SetupIntentResponse
        {
            Id = setupIntent.Id,
            ClientSecret = setupIntent.ClientSecret,
            Status = setupIntent.Status,
            CustomerId = setupIntent.CustomerId,
            PaymentMethodId = setupIntent.PaymentMethodId,
        };
    }

    public async Task<PaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        var options = new PaymentMethodCreateOptions
        {
            Type = "card",
            Card = new PaymentMethodCardOptions
            {
                Number = request.Card.Number,
                ExpMonth = request.Card.ExpMonth,
                ExpYear = request.Card.ExpYear,
                Cvc = request.Card.Cvc,
            },
        };

        var paymentMethod = await _paymentMethodService.CreateAsync(options);

        // Attach to customer if specified
        if (!string.IsNullOrEmpty(request.CustomerId))
        {
            var attachOptions = new PaymentMethodAttachOptions
            {
                Customer = request.CustomerId,
            };
            paymentMethod = await _paymentMethodService.AttachAsync(paymentMethod.Id, attachOptions);
        }

        return MapToPaymentMethodResponse(paymentMethod);
    }

    public async Task<CustomerPaymentMethodsResponse> GetCustomerPaymentMethodsAsync(string customerId)
    {
        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card",
        };

        var paymentMethods = await _paymentMethodService.ListAsync(options);

        return new CustomerPaymentMethodsResponse
        {
            CustomerId = customerId,
            PaymentMethods = paymentMethods.Data.Select(MapToPaymentMethodResponse).ToList(),
        };
    }

    private static PaymentMethodResponse MapToPaymentMethodResponse(PaymentMethod pm) => new()
    {
        Id = pm.Id,
        Type = pm.Type,
        CustomerId = pm.CustomerId,
        Card = new Models.CardInfo
        {
            Brand = pm.Card.Brand,
            Last4 = pm.Card.Last4,
            ExpMonth = (int)pm.Card.ExpMonth,
            ExpYear = (int)pm.Card.ExpYear,
        },
    };
}
