CREATE TABLE [dbo].[ContactTourMap] (
    [ContactTourMapID] INT      IDENTITY (1, 1) NOT NULL,
    [TourID]           INT      NOT NULL,
    [ContactID]        INT      NOT NULL,
    [IsCompleted]      BIT      NOT NULL,
    [LastUpdatedBy]    INT      NULL,
    [LastUpdatedOn]    DATETIME NULL,
    CONSTRAINT [PK_ContactTourMap] PRIMARY KEY CLUSTERED ([ContactTourMapID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ContactTours_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactTours_Tours] FOREIGN KEY ([TourID]) REFERENCES [dbo].[Tours] ([TourID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTourMap_TourID]
    ON [dbo].[ContactTourMap]([TourID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactTourMap_ContactID]
    ON [dbo].[ContactTourMap]([ContactID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [ContactTourMap_TourID_IsCompleted]
    ON [dbo].[ContactTourMap]([TourID] ASC, [ContactID] ASC, [IsCompleted] ASC)
    INCLUDE([ContactTourMapID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE TRIGGER [dbo].[tr_ContactTourMap_Delete] ON [dbo].[ContactTourMap] FOR DELETE AS INSERT INTO ContactTourMap_Audit(ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'D',0 FROM Deleted

UPDATE [dbo].[ContactTourMap_Audit]
SET [AuditStatus] = 0
FROM [dbo].[ContactTourMap_Audit] CTMA INNER JOIN Deleted D ON CTMA.ContactTourMapID = D.ContactTourMapID

GO
CREATE TRIGGER [dbo].[tr_ContactTourMap_Insert] ON [dbo].[ContactTourMap] FOR INSERT AS INSERT INTO ContactTourMap_Audit(ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'I',1 FROM Inserted

GO
CREATE TRIGGER [dbo].[tr_ContactTourMap_Update] ON [dbo].[ContactTourMap] FOR UPDATE AS INSERT INTO ContactTourMap_Audit(ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,AuditAction,AuditStatus) SELECT ContactTourMapID,TourID,ContactID,IsCompleted,LastUpdatedBy,LastUpdatedOn,'U',1 FROM Inserted
