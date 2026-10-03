CREATE TABLE dbo.provider_fee_rules
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    country_code        NVARCHAR(2) NULL,
    currency_code       NVARCHAR(3) NULL,
    percent_fee         NUMERIC(8,4) NOT NULL DEFAULT 0,
    fixed_fee           NUMERIC(22,8) NOT NULL DEFAULT 0,
    min_amount          NUMERIC(22,8) NULL,
    max_amount          NUMERIC(22,8) NULL,
    valid_from          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    valid_to            DATETIME2(7) NULL,

    CONSTRAINT fk_provider_fee_rules_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_provider_fee_rules_country
        FOREIGN KEY (country_code)
        REFERENCES dbo.countries(code),

    CONSTRAINT fk_provider_fee_rules_currency
        FOREIGN KEY (currency_code)
        REFERENCES dbo.currencies(code),

    CONSTRAINT chk_provider_fee_percent
        CHECK (percent_fee >= 0),

    CONSTRAINT chk_provider_fee_fixed
        CHECK (fixed_fee >= 0),

    CONSTRAINT chk_provider_fee_amount_range
        CHECK (
            min_amount IS NULL
            OR max_amount IS NULL
            OR min_amount <= max_amount
        )
);

GO

CREATE INDEX ix_provider_fee_rules_account
    ON dbo.provider_fee_rules(provider_account_id);
