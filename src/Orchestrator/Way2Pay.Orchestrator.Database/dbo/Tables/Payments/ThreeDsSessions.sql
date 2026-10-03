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

GO

CREATE INDEX ix_three_ds_sessions_attempt
    ON dbo.three_ds_sessions(payment_attempt_id);
