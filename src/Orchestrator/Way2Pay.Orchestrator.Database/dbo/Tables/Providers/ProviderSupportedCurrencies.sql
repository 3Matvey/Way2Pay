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
