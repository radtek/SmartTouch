CREATE TABLE [dbo].[OpportunitiesRelationshipMap] (
    [OpportunityRelationshipMapID] INT      IDENTITY (1, 1) NOT NULL,
    [RelationshipTypeID]           SMALLINT NOT NULL,
    [OpportunityID]                INT      NOT NULL,
    [ContactID]                    INT      NOT NULL,
    CONSTRAINT [PK_OpportunitiesRelationshipMap] PRIMARY KEY CLUSTERED ([OpportunityRelationshipMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunitiesRelationshipMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_OpportunitiesRelationshipMap_DropdownValues] FOREIGN KEY ([RelationshipTypeID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_OpportunitiesRelationshipMap_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID])
);


GO
CREATE TRIGGER [dbo].[tr_OpportunitiesRelationshipMap_Update] ON [dbo].[OpportunitiesRelationshipMap] FOR UPDATE AS INSERT INTO OpportunitiesRelationshipMap_Audit(OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,'U', 1 FROM Inserted


GO
CREATE TRIGGER [dbo].[tr_OpportunitiesRelationshipMap_Delete] ON [dbo].[OpportunitiesRelationshipMap] FOR DELETE AS INSERT INTO OpportunitiesRelationshipMap_Audit(OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,'D',0 FROM Deleted


GO
CREATE TRIGGER [dbo].[tr_OpportunitiesRelationshipMap_Insert] ON [dbo].[OpportunitiesRelationshipMap] FOR INSERT AS INSERT INTO OpportunitiesRelationshipMap_Audit(OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,AuditAction,AuditStatus) SELECT OpportunityRelationshipMapID,RelationshipTypeID,OpportunityID,ContactID,'I',1 FROM Inserted

