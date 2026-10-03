CREATE TABLE dbo.provider_health_checks
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    status              NVARCHAR(30) NOT NULL,
    latency_ms          INT NULL,
    failure_reason      NVARCHAR(MAX) NULL,
    checked_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_provider_health_checks_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_provider_health_latency
        CHECK (latency_ms IS NULL OR latency_ms >= 0)
);

GO

CREATE INDEX ix_provider_health_checks_account_time
    ON dbo.provider_health_checks(provider_account_id, checked_at DESC);
