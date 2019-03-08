CREATE TABLE [dbo].[OpportunityContactMap] (
    [OpportunityContactMapID] INT IDENTITY (1, 1) NOT NULL,
    [OpportunityID]           INT NOT NULL,
    [ContactID]               INT NOT NULL,
    CONSTRAINT [PK_OpportunityContactMap] PRIMARY KEY CLUSTERED ([OpportunityContactMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunityContactMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_OpportunityContactMap_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID])
);


GO
CREATE TRIGGER [dbo].[tr_OpportunityContactMap_Delete] ON [dbo].[OpportunityContactMap] FOR DELETE AS INSERT INTO OpportunityContactMap_Audit(OpportunityContactMapID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityContactMapID,OpportunityID,ContactID,'D',0 FROM Deleted


GO
CREATE TRIGGER [dbo].[tr_OpportunityContactMap_Insert] ON [dbo].[OpportunityContactMap] FOR INSERT AS INSERT INTO OpportunityContactMap_Audit(OpportunityContactMapID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityContactMapID,OpportunityID,ContactID,'I',1 FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_OpportunityContactMap_Update] ON [dbo].[OpportunityContactMap] FOR UPDATE AS INSERT INTO OpportunityContactMap_Audit(OpportunityContactMapID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityContactMapID,OpportunityID,ContactID,'U',1 FROM Inserted

