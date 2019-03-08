CREATE TABLE [dbo].[Forms] (
    [FormID]              INT            IDENTITY (1, 1) NOT NULL,
    [Name]                NVARCHAR (75)  NOT NULL,
    [Acknowledgement]     NVARCHAR (200) NOT NULL,
    [AcknowledgementType] TINYINT        NOT NULL,
    [HTMLContent]         NVARCHAR (MAX) NOT NULL,
    [Status]              SMALLINT       NOT NULL,
    [AccountID]           INT            NOT NULL,
    [CreatedBy]           INT            NOT NULL,
    [CreatedOn]           DATETIME       NOT NULL,
    [LastModifiedBy]      INT            NOT NULL,
    [LastModifiedOn]      DATETIME       NOT NULL,
    [IsDeleted]           BIT            NOT NULL,
    [Submissions]         AS             ([dbo].[Forms_Submission]([FormID])),
    [LeadSourceID]        SMALLINT       NULL,
    CONSTRAINT [PK_Forms] PRIMARY KEY CLUSTERED ([FormID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Forms_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Forms_DropdownValues] FOREIGN KEY ([LeadSourceID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_Forms_Statuses] FOREIGN KEY ([Status]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_Forms_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Forms_Users1] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_Forms_6_576721107__K1_K12]
    ON [dbo].[Forms]([FormID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Forms_AccountID]
    ON [dbo].[Forms]([AccountID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Forms_IsDeleted]
    ON [dbo].[Forms]([IsDeleted] ASC)
    INCLUDE([FormID], [Name], [Acknowledgement], [AcknowledgementType], [HTMLContent], [Status], [AccountID], [CreatedBy], [LastModifiedBy], [LastModifiedOn]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_Forms_Name_AccountID_IsDeleted]
    ON [dbo].[Forms]([Name] ASC, [AccountID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE TRIGGER [dbo].[tr_Forms_Delete] ON [dbo].[Forms] FOR DELETE AS 

INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT FormID, LastModifiedBy,10, 4, GETUTCDATE() FROM Inserted



GO

CREATE TRIGGER [dbo].[tr_Forms_Insert] ON [dbo].[Forms] FOR INSERT AS 

INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT FormID, CreatedBy,10, 1, GETUTCDATE(),AccountID,Name FROM Inserted




GO

CREATE TRIGGER [dbo].[tr_Forms_Update] ON [dbo].[Forms] FOR UPDATE AS

INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID,EntityName)
	 SELECT FormID, LastModifiedBy,10, CASE WHEN IsDeleted = 0 THEN 3 
											WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(),AccountID,Name FROM Inserted



