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
