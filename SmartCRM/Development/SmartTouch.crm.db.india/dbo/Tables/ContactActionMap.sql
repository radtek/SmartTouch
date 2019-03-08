CREATE TABLE [dbo].[ContactActionMap] (
    [ContactActionMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ActionID]           INT      NOT NULL,
    [ContactID]          INT      NOT NULL,
    [IsCompleted]        BIT      NULL,
    [LastUpdatedBy]      INT      NULL,
    [LastUpdatedOn]      DATETIME NULL,
    CONSTRAINT [PK_ContactActionMap] PRIMARY KEY CLUSTERED ([ContactActionMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ActionContacts_Actions] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Actions] ([ActionID]),
    CONSTRAINT [FK_ActionContacts_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactActionMap_ActionID]
    ON [dbo].[ContactActionMap]([ActionID] ASC)
    INCLUDE([ContactID], [IsCompleted]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactActionMap_ActionID_ActionID]
    ON [dbo].[ContactActionMap]([ActionID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactActionMap_ContactID]
    ON [dbo].[ContactActionMap]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_ContactActionMap_LastUpdatedBy_ContactActionMapID]
    ON [dbo].[ContactActionMap]([LastUpdatedBy] ASC)
    INCLUDE([ContactActionMapID], [ActionID], [ContactID], [IsCompleted], [LastUpdatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE TRIGGER [dbo].[tr_ContactActionMap_Delete] ON [dbo].[ContactActionMap] FOR DELETE AS INSERT INTO ContactActionMap_Audit(ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'D',0 FROM Deleted

UPDATE [dbo].[ContactActionMap_Audit]
SET [AuditStatus] = 0
FROM [dbo].[ContactActionMap_Audit] CAMA INNER JOIN Deleted D ON CAMA.ContactActionMapID = D.ContactActionMapID


GO

CREATE TRIGGER [dbo].[tr_ContactActionMap_Insert] ON [dbo].[ContactActionMap] FOR
 INSERT AS INSERT INTO ContactActionMap_Audit(ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'I',1 FROM Inserted


GO

CREATE TRIGGER [dbo].[tr_ContactActionMap_Update] ON [dbo].[ContactActionMap] FOR UPDATE AS INSERT INTO ContactActionMap_Audit(ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactActionMapID,ActionID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'U',1 FROM Inserted

