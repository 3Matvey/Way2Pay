CREATE TABLE dbo.routing_policy_versions
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    routing_policy_id   UNIQUEIDENTIFIER NOT NULL,
    version             INT NOT NULL,
    activated_at        DATETIME2(7) NULL,
    deactivated_at      DATETIME2(7) NULL,
    created_at          DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_routing_policy_versions_policy
        FOREIGN KEY (routing_policy_id)
        REFERENCES dbo.routing_policies(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_routing_policy_version
        UNIQUE (routing_policy_id, version),

    CONSTRAINT chk_routing_policy_version_positive
        CHECK (version > 0)
);
