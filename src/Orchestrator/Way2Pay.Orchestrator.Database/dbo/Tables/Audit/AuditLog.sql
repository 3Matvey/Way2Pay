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

GO

CREATE INDEX ix_audit_log_entity
    ON dbo.audit_log(entity_type, entity_id, created_at DESC);
