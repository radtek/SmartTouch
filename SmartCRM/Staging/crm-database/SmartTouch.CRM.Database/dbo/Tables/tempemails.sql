CREATE TABLE [dbo].[tempemails] (
    [ContactEmailID] INT           NOT NULL,
    [ContactID]      INT           NOT NULL,
    [Email]          VARCHAR (256) NOT NULL,
    [EmailStatus]    SMALLINT      NOT NULL,
    [IsPrimary]      BIT           NOT NULL,
    [AccountID]      INT           NOT NULL,
    [SnoozeUntil]    DATETIME      NULL,
    [IsDeleted]      BIT           NOT NULL
);

