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

GO

CREATE INDEX ix_payment_attempts_payment
    ON dbo.payment_attempts(payment_id);

GO

CREATE INDEX ix_payment_attempts_provider_transaction
    ON dbo.payment_attempts(provider_account_id, provider_transaction_id);
