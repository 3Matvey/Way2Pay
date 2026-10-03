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
