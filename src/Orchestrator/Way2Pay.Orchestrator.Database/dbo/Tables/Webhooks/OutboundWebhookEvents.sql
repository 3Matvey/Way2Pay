CREATE TABLE dbo.outbound_webhook_events
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    event_type  NVARCHAR(100) NOT NULL,
    resource_id UNIQUEIDENTIFIER NULL,
    payload     JSON NOT NULL,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
