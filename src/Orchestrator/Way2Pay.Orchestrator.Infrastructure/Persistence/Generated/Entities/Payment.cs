using System;
using System.Collections.Generic;

namespace Way2Pay.Orchestrator.Infrastructure.Persistence.Generated.Entities;

/// <summary>
/// Платежи, принятые системой для обработки. Здесь хранится текущее состояние; отдельные обращения к провайдерам хранятся в payment_attempts.
/// </summary>
public partial class Payment
{
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор платежа во внешней системе. Непустые значения уникальны; NULL допускается для нескольких платежей.
    /// </summary>
    public string? ExternalPaymentId { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string? CountryCode { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Время последнего изменения платежа. DEFAULT задает значение при вставке; последующие обновления должно отражать приложение.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<PaymentStatusHistory> PaymentStatusHistories { get; set; } = new List<PaymentStatusHistory>();
}
