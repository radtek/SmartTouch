CREATE TABLE [dbo].[tblLogsfortuning] (
    [ID]                INT            IDENTITY (1, 1) NOT NULL,
    [ExecutionDatetime] DATETIME       NULL,
    [ExecutionTime]     FLOAT (53)     NULL,
    [SPName]            VARCHAR (1000) NULL,
    [Parameters]        VARCHAR (MAX)  NULL
);

