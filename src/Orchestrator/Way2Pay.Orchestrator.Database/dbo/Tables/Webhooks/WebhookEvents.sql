-- Native JSON stores objects/arrays. Wrap scalar values in an object.

CREATE TABLE dbo.webhook_events
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    external_event_id   NVARCHAR(255) NOT NULL,
    event_type          NVARCHAR(100) NOT NULL,
    payload             JSON NOT NULL,
    signature_valid     BIT NULL,
    processing_status   NVARCHAR(40) NOT NULL,
    received_at         DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    processed_at        DATETIME2(7) NULL,

    CONSTRAINT fk_webhook_events_provider_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    CONSTRAINT uq_webhook_event_external
        UNIQUE (provider_account_id, external_event_id)
);

GO

CREATE INDEX ix_webhook_events_processing_status
    ON dbo.webhook_events(processing_status, received_at);

-- Outgoing events are distinct from incoming provider webhook_events.
-- All retries and endpoint deliveries share the same event ID and payload.
