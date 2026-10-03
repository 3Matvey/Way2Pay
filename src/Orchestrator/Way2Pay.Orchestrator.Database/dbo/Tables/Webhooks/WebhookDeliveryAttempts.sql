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
