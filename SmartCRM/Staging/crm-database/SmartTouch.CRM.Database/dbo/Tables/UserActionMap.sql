CREATE TABLE [dbo].[UserActionMap] (
    [UserActionMapID] INT              IDENTITY (1, 1) NOT NULL,
    [ActionID]        INT              NOT NULL,
    [UserID]          INT              NOT NULL,
    [LastUpdatedBy]   INT              NULL,
    [LastUpdatedOn]   DATETIME         NULL,
    [UserEmailGuid]   UNIQUEIDENTIFIER CONSTRAINT [Def_UserEmailGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    [UserTextGuid]    UNIQUEIDENTIFIER CONSTRAINT [Def_UserTextGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    CONSTRAINT [PK_UserActionMap] PRIMARY KEY CLUSTERED ([UserActionMapID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_UserActionMap_Actions] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Actions] ([ActionID]),
    CONSTRAINT [FK_UserActionMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserActionMap_UserID_ActionID]
    ON [dbo].[UserActionMap]([UserID] ASC)
    INCLUDE([ActionID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_UserActionMap_missing_41]
    ON [dbo].[UserActionMap]([ActionID] ASC);

