namespace CardPaymentApi.Models;

/// <summary>
/// Request to create a SetupIntent for saving a card.
/// </summary>
public class CreateSetupIntentRequest
{
    /// <summary>
    /// The customer ID to attach the payment method to.
    /// </summary>
    public required string CustomerId { get; set; }
}

/// <summary>
/// Response from creating a SetupIntent.
/// </summary>
public class SetupIntentResponse
{
    public required string Id { get; set; }
    public required string ClientSecret { get; set; }
    public required string Status { get; set; }
    public string? CustomerId { get; set; }
    public string? PaymentMethodId { get; set; }
}

/// <summary>
/// Request to confirm a SetupIntent with a payment method.
/// </summary>
public class ConfirmSetupIntentRequest
{
    public required string SetupIntentId { get; set; }
    public required string PaymentMethodId { get; set; }
}

/// <summary>
/// Card details for creating a payment method.
/// </summary>
public class CardDetailsRequest
{
    /// <summary>
    /// Card number (e.g., "4242424242424242")
    /// </summary>
    public required string Number { get; set; }

    /// <summary>
    /// Expiration month (1-12)
    /// </summary>
    public required int ExpMonth { get; set; }

    /// <summary>
    /// Expiration year (e.g., 2027)
    /// </summary>
    public required int ExpYear { get; set; }

    /// <summary>
    /// Card verification code
    /// </summary>
    public required string Cvc { get; set; }

    /// <summary>
    /// Cardholder name (optional)
    /// </summary>
    public string? CardholderName { get; set; }
}

/// <summary>
/// Request to create a payment method from card details.
/// </summary>
public class CreatePaymentMethodRequest
{
    public required CardDetailsRequest Card { get; set; }

    /// <summary>
    /// Customer ID to attach the payment method to (optional)
    /// </summary>
    public string? CustomerId { get; set; }
}

/// <summary>
/// Response from creating a payment method.
/// </summary>
public class PaymentMethodResponse
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required CardInfo Card { get; set; }
    public string? CustomerId { get; set; }
}

public class CardInfo
{
    public required string Brand { get; set; }
    public required string Last4 { get; set; }
    public required int ExpMonth { get; set; }
    public required int ExpYear { get; set; }
}

/// <summary>
/// Response listing saved payment methods for a customer.
/// </summary>
public class CustomerPaymentMethodsResponse
{
    public required string CustomerId { get; set; }
    public required List<PaymentMethodResponse> PaymentMethods { get; set; }
}
