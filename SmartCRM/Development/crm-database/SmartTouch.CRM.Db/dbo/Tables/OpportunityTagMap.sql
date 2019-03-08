CREATE TABLE [dbo].[OpportunityTagMap] (
    [OpportunityTagMapID] INT      IDENTITY (1, 1) NOT NULL,
    [OpportunityID]       INT      NOT NULL,
    [TagID]               INT      NOT NULL,
    [TaggedBy]            INT      NOT NULL,
    [TaggedOn]            DATETIME NOT NULL,
    CONSTRAINT [PK_OpportunityTagMap] PRIMARY KEY CLUSTERED ([OpportunityTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunityTagMap_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID]),
    CONSTRAINT [FK_OpportunityTagMap_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]),
    CONSTRAINT [FK_OpportunityTagMap_Users] FOREIGN KEY ([TaggedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE TRIGGER [dbo].[tr_OpportunityTagMap_Delete] ON [dbo].[OpportunityTagMap] FOR DELETE AS INSERT INTO OpportunityTagMap_Audit(OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,AuditAction,AuditStatus) SELECT OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,'D',0 FROM Deleted


GO

CREATE TRIGGER [dbo].[tr_OpportunityTagMap_Insert] ON [dbo].[OpportunityTagMap] FOR INSERT AS INSERT INTO OpportunityTagMap_Audit(OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,AuditAction,AuditStatus) SELECT OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,'I',1 FROM Inserted



GO
CREATE TRIGGER [dbo].[tr_OpportunityTagMap_Update] ON [dbo].[OpportunityTagMap] FOR UPDATE AS INSERT INTO OpportunityTagMap_Audit(OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,AuditAction,AuditStatus) SELECT OpportunityTagMapID,OpportunityID,TagID,TaggedBy,TaggedOn,'U',1 FROM Inserted

