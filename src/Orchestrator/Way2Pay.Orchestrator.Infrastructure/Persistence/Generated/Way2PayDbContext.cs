using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Way2Pay.Orchestrator.Infrastructure.Persistence.Generated.Entities;

namespace Way2Pay.Orchestrator.Infrastructure.Persistence.Generated;

public partial class Way2PayDbContext : DbContext
{
    public Way2PayDbContext(DbContextOptions<Way2PayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentStatusHistory> PaymentStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__idempote__3213E83F3B3028F6");

            entity.ToTable("idempotency_keys", tb => tb.HasComment("Ключи идемпотентности запросов для распознавания повторных обращений и связи с созданным ресурсом."));

            entity.HasIndex(e => e.IdempotencyKey1, "UQ__idempote__A7BA59F4D7F924EF").IsUnique();

            entity.HasIndex(e => e.ExpiresAt, "ix_idempotency_keys_expires_at");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdempotencyKey1)
                .HasMaxLength(255)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.RequestHash)
                .HasMaxLength(128)
                .HasComment("Отпечаток запроса для сопоставления повторного обращения с исходным содержимым.")
                .HasColumnName("request_hash");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.ResourceType)
                .HasMaxLength(50)
                .HasColumnName("resource_type");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83F13738DC2");

            entity.ToTable("payments", tb => tb.HasComment("Платежи, принятые системой для обработки. Здесь хранится текущее состояние; отдельные обращения к провайдерам хранятся в payment_attempts."));

            entity.HasIndex(e => e.CreatedAt, "ix_payments_created_at").IsDescending();

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_payments_status_created_at").IsDescending(false, true);

            entity.HasIndex(e => e.ExternalPaymentId, "uq_payments_external_payment_id")
                .IsUnique()
                .HasFilter("([external_payment_id] IS NOT NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("numeric(22, 8)")
                .HasColumnName("amount");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .HasColumnName("country_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasColumnName("currency_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExternalPaymentId)
                .HasMaxLength(255)
                .HasComment("Идентификатор платежа во внешней системе. Непустые значения уникальны; NULL допускается для нескольких платежей.")
                .HasColumnName("external_payment_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasComment("Время последнего изменения платежа. DEFAULT задает значение при вставке; последующие обновления должно отражать приложение.")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PaymentStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment___3213E83F56506764");

            entity.ToTable("payment_status_history", tb => tb.HasComment("История изменений статуса платежа. Одна запись фиксирует один переход или установку статуса."));

            entity.HasIndex(e => new { e.PaymentId, e.ChangedAt }, "ix_payment_status_history_payment_time");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("changed_at");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(40)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(40)
                .HasComment("Предыдущий статус платежа, если он известен; NULL допустим, например при первой записи истории.")
                .HasColumnName("old_status");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Reason).HasColumnName("reason");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentStatusHistories)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("fk_payment_status_history_payment");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
