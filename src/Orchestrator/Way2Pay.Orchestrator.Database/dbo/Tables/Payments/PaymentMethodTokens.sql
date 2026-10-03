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
