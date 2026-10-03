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

GO

CREATE INDEX ix_payment_status_history_payment_time
    ON dbo.payment_status_history(payment_id, changed_at);
