CREATE TABLE [dbo].[ReIndexAccounts] (
    [ReIndexID]          INT            IDENTITY (1, 1) NOT NULL,
    [ReIndexAccountID]   INT            NOT NULL,
    [ReIndexAccountName] NVARCHAR (50)  NOT NULL,
    [ReIndexModule]      NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ReIndexAccounts] PRIMARY KEY CLUSTERED ([ReIndexID] ASC)
);

