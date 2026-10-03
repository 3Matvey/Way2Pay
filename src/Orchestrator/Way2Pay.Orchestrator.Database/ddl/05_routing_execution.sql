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

CREATE TABLE dbo.routing_decision_candidates
(
    id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    routing_decision_id UNIQUEIDENTIFIER NOT NULL,
    provider_account_id UNIQUEIDENTIFIER NOT NULL,
    priority            INT NULL,
    estimated_fee       NUMERIC(22,8) NULL,
    health_status       NVARCHAR(30) NULL,
    is_eligible         BIT NOT NULL,
    rejection_reason    NVARCHAR(MAX) NULL,

    CONSTRAINT fk_routing_candidates_decision
        FOREIGN KEY (routing_decision_id)
        REFERENCES dbo.routing_decisions(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_routing_candidates_provider
        FOREIGN KEY (provider_account_id)
        REFERENCES dbo.provider_accounts(id),

    CONSTRAINT uq_routing_candidate_provider
        UNIQUE (routing_decision_id, provider_account_id)
);

-- SQL Server checks this FK immediately (no deferred constraints).
-- In one transaction: insert a decision with NULL selection, insert candidates,
-- then UPDATE selected_provider_account_id. NULL means no provider selected.
-- Clear the selection before explicitly deleting its selected candidate.
GO

ALTER TABLE dbo.routing_decisions
    ADD CONSTRAINT fk_routing_decisions_selected_candidate
    FOREIGN KEY (id, selected_provider_account_id)
    REFERENCES dbo.routing_decision_candidates(routing_decision_id, provider_account_id);
