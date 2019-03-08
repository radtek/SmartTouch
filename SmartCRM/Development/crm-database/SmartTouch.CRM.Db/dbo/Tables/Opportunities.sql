CREATE TABLE [dbo].[Opportunities] (
    [OpportunityID]       INT             IDENTITY (1, 1) NOT NULL,
    [OpportunityName]     NVARCHAR (75)   NOT NULL,
    [Potential]           MONEY           NOT NULL,
    [StageID]             SMALLINT         NULL,
    [ExpectedClosingDate] DATETIME        NULL,
    [Description]         NVARCHAR (1000) NULL,
    [Owner]               INT             NOT NULL,
    [AccountID]           INT             NOT NULL,
    [CreatedBy]           INT             NOT NULL,
    [CreatedOn]           DATETIME        NOT NULL,
    [LastModifiedBy]      INT             NULL,
    [LastModifiedOn]      DATETIME        NULL,
    [IsDeleted]           BIT             NULL,
	[OpportunityType]       VARCHAR(75)     NULL,
	[ProductType]           VARCHAR(75)     NULL,
	[Address]             VARCHAR(250)    NULL, 
	[ImageID]               INT             NULL
    CONSTRAINT [PK_Opportunities] PRIMARY KEY CLUSTERED ([OpportunityID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Opportunities_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Opportunities_Users] FOREIGN KEY ([Owner]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Opportunities_Users1] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Opportunities_Users2] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO

CREATE TRIGGER [dbo].[tr_Opportunities_Delete] ON [dbo].[Opportunities] FOR DELETE AS INSERT INTO Opportunities_Audit(OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,AuditAction, AuditStatus,OpportunityType,ProductType,Address,ImageID) SELECT OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,'D', 0,OpportunityType,ProductType,Address,ImageID FROM Deleted

 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	SELECT OpportunityID, CreatedBy, 16, 4, GETUTCDATE() FROM Inserted


GO



CREATE TRIGGER [dbo].[tr_Opportunities_Insert] ON [dbo].[Opportunities] FOR INSERT AS INSERT INTO Opportunities_Audit(OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,AuditAction, AuditStatus,OpportunityType,ProductType,Address,ImageID) SELECT OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,'I', 1,OpportunityType,ProductType,Address,ImageID FROM Inserted


 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	SELECT OpportunityID, CreatedBy, 16, 1, GETUTCDATE(),AccountID,OpportunityName FROM Inserted




GO






CREATE TRIGGER [dbo].[tr_Opportunities_Update] ON [dbo].[Opportunities]
FOR UPDATE AS 
	INSERT INTO Opportunities_Audit(OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,AuditAction, AuditStatus,OpportunityType,ProductType,Address,ImageID)
	SELECT OpportunityID,OpportunityName,Potential,StageID,ExpectedClosingDate,Description,Owner,AccountID,CreatedBy,CreatedOn,LastModifiedBy,LastModifiedOn,IsDeleted,'U', 1,OpportunityType,ProductType,Address,ImageID FROM Inserted


 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	SELECT OpportunityID, LastModifiedBy, 16, CASE WHEN (IsDeleted = 0 OR IsDeleted IS NULL) THEN 3 
											  WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(),AccountID,OpportunityName FROM Inserted




GO




