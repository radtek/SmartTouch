CREATE TABLE [dbo].[DashboardSettings] (
    [DashBoardID] TINYINT      IDENTITY (1, 1) NOT NULL,
    [Report]      VARCHAR (30) NOT NULL,
    [Value]       BIT          NULL,
    CONSTRAINT [PK_DashboardSettings] PRIMARY KEY CLUSTERED ([DashBoardID] ASC) WITH (FILLFACTOR = 90)
);

