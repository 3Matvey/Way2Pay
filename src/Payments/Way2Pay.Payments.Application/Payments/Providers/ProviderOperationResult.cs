using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Application.Payments.Providers;

/// <summary>A provider outcome constructed through factories that enforce its required fields.</summary>
public sealed record ProviderOperationResult
{
    public ProviderOperationOutcome Outcome { get; }

    /// <summary>The transaction ID, including one supplied before the final outcome is known.</summary>
    public string? ProviderTransactionId { get; }

    /// <summary>A non-empty code for a confirmed failure; absent for success and Unknown.</summary>
    public string? FailureCode { get; }

    private ProviderOperationResult(ProviderOperationOutcome outcome, string? transactionId, string? failureCode)
    {
        Outcome = outcome;
        ProviderTransactionId = transactionId;
        FailureCode = failureCode;
    }

    /// <summary>Creates a confirmed success with the required provider transaction ID.</summary>
    public static ProviderOperationResult Succeeded(string providerTransactionId) =>
        new(ProviderOperationOutcome.Succeeded, Guard.Required(providerTransactionId), null);

    /// <summary>Creates a confirmed failure without monetary effect, optionally retaining a transaction ID.</summary>
    public static ProviderOperationResult Failed(string failureCode, string? providerTransactionId = null) =>
        new(ProviderOperationOutcome.Failed, ValidateOptionalId(providerTransactionId), Guard.Required(failureCode));

    /// <summary>Creates an unresolved outcome, optionally retaining a transaction ID for reconciliation.</summary>
    public static ProviderOperationResult Unknown(string? providerTransactionId = null) =>
        new(ProviderOperationOutcome.Unknown, ValidateOptionalId(providerTransactionId), null);

    private static string? ValidateOptionalId(string? transactionId) =>
        transactionId is null ? null : Guard.Required(transactionId);
}
