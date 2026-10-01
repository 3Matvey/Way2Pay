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
CREATE UNIQUE INDEX uq_payments_external_payment_id
    ON dbo.payments(external_payment_id)
    WHERE external_payment_id IS NOT NULL;

CREATE INDEX ix_payments_status_created_at
    ON dbo.payments(status, created_at DESC);

CREATE INDEX ix_payments_created_at
    ON dbo.payments(created_at DESC);

CREATE TABLE dbo.payment_method_tokens
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    provider_token      NVARCHAR(512) NOT NULL,
    payment_method_type NVARCHAR(40) NOT NULL,
    card_brand          NVARCHAR(30) NULL,
    last4               NVARCHAR(4) NULL,
    expiry_month        SMALLINT NULL,
    expiry_year         SMALLINT NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_payment_method_tokens_provider_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    CONSTRAINT uq_payment_method_tokens_account
        UNIQUE (id, provider_account_id),

    CONSTRAINT chk_payment_method_expiry_month
        CHECK (
            expiry_month IS NULL
            OR expiry_month BETWEEN 1 AND 12
        )
);

CREATE TABLE dbo.payment_attempts
(
    id                      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_id              UNIQUEIDENTIFIER NOT NULL,
    provider_account_id     UNIQUEIDENTIFIER NOT NULL,
    payment_method_token_id UNIQUEIDENTIFIER NULL,
    attempt_number          INT NOT NULL,
    provider_transaction_id NVARCHAR(255) NULL,
    status                  NVARCHAR(40) NOT NULL,
    amount                  NUMERIC(22,8) NOT NULL,
    failure_code            NVARCHAR(100) NULL,
    failure_message         NVARCHAR(MAX) NULL,
    started_at              DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at            DATETIME2(7) NULL,

    CONSTRAINT fk_payment_attempts_payment
        FOREIGN KEY (payment_id)
        REFERENCES dbo.payments(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_payment_attempts_provider_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    -- A provider-scoped token may only be used by the same provider account.
    CONSTRAINT fk_payment_attempts_token
        FOREIGN KEY (payment_method_token_id, provider_account_id)
        REFERENCES dbo.payment_method_tokens(id, provider_account_id),

    CONSTRAINT uq_payment_attempts_payment
        UNIQUE (id, payment_id),

    CONSTRAINT uq_payment_attempts_provider
        UNIQUE (id, provider_account_id),

    CONSTRAINT uq_payment_attempt_number
        UNIQUE (payment_id, attempt_number),

    CONSTRAINT chk_payment_attempt_number
        CHECK (attempt_number > 0),

    CONSTRAINT chk_payment_attempt_amount
        CHECK (amount > 0)
);

CREATE INDEX ix_payment_attempts_payment
    ON dbo.payment_attempts(payment_id);

CREATE INDEX ix_payment_attempts_provider_transaction
    ON dbo.payment_attempts(provider_account_id, provider_transaction_id);

CREATE TABLE dbo.payment_operations
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_id          UNIQUEIDENTIFIER NOT NULL,
    payment_attempt_id  UNIQUEIDENTIFIER NULL,
    operation_type      NVARCHAR(40) NOT NULL,
    amount              NUMERIC(22,8) NOT NULL,
    status              NVARCHAR(40) NOT NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_payment_operations_payment
        FOREIGN KEY (payment_id)
        REFERENCES dbo.payments(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_payment_operations_attempt
        FOREIGN KEY (payment_attempt_id, payment_id)
        REFERENCES dbo.payment_attempts(id, payment_id),

    CONSTRAINT chk_payment_operations_amount
        CHECK (amount > 0)
);

CREATE INDEX ix_payment_operations_payment
    ON dbo.payment_operations(payment_id, created_at);

CREATE TABLE dbo.payment_status_history
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_id  UNIQUEIDENTIFIER NOT NULL,
    old_status  NVARCHAR(40) NULL,
    new_status  NVARCHAR(40) NOT NULL,
    reason      NVARCHAR(MAX) NULL,
    changed_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_payment_status_history_payment
        FOREIGN KEY (payment_id)
        REFERENCES dbo.payments(id)
        ON DELETE CASCADE
);

CREATE INDEX ix_payment_status_history_payment_time
    ON dbo.payment_status_history(payment_id, changed_at);

CREATE TABLE dbo.three_ds_sessions
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_attempt_id  UNIQUEIDENTIFIER NOT NULL,
    provider_session_id NVARCHAR(255) NULL,
    status              NVARCHAR(40) NOT NULL,
    redirect_url        NVARCHAR(MAX) NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at          DATETIME2(7) NULL,
    completed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_three_ds_sessions_attempt
        FOREIGN KEY (payment_attempt_id)
        REFERENCES dbo.payment_attempts(id)
        ON DELETE CASCADE
);

CREATE INDEX ix_three_ds_sessions_attempt
    ON dbo.three_ds_sessions(payment_attempt_id);
