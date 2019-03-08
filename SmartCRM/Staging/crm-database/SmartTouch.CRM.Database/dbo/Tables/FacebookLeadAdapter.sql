CREATE TABLE [dbo].[FacebookLeadAdapter] (
    [FacebookLeadAdapterID]      INT           IDENTITY (1, 1) NOT NULL,
    [PageAccessToken]            VARCHAR (250) NOT NULL,
    [AddID]                      BIGINT        NOT NULL,
    [LeadAdapterAndAccountMapID] INT           NOT NULL,
    [Name]                       VARCHAR (250) NULL,
    [PageID]                     BIGINT        NOT NULL,
    [TokenUpdatedOn]             DATETIME      NULL,
    CONSTRAINT [PK_FacebookLeadAdapter] PRIMARY KEY CLUSTERED ([FacebookLeadAdapterID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_FacebookLeadAdapter_LeadAdapterAndAccountMap] FOREIGN KEY ([LeadAdapterAndAccountMapID]) REFERENCES [dbo].[LeadAdapterAndAccountMap] ([LeadAdapterAndAccountMapID])
);

