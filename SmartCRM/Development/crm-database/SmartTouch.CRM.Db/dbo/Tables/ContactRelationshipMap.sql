CREATE TABLE [dbo].[ContactRelationshipMap] (
    [ContactRelationshipMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]                INT      NOT NULL,
    [RelationshipType]         SMALLINT NOT NULL,
    [RelatedUserID]            INT      NULL,
    [RelatedContactID]         INT      NULL,
    [CreatedBy]                INT      NOT NULL,
    [CreatedOn]                DATETIME NOT NULL,
    CONSTRAINT [PK_ContactsRelationship] PRIMARY KEY CLUSTERED ([ContactRelationshipMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactRelationshipMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactRelationshipMap_Contacts1] FOREIGN KEY ([RelatedContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactRelationshipMap_DropdownValues] FOREIGN KEY ([RelationshipType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_ContactRelationshipMap_Users] FOREIGN KEY ([RelatedUserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ContactRelationshipMap_Users1] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO

CREATE TRIGGER [dbo].[tr_ContactRelationshipMap_Delete] ON [dbo].[ContactRelationshipMap] FOR DELETE AS INSERT INTO ContactRelationshipMap_Audit(ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,AuditAction,AuditStatus) SELECT ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,'D',0 FROM Deleted

UPDATE [dbo].[ContactRelationshipMap_Audit]
	SET [AuditStatus] = 0
	FROM [dbo].[ContactRelationshipMap_Audit] CRMA INNER JOIN Deleted D ON CRMA.[ContactRelationshipMapID] = D.ContactRelationshipMapID






GO

CREATE TRIGGER [dbo].[tr_ContactRelationshipMap_Insert] ON [dbo].[ContactRelationshipMap] FOR INSERT AS INSERT INTO ContactRelationshipMap_Audit(ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,AuditAction,AuditStatus) SELECT ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,'I',1 FROM Inserted






GO

CREATE TRIGGER [dbo].[tr_ContactRelationshipMap_Update] ON [dbo].[ContactRelationshipMap] FOR UPDATE AS INSERT INTO ContactRelationshipMap_Audit(ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,AuditAction,AuditStatus) SELECT ContactRelationshipMapID,ContactID,RelationshipType,RelatedUserID,RelatedContactID,CreatedBy,CreatedOn,'U',1 FROM Inserted





