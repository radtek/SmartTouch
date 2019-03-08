CREATE TABLE [dbo].[StoreProcParamsList] (
    [ListID]     BIGINT         IDENTITY (1, 1) NOT NULL,
    [AccountID]  INT            NULL,
    [UserID]     INT            NULL,
    [ReportName] VARCHAR (255)  NULL,
    [ParamList]  NVARCHAR (MAX) NULL,
    [ReportDate] DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([ListID] ASC) WITH (FILLFACTOR = 90)
);

