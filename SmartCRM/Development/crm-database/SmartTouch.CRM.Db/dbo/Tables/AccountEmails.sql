CREATE TABLE [dbo].[AccountEmails] (
    [EmailID]           INT           IDENTITY (1, 1) NOT NULL,
    [Email]             VARCHAR (256) NOT NULL,
    [UserID]            INT           NULL,
    [AccountID]         INT           NOT NULL,
    [IsPrimary]         BIT           NOT NULL,
    [EmailSignature]    VARCHAR (MAX) NULL,
    [ServiceProviderID] INT           NOT NULL,
    CONSTRAINT [PK_AccountEmails] PRIMARY KEY CLUSTERED ([EmailID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountEmails_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_AccountEmails_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

