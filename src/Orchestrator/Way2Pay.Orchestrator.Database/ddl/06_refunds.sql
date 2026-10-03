CREATE TABLE dbo.refunds
(
    id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_id    UNIQUEIDENTIFIER NOT NULL,
    payment_attempt_id UNIQUEIDENTIFIER NOT NULL,
    amount        NUMERIC(22,8) NOT NULL,
    reason        NVARCHAR(MAX) NULL,
    status        NVARCHAR(40) NOT NULL,
    created_at    DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at  DATETIME2(7) NULL,

    CONSTRAINT fk_refunds_payment
        FOREIGN KEY (payment_id)
        REFERENCES dbo.payments(id),

    -- Each refund targets one concrete payment attempt of this payment.
    CONSTRAINT fk_refunds_payment_attempt
        FOREIGN KEY (payment_attempt_id, payment_id)
        REFERENCES dbo.payment_attempts(id, payment_id),

    CONSTRAINT uq_refunds_payment_attempt
        UNIQUE (id, payment_attempt_id),

    CONSTRAINT chk_refunds_amount
        CHECK (amount > 0)
);

GO

CREATE INDEX ix_refunds_payment
    ON dbo.refunds(payment_id);

GO

CREATE TABLE dbo.refund_attempts
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    refund_id           UNIQUEIDENTIFIER NOT NULL,
    payment_attempt_id  UNIQUEIDENTIFIER NOT NULL,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    provider_refund_id  NVARCHAR(255) NULL,
    status              NVARCHAR(40) NOT NULL,
    failure_code        NVARCHAR(100) NULL,
    failure_message     NVARCHAR(MAX) NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_refund_attempts_refund
        FOREIGN KEY (refund_id, payment_attempt_id)
        REFERENCES dbo.refunds(id, payment_attempt_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_refund_attempts_payment_attempt
        FOREIGN KEY (payment_attempt_id, provider_account_id)
        REFERENCES dbo.payment_attempts(id, provider_account_id)
);

GO

CREATE INDEX ix_refund_attempts_refund
    ON dbo.refund_attempts(refund_id);
