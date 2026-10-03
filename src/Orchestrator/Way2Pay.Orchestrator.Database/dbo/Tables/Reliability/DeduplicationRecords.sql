CREATE TABLE dbo.deduplication_records
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    source              NVARCHAR(100) NOT NULL,
    external_event_id   NVARCHAR(255) NOT NULL,
    payload_hash        NVARCHAR(128) NULL,
    processed_at        DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT uq_deduplication_source_event
        UNIQUE (source, external_event_id)
);
