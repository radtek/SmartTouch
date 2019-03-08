CREATE TABLE [dbo].[ActiveContactsIndexData] (
    [DataID]    INT      IDENTITY (1, 1) NOT NULL,
    [AccountID] INT      NOT NULL,
    [ContactID] INT      NOT NULL,
    [IsActive]  INT      NOT NULL,
    [CreatedOn] DATETIME NULL,
    PRIMARY KEY CLUSTERED ([DataID] ASC)
);

