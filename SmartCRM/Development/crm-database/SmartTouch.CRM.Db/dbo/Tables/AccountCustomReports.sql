CREATE TABLE [dbo].[AccountCustomReports] (
    [AccountCustomReportID] INT             IDENTITY (1, 1) NOT NULL,
    [ReportID]              INT             NOT NULL,
    [ReportName]            VARCHAR (50)    NULL,
    [AccountID]             INT             NULL,
    [CreatedDate]           DATETIME        CONSTRAINT [DF__AccountCu__Creat__784AA6CD] DEFAULT (getutcdate()) NULL,
    [CreatedBy]             INT             NULL,
    [LastRunOn]             DATETIME        NULL,
    [SearchCriteria]        NVARCHAR (4000) NULL,
    [IsDeleted]             BIT             CONSTRAINT [DF__AccountCu__IsDel__747A15E9] DEFAULT ((0)) NULL,
    [ColumnList]            NVARCHAR (2500) NULL,
    [TableList]             NVARCHAR (2500) NULL,
    CONSTRAINT [PK_AccountCustomReports] PRIMARY KEY CLUSTERED ([AccountCustomReportID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountCustomReports_Reports] FOREIGN KEY ([ReportID]) REFERENCES [dbo].[Reports] ([ReportID])
);

