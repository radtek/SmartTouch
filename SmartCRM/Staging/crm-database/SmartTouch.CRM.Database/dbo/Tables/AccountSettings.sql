CREATE TABLE [dbo].[AccountSettings] (
    [AccountSettingsID] INT           IDENTITY (1, 1) NOT NULL,
    [AccountID]         INT           NULL,
    [StatusID]          SMALLINT      NULL,
    [viewName]          VARCHAR (200) NULL,
    PRIMARY KEY CLUSTERED ([AccountSettingsID] ASC) WITH (FILLFACTOR = 90)
);

