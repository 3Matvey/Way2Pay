CREATE TABLE dbo.provider_credentials
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    credential_type     NVARCHAR(50) NOT NULL,
    encrypted_value     NVARCHAR(MAX) NOT NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at          DATETIME2(7) NULL,

    CONSTRAINT fk_provider_credentials_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_provider_credentials_type
        UNIQUE (provider_account_id, credential_type)
);
