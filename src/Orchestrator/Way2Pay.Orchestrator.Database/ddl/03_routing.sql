CREATE TABLE dbo.routing_policies
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    name        NVARCHAR(150) NOT NULL,
    description NVARCHAR(MAX) NULL,
    is_active   BIT NOT NULL DEFAULT 1,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

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

GO

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

GO

CREATE TABLE dbo.routing_rule_conditions
(
    id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    routing_rule_id UNIQUEIDENTIFIER NOT NULL,
    field_name      NVARCHAR(100) NOT NULL,
    [operator]        NVARCHAR(30) NOT NULL,
    [value]           NVARCHAR(MAX) NOT NULL,

    CONSTRAINT fk_routing_rule_conditions_rule
        FOREIGN KEY (routing_rule_id)
        REFERENCES dbo.routing_rules(id)
        ON DELETE CASCADE
);

GO

CREATE TABLE dbo.routing_rule_actions
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    routing_rule_id     UNIQUEIDENTIFIER NOT NULL,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    priority            INT NOT NULL,
    weight              INT NULL,

    CONSTRAINT fk_routing_rule_actions_rule
        FOREIGN KEY (routing_rule_id)
        REFERENCES dbo.routing_rules(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_routing_rule_actions_provider_account
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    CONSTRAINT uq_routing_rule_action_provider
        UNIQUE (routing_rule_id, provider_account_id),

    CONSTRAINT chk_routing_rule_action_priority
        CHECK (priority >= 0),

    CONSTRAINT chk_routing_rule_action_weight
        CHECK (weight IS NULL OR weight >= 0)
);
