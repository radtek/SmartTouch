CREATE TABLE [dbo].[ModuleOperationsMap] (
    [ModuleOperationsMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ModuleID]              TINYINT  NOT NULL,
    [OperationID]           SMALLINT NOT NULL,
    CONSTRAINT [PK_ModuleOperationsMap] PRIMARY KEY CLUSTERED ([ModuleOperationsMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ModuleOperationsMap_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_ModuleOperationsMap_Operations] FOREIGN KEY ([OperationID]) REFERENCES [dbo].[Operations] ([OperationID])
);

