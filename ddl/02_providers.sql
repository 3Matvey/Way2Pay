CREATE TABLE dbo.providers
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    code        NVARCHAR(50) NOT NULL UNIQUE,
    name        NVARCHAR(100) NOT NULL,
    status      NVARCHAR(30) NOT NULL,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);

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

CREATE INDEX ix_provider_accounts_provider_id
    ON dbo.provider_accounts(provider_id);

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

CREATE TABLE dbo.provider_capabilities
(
    provider_id UNIQUEIDENTIFIER NOT NULL,
    capability  NVARCHAR(50) NOT NULL,

    PRIMARY KEY (provider_id, capability),

    CONSTRAINT fk_provider_capabilities_provider
        FOREIGN KEY (provider_id)
        REFERENCES dbo.providers(id)
        ON DELETE CASCADE
);

CREATE TABLE dbo.provider_supported_countries
(
    provider_id  UNIQUEIDENTIFIER NOT NULL,
    country_code NVARCHAR(2) NOT NULL,

    PRIMARY KEY (provider_id, country_code),

    CONSTRAINT fk_provider_countries_provider
        FOREIGN KEY (provider_id)
        REFERENCES dbo.providers(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_provider_countries_country
        FOREIGN KEY (country_code)
        REFERENCES dbo.countries(code)
);

CREATE TABLE dbo.provider_supported_currencies
(
    provider_id   UNIQUEIDENTIFIER NOT NULL,
    currency_code NVARCHAR(3) NOT NULL,

    PRIMARY KEY (provider_id, currency_code),

    CONSTRAINT fk_provider_currencies_provider
        FOREIGN KEY (provider_id)
        REFERENCES dbo.providers(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_provider_currencies_currency
        FOREIGN KEY (currency_code)
        REFERENCES dbo.currencies(code)
);

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

CREATE INDEX ix_provider_fee_rules_account
    ON dbo.provider_fee_rules(provider_account_id);

CREATE TABLE dbo.provider_health_checks
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    status              NVARCHAR(30) NOT NULL,
    latency_ms          INT NULL,
    failure_reason      NVARCHAR(MAX) NULL,
    checked_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_provider_health_checks_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_provider_health_latency
        CHECK (latency_ms IS NULL OR latency_ms >= 0)
);

CREATE INDEX ix_provider_health_checks_account_time
    ON dbo.provider_health_checks(provider_account_id, checked_at DESC);
