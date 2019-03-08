CREATE TABLE [dbo].[SearchDefinitions] (
    [SearchDefinitionID]    INT            IDENTITY (1, 1) NOT NULL,
    [SearchDefinitionName]  NVARCHAR (100) NOT NULL,
    [ElasticQuery]          VARCHAR (500)  NULL,
    [SearchPredicateTypeID] SMALLINT       NOT NULL,
    [CustomPredicateScript] VARCHAR (MAX)  NULL,
    [LastRunDate]           DATETIME       NULL,
    [CreatedBy]             INT            NOT NULL,
    [CreatedOn]             DATETIME       NOT NULL,
    [AccountID]             INT            NULL,
    [IsFavoriteSearch]      BIT            NOT NULL,
    [IsPreConfiguredSearch] BIT            NOT NULL,
    [SelectAllSearch]       BIT            NULL,
    CONSTRAINT [PK_SearchDefinitions] PRIMARY KEY CLUSTERED ([SearchDefinitionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SearchDefinitions_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_SearchDefinitions_SearchPredicateTypes] FOREIGN KEY ([SearchPredicateTypeID]) REFERENCES [dbo].[SearchPredicateTypes] ([SearchPredicateTypeID]),
    CONSTRAINT [FK_SearchDefinitions_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO



CREATE TRIGGER [dbo].[tr_SearchDefinitions_Delete] ON [dbo].[SearchDefinitions]
FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT SearchDefinitionID, CreatedBy, 31, 4, GETUTCDATE(),AccountID,SearchDefinitionName FROM deleted






GO


CREATE TRIGGER [dbo].[tr_SearchDefinitions_Insert] ON [dbo].[SearchDefinitions] FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT SearchDefinitionID, CreatedBy, 31, 1, GETUTCDATE(),AccountID,SearchDefinitionName FROM Inserted





GO


CREATE TRIGGER [dbo].[tr_SearchDefinitions_Update] ON [dbo].[SearchDefinitions]
FOR UPDATE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	SELECT SearchDefinitionID, CreatedBy, 31, 3, GETUTCDATE(),AccountID,SearchDefinitionName FROM Inserted




