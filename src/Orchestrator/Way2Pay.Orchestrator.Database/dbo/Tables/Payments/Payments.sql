CREATE TABLE dbo.payments
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    external_payment_id NVARCHAR(255) NULL,
    amount              NUMERIC(22,8) NOT NULL,
    currency_code       NVARCHAR(3) NOT NULL,
    country_code        NVARCHAR(2) NULL,
    status              NVARCHAR(40) NOT NULL,
    description         NVARCHAR(MAX) NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_payments_currency
        FOREIGN KEY (currency_code)
        REFERENCES dbo.currencies(code),

    CONSTRAINT fk_payments_country
        FOREIGN KEY (country_code)
        REFERENCES dbo.countries(code),

    CONSTRAINT chk_payments_amount
        CHECK (amount > 0)
);

-- Allow multiple NULL external IDs; enforce uniqueness for supplied IDs only.

GO

CREATE UNIQUE INDEX uq_payments_external_payment_id
    ON dbo.payments(external_payment_id)
    WHERE external_payment_id IS NOT NULL;

GO

CREATE INDEX ix_payments_status_created_at
    ON dbo.payments(status, created_at DESC);

GO

CREATE INDEX ix_payments_created_at
    ON dbo.payments(created_at DESC);
