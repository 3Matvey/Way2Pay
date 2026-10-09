namespace Way2Pay.Payments.Application.Payments.Providers;

/// <summary>The provider-confirmed outcome, independent of the payment's financial state.</summary>
public enum ProviderOperationOutcome
{
    /// <summary>The requested monetary action was confirmed as successful.</summary>
    Succeeded,

    /// <summary>The provider confirmed that the attempt had no monetary effect.</summary>
    Failed,

    /// <summary>The monetary outcome could not be established; another attempt is unsafe.</summary>
    Unknown
}
