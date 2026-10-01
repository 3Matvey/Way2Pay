-- Native JSON stores objects/arrays. Wrap scalar values in an object.

CREATE TABLE dbo.dead_letter_messages
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    original_message_id UNIQUEIDENTIFIER NULL,
    message_type        NVARCHAR(150) NOT NULL,
    payload             JSON NOT NULL,
    failure_reason      NVARCHAR(MAX) NOT NULL,
    retry_count         INT NOT NULL DEFAULT 0,
    failed_at           DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    resolved_at         DATETIME2(7) NULL,

    CONSTRAINT chk_dead_letter_retry_count
        CHECK (retry_count >= 0)
);

CREATE INDEX ix_dead_letter_messages_unresolved
    ON dbo.dead_letter_messages(failed_at)
    WHERE resolved_at IS NULL;

CREATE TABLE dbo.audit_log
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    actor       NVARCHAR(255) NULL,
    entity_type NVARCHAR(100) NOT NULL,
    entity_id   UNIQUEIDENTIFIER NULL,
    action      NVARCHAR(100) NOT NULL,
    old_value   JSON NULL,
    new_value   JSON NULL,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE INDEX ix_audit_log_entity
    ON dbo.audit_log(entity_type, entity_id, created_at DESC);
