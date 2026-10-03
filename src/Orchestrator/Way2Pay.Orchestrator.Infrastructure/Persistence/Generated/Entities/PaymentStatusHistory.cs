using System;
using System.Collections.Generic;

namespace Way2Pay.Orchestrator.Infrastructure.Persistence.Generated.Entities;

/// <summary>
/// История изменений статуса платежа. Одна запись фиксирует один переход или установку статуса.
/// </summary>
public partial class PaymentStatusHistory
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    /// <summary>
    /// Предыдущий статус платежа, если он известен; NULL допустим, например при первой записи истории.
    /// </summary>
    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
