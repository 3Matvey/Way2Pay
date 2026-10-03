CREATE TABLE dbo.currencies
(
    code            NVARCHAR(3) NOT NULL PRIMARY KEY,
    name            NVARCHAR(100) NOT NULL,
    numeric_code    NVARCHAR(3) NULL,
    decimal_places  SMALLINT NOT NULL DEFAULT 2,

    CONSTRAINT chk_currencies_decimal_places
        CHECK (decimal_places BETWEEN 0 AND 8)
);
