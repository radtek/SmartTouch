CREATE TABLE [dbo].[ReportModuleMap] (
    [ReportModuleMapID] TINYINT IDENTITY (1, 1) NOT NULL,
    [ReportType]        TINYINT NOT NULL,
    [ModuleID]          TINYINT NOT NULL,
    PRIMARY KEY CLUSTERED ([ReportModuleMapID] ASC) WITH (FILLFACTOR = 90)
);

