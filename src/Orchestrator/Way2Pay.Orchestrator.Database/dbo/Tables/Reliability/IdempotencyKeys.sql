CREATE TABLE dbo.idempotency_keys
(
    id                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    idempotency_key   NVARCHAR(255) NOT NULL UNIQUE,
    request_hash      NVARCHAR(128) NOT NULL,
    resource_type     NVARCHAR(50) NULL,
    resource_id       UNIQUEIDENTIFIER NULL,
    status            NVARCHAR(30) NOT NULL,
    created_at        DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at        DATETIME2(7) NULL
);

GO

CREATE INDEX ix_idempotency_keys_expires_at
    ON dbo.idempotency_keys(expires_at);
