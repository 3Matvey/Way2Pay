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
