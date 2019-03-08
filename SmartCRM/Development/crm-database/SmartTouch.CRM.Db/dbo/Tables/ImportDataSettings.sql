CREATE TABLE [dbo].[ImportDataSettings] (
    [ImportDataSettingID]    INT              IDENTITY (1, 1) NOT NULL,
    [UpdateOnDuplicate]      BIT              NOT NULL,
    [UniqueImportIdentifier] UNIQUEIDENTIFIER NOT NULL,
    [AccountID]              INT              NOT NULL,
    [ProcessBy]              INT              NOT NULL,
    [ProcessDate]            DATETIME         NOT NULL,
    [DuplicateLogic]         TINYINT          NOT NULL,
    [LeadAdaperJobID]        INT              NULL,
    CONSTRAINT [PK_ImportDataSettings] PRIMARY KEY CLUSTERED ([ImportDataSettingID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ImportDataSettings_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_ImportDataSettings_LeadAdapterJobLogs] FOREIGN KEY ([LeadAdaperJobID]) REFERENCES [dbo].[LeadAdapterJobLogs] ([LeadAdapterJobLogID]),
    CONSTRAINT [FK_ImportDataSettings_Users] FOREIGN KEY ([ProcessBy]) REFERENCES [dbo].[Users] ([UserID])
);

