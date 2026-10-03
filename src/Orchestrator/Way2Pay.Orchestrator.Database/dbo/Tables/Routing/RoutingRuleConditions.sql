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
