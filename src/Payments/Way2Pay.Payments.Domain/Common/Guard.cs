using System.Runtime.CompilerServices;

namespace Way2Pay.Payments.Domain.Common;

public static class Guard
{
    public static Guid RequiredId(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("An identifier is required.", parameterName)
            : value;
    }

    public static string Required(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value;
    }

    public static decimal Positive(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        return value <= 0
            ? throw new ArgumentOutOfRangeException(parameterName, "The amount must be positive.")
            : value;
    }
}
