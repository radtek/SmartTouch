CREATE TYPE [dbo].[ContactEmailType] AS TABLE (
    [ContactEmailID] INT           NULL,
    [ContactID]      INT           NULL,
    [EmailId]        VARCHAR (256) NULL,
    [EmailStatus]    TINYINT       NOT NULL,
    [IsPrimary]      BIT           NOT NULL,
    [AccountID]      INT           NOT NULL,
    [SnoozeUntil]    DATETIME      NULL,
    [IsDeleted]      BIT           NOT NULL);

