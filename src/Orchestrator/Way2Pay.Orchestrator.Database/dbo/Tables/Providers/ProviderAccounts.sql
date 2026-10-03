CREATE TABLE dbo.provider_accounts
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_id         UNIQUEIDENTIFIER NOT NULL,
    external_account_id NVARCHAR(255) NULL,
    display_name        NVARCHAR(100) NOT NULL,
    status              NVARCHAR(30) NOT NULL,
    is_sandbox          BIT NOT NULL DEFAULT 0,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_provider_accounts_provider
        FOREIGN KEY (provider_id)
        REFERENCES dbo.providers(id)
);

GO

CREATE INDEX ix_provider_accounts_provider_id
    ON dbo.provider_accounts(provider_id);
