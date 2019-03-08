CREATE TABLE [dbo].[Emails] (
    [EmailID]        INT           IDENTITY (1, 1) NOT NULL,
    [Email]          VARCHAR (256) NOT NULL,
    [UserID]         INT           NOT NULL,
    [AccountID]      INT           NOT NULL,
    [IsPrimary]      BIT           NOT NULL,
    [EmailSignature] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Emails] PRIMARY KEY CLUSTERED ([EmailID] ASC) WITH (FILLFACTOR = 90)
);

