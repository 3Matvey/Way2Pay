CREATE TABLE dbo.reconciliation_jobs
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    status              NVARCHAR(40) NOT NULL,
    payments_checked    INT NOT NULL DEFAULT 0,
    mismatch_count      INT NOT NULL DEFAULT 0,
    started_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_reconciliation_jobs_provider
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    CONSTRAINT uq_reconciliation_jobs_provider
        UNIQUE (id, provider_account_id),

    CONSTRAINT chk_reconciliation_payments_checked
        CHECK (payments_checked >= 0),

    CONSTRAINT chk_reconciliation_mismatch_count
        CHECK (mismatch_count >= 0)
);
