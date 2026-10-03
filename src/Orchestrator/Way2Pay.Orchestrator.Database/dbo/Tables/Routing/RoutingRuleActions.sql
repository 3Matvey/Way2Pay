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
