CREATE TABLE [dbo].[CustomFieldTabs] (
    [CustomFieldTabID] INT           IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (75) NOT NULL,
    [StatusID]         SMALLINT      NOT NULL,
    [SortID]           TINYINT       NOT NULL,
    [AccountID]        INT           NOT NULL,
    [IsLeadAdapterTab] BIT           NOT NULL,
    CONSTRAINT [PK_CustomFieldTabs] PRIMARY KEY CLUSTERED ([CustomFieldTabID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CustomFieldTabs_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_CustomFieldTabs_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);

