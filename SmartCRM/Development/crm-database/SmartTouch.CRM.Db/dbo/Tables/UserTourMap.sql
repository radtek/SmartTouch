

CREATE TABLE [dbo].[UserTourMap](
	[UserTourMapID] [int] IDENTITY(1,1) NOT NULL,
	[TourID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[LastUpdatedBy] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
	[UserEmailGuid] UNIQUEIDENTIFIER CONSTRAINT [Def_UserTourMap_UserEmailGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    [UserTextGuid]  UNIQUEIDENTIFIER CONSTRAINT [Def_UserTourMap_UserTextGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
CONSTRAINT [PK_UserTourMap] PRIMARY KEY CLUSTERED ([UserTourMapID] ASC) WITH (FILLFACTOR = 90),
CONSTRAINT [FK_UserTourMap_Tours] FOREIGN KEY ([TourID]) REFERENCES [dbo].[Tours] ([TourID]),
CONSTRAINT [FK_UserTourMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);
GO

CREATE NONCLUSTERED INDEX [IX_UserTourMap_UserID_TourID] ON [dbo].[UserTourMap]
(
	[UserID] ASC
)
INCLUDE ( 	[TourID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

Create NonClustered Index IX_UserTourMap_missing_23 On [dbo].[UserTourMap] ([TourID]);
GO


