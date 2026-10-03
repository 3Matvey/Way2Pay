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
