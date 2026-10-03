CREATE TABLE dbo.routing_decisions
(
    id                          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    payment_id                  UNIQUEIDENTIFIER NOT NULL,
    routing_policy_version_id   UNIQUEIDENTIFIER NOT NULL,
    selected_provider_account_id UNIQUEIDENTIFIER NULL,
    reason                      NVARCHAR(MAX) NULL,
    created_at                  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_routing_decisions_payment
        FOREIGN KEY (payment_id)
        REFERENCES dbo.payments(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_routing_decisions_policy_version
        FOREIGN KEY (routing_policy_version_id)
        REFERENCES dbo.routing_policy_versions(id),

    CONSTRAINT fk_routing_decisions_selected_provider
        FOREIGN KEY (selected_provider_account_id)
        REFERENCES dbo.provider_accounts(id)
);

GO

CREATE INDEX ix_routing_decisions_payment
    ON dbo.routing_decisions(payment_id);

GO

ALTER TABLE dbo.routing_decisions
    ADD CONSTRAINT fk_routing_decisions_selected_candidate
    FOREIGN KEY (id, selected_provider_account_id)
    REFERENCES dbo.routing_decision_candidates(routing_decision_id, provider_account_id);
