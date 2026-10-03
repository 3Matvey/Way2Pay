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
GO

CREATE TABLE dbo.outbound_webhook_events
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    event_type  NVARCHAR(100) NOT NULL,
    resource_id UNIQUEIDENTIFIER NULL,
    payload     JSON NOT NULL,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

CREATE TABLE dbo.webhook_delivery_attempts
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    webhook_endpoint_id UNIQUEIDENTIFIER NOT NULL,
    outbound_webhook_event_id UNIQUEIDENTIFIER NOT NULL,
    attempt_number      INT NOT NULL,
    http_status_code    INT NULL,
    status              NVARCHAR(40) NOT NULL,
    attempted_at        DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    next_retry_at       DATETIME2(7) NULL,

    CONSTRAINT fk_webhook_delivery_attempts_endpoint
        FOREIGN KEY (webhook_endpoint_id)
        REFERENCES dbo.webhook_endpoints(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_webhook_delivery_attempts_event
        FOREIGN KEY (outbound_webhook_event_id)
        REFERENCES dbo.outbound_webhook_events(id),

    CONSTRAINT uq_webhook_delivery_attempt
        UNIQUE (outbound_webhook_event_id, webhook_endpoint_id, attempt_number),

    CONSTRAINT chk_webhook_delivery_attempt
        CHECK (attempt_number > 0),

    CONSTRAINT chk_webhook_http_status
        CHECK (
            http_status_code IS NULL
            OR http_status_code BETWEEN 100 AND 599
        )
);

GO

CREATE INDEX ix_webhook_delivery_retry
    ON dbo.webhook_delivery_attempts(status, next_retry_at);
