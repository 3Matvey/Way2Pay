CREATE TABLE dbo.routing_rules
(
    id                          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    routing_policy_version_id   UNIQUEIDENTIFIER NOT NULL,
    name                        NVARCHAR(150) NOT NULL,
    priority                    INT NOT NULL,
    is_enabled                  BIT NOT NULL DEFAULT 1,

    CONSTRAINT fk_routing_rules_version
        FOREIGN KEY (routing_policy_version_id)
        REFERENCES dbo.routing_policy_versions(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_routing_rules_priority
        CHECK (priority >= 0)
);

GO

CREATE INDEX ix_routing_rules_version_priority
    ON dbo.routing_rules(routing_policy_version_id, priority);
