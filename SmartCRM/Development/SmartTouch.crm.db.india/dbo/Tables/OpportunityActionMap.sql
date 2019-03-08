CREATE TABLE [dbo].[OpportunityActionMap] (
    [OpportunityActionMapID] INT IDENTITY (1, 1) NOT NULL,
    [OpportunityID]          INT NOT NULL,
    [ActionID]               INT NOT NULL,
    [IsCompleted]            BIT NULL,
    CONSTRAINT [PK_OpportunityActionMap] PRIMARY KEY CLUSTERED ([OpportunityActionMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunityActionMap_Actions] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Actions] ([ActionID]),
    CONSTRAINT [FK_OpportunityActionMap_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID])
);


GO


CREATE TRIGGER [dbo].[tr_OpportunityActionMap_Delete] ON [dbo].[OpportunityActionMap] FOR DELETE AS INSERT INTO OpportunityActionMap_Audit(OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,AuditAction,AuditStatus) SELECT OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,'D',0 FROM Deleted

UPDATE [dbo].[OpportunityActionMap_Audit]
SET [AuditStatus] = 0
FROM [dbo].[OpportunityActionMap_Audit] AA INNER JOIN Deleted D ON AA.ActionID = D.[ActionID]





GO


CREATE TRIGGER [dbo].[tr_OpportunityActionMap_Insert] ON [dbo].[OpportunityActionMap]
 FOR INSERT AS 
 INSERT INTO OpportunityActionMap_Audit(OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,AuditAction,AuditStatus)
  SELECT OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,'I',1 FROM Inserted





GO


CREATE TRIGGER [dbo].[tr_OpportunityActionMap_Update] ON [dbo].[OpportunityActionMap] FOR UPDATE AS INSERT INTO OpportunityActionMap_Audit(OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,AuditAction,AuditStatus) SELECT OpportunityActionMapID,OpportunityID,ActionID,IsCompleted,'U',1 FROM Inserted






