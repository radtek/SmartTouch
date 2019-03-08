CREATE TABLE [dbo].[Users] (
    [UserID]            INT            IDENTITY (1, 1) NOT NULL,
    [AccountID]         INT            NOT NULL,
    [FirstName]         NVARCHAR (200) NULL,
    [LastName]          NVARCHAR (200) NULL,
    [PrimaryEmail]      VARCHAR (256)  NOT NULL,
    [Password]          NVARCHAR (75)  NULL,
    [RoleID]            SMALLINT       NOT NULL,
    [Title]             VARCHAR (30)   NULL,
    [Company]           NVARCHAR (200) NULL,
    [HomePhone]         VARCHAR (75)   NULL,
    [WorkPhone]         VARCHAR (75)   NULL,
    [MobilePhone]       VARCHAR (75)   NULL,
    [CommunicationID]   INT            NULL,
    [Status]            TINYINT        NOT NULL,
    [IsDeleted]         BIT            CONSTRAINT [DF_Users_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedBy]         INT            NOT NULL,
    [CreatedOn]         DATETIME       NOT NULL,
    [ModifiedBy]        INT            NULL,
    [ModifiedOn]        DATETIME       NULL,
    [PasswordResetFlag] BIT            NULL,
    [PasswordResetOn]   DATETIME       NULL,
    [EmailSignature]    NVARCHAR (MAX) NULL,
    [PrimaryPhoneType]  CHAR (1)       NULL,
    [HasTourCompleted]  BIT            NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Users_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles] ([RoleID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Users_AccountID_IsDeleted]
    ON [dbo].[Users]([AccountID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Users_AccountID_RoleID_IsDeleted]
    ON [dbo].[Users]([AccountID] ASC, [RoleID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Users_AccountID_Status]
    ON [dbo].[Users]([AccountID] ASC, [Status] ASC) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_Users_IsDeleted_FirstName]
    ON [dbo].[Users]([IsDeleted] ASC, [FirstName] ASC)
    INCLUDE([AccountID], [LastName], [PrimaryEmail], [RoleID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Status_IsDeleted_AccountID]
    ON [dbo].[Users]([Status] ASC, [IsDeleted] ASC, [AccountID] ASC)
    INCLUDE([UserID], [FirstName], [LastName], [PrimaryEmail]) WITH (FILLFACTOR = 90);


GO
CREATE TRIGGER [dbo].[tr_Users_Delete] ON [dbo].[Users] FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT UserID, ModifiedBy, 2, 4, GETUTCDATE() FROM Inserted



GO

CREATE TRIGGER [dbo].[tr_Users_Insert] ON [dbo].[Users] FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT UserID, CreatedBy, 2, 1, GETUTCDATE(),AccountID,FirstName+' '+LastName FROM Inserted




GO



CREATE TRIGGER [dbo].[tr_Users_Update] ON [dbo].[Users]
FOR UPDATE AS 
	INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT UserID, ModifiedBy, 2, CASE WHEN IsDeleted = 0 THEN 3 
										WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(),AccountID,FirstName+' '+LastName FROM Inserted





