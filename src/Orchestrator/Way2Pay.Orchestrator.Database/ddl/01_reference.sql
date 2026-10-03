CREATE TABLE dbo.currencies
(
    code            NVARCHAR(3) NOT NULL PRIMARY KEY,
    name            NVARCHAR(100) NOT NULL,
    numeric_code    NVARCHAR(3) NULL,
    decimal_places  SMALLINT NOT NULL DEFAULT 2,

    CONSTRAINT chk_currencies_decimal_places
        CHECK (decimal_places BETWEEN 0 AND 8)
);

GO

CREATE TABLE dbo.countries
(
    code NVARCHAR(2) NOT NULL PRIMARY KEY,
    name NVARCHAR(100) NOT NULL
);

GO

CREATE TABLE dbo.webhook_endpoints
(
    id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    name        NVARCHAR(100) NOT NULL,
    url         NVARCHAR(2048) NOT NULL,
    secret      NVARCHAR(512) NULL,
    is_active   BIT NOT NULL DEFAULT 1,
    created_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at  DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
