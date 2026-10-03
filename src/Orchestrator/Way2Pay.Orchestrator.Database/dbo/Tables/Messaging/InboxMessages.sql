CREATE TABLE dbo.inbox_messages
(
    message_id      UNIQUEIDENTIFIER NOT NULL,
    consumer_name   NVARCHAR(150) NOT NULL,
    processed_at    DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    PRIMARY KEY (message_id, consumer_name)
);
