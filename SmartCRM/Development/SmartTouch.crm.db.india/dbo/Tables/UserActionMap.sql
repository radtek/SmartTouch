

CREATE TABLE [dbo].[UserActionMap](
	[UserActionMapID] [int] IDENTITY(1,1) NOT NULL,
	[ActionID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[LastUpdatedBy] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
	[UserEmailGuid] [uniqueidentifier] NULL,
	[UserTextGuid] [uniqueidentifier] NULL,
CONSTRAINT [PK_UserActionMap] PRIMARY KEY CLUSTERED ([UserActionMapID] ASC) WITH (FILLFACTOR = 90),
CONSTRAINT [FK_UserActionMap_Actions] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Actions] ([ActionID]),
CONSTRAINT [FK_UserActionMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);
GO

CREATE NONCLUSTERED INDEX [IX_UserActionMap_UserID_ActionID] ON [dbo].[UserActionMap]
(
	[UserID] ASC
)
INCLUDE ( 	[ActionID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


Create NonClustered Index IX_UserActionMap_missing_41 On [dbo].[UserActionMap] ([ActionID]);
GO


