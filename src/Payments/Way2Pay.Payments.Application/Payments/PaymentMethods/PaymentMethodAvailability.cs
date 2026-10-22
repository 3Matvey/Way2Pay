namespace Way2Pay.Payments.Application.Payments.PaymentMethods;

/// <summary>A non-secret view of a credential source's current availability and account restriction.</summary>
/// <param name="IsAvailable">Whether the source is currently usable by its merchant.</param>
/// <param name="ProviderAccountId">Null for a vault card; the owning account for a provider token.</param>
/// <remarks>This view is not persisted in Payments and does not authorize credential disclosure.</remarks>
public sealed record PaymentMethodAvailability(bool IsAvailable, Guid? ProviderAccountId);
