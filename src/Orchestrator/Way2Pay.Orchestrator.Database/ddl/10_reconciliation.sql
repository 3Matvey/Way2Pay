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

GO

CREATE TABLE dbo.reconciliation_items
(
    id                      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    reconciliation_job_id   UNIQUEIDENTIFIER NOT NULL,
    payment_attempt_id      UNIQUEIDENTIFIER NOT NULL,
    provider_account_id     UNIQUEIDENTIFIER NOT NULL,
    local_status            NVARCHAR(40) NOT NULL,
    provider_status         NVARCHAR(40) NOT NULL,
    resolution              NVARCHAR(100) NULL,
    checked_at              DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_reconciliation_items_job
        FOREIGN KEY (reconciliation_job_id, provider_account_id)
        REFERENCES dbo.reconciliation_jobs(id, provider_account_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_reconciliation_items_attempt
        FOREIGN KEY (payment_attempt_id, provider_account_id)
        REFERENCES dbo.payment_attempts(id, provider_account_id)
);

GO

CREATE INDEX ix_reconciliation_items_job
    ON dbo.reconciliation_items(reconciliation_job_id);
