using System;
using System.Collections.Generic;

namespace Way2Pay.Orchestrator.Infrastructure.Persistence.Generated.Entities;

/// <summary>
/// Ключи идемпотентности запросов для распознавания повторных обращений и связи с созданным ресурсом.
/// </summary>
public partial class IdempotencyKey
{
    public Guid Id { get; set; }

    public string IdempotencyKey1 { get; set; } = null!;

    /// <summary>
    /// Отпечаток запроса для сопоставления повторного обращения с исходным содержимым.
    /// </summary>
    public string RequestHash { get; set; } = null!;

    public string? ResourceType { get; set; }

    public Guid? ResourceId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
