CREATE TABLE [dbo].[Reports] (
    [ReportID]      INT           IDENTITY (1, 1) NOT NULL,
    [ReportName]    VARCHAR (200) NOT NULL,
    [AccountID]     INT           NULL,
    [LastRunOn]     DATETIME      NULL,
    [ReportType]    TINYINT       NOT NULL,
    [CreatedBy]     INT           NULL,
    [CreatedOn]     DATETIME      NULL,
    [LastUpdatedBy] INT           NULL,
    [LastUpdatedOn] DATETIME      NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([ReportID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Reports_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Reports_Users1] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE TRIGGER [dbo].[tr_Reports_Delete] ON [dbo].[Reports] FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT ReportID, CreatedBy, 24, 4, GETUTCDATE() FROM Inserted



GO


CREATE TRIGGER [dbo].[tr_Reports_Insert] ON [dbo].[Reports] FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID, EntityName)
	 SELECT ReportID, CreatedBy, 24, 6, GETUTCDATE(), AccountID, ReportName FROM Inserted







GO



CREATE TRIGGER [dbo].[tr_Reports_Update] ON [dbo].[Reports]
FOR UPDATE AS 
	INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],[AccountID],EntityName)
	SELECT ReportID,CreatedBy,24,3,GETUTCDATE(),AccountID,'U' FROM Inserted





