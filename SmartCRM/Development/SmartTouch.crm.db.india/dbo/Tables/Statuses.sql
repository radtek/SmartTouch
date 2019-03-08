CREATE TABLE [dbo].[Statuses] (
    [StatusID]    SMALLINT       NOT NULL,
    [Name]        VARCHAR (50)   NOT NULL,
    [ModuleID]    TINYINT        NOT NULL,
    [Description] NVARCHAR (200) NULL,
    CONSTRAINT [PK_Statuses] PRIMARY KEY CLUSTERED ([StatusID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Statuses_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID])
);

