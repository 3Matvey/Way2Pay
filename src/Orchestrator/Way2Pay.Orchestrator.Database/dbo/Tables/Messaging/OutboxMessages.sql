-- Native JSON stores objects/arrays. Wrap scalar values in an object.

CREATE TABLE dbo.outbox_messages
(
    id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    event_type      NVARCHAR(150) NOT NULL,
    aggregate_type  NVARCHAR(100) NULL,
    aggregate_id    UNIQUEIDENTIFIER NULL,
    payload         JSON NOT NULL,
    created_at      DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    processed_at    DATETIME2(7) NULL,
    retry_count     INT NOT NULL DEFAULT 0,

    CONSTRAINT chk_outbox_retry_count
        CHECK (retry_count >= 0)
);

GO

CREATE INDEX ix_outbox_messages_unprocessed
    ON dbo.outbox_messages(created_at)
    WHERE processed_at IS NULL;
