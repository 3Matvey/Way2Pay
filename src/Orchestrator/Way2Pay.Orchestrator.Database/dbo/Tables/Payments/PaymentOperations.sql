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

GO

CREATE INDEX ix_payment_operations_payment
    ON dbo.payment_operations(payment_id, created_at);
