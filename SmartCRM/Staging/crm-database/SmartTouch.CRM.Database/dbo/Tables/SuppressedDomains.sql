CREATE TABLE [dbo].[SuppressedDomains] (
    [SuppressedDomainID] INT           IDENTITY (1, 1) NOT NULL,
    [Domain]             VARCHAR (300) NOT NULL,
    [AccountID]          INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([SuppressedDomainID] ASC) WITH (FILLFACTOR = 90)
);

