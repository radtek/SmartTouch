CREATE TABLE [dbo].[UserTourMap] (
    [UserTourMapID] INT              IDENTITY (1, 1) NOT NULL,
    [TourID]        INT              NOT NULL,
    [UserID]        INT              NOT NULL,
    [LastUpdatedBy] INT              NULL,
    [LastUpdatedOn] DATETIME         NULL,
    [UserEmailGuid] UNIQUEIDENTIFIER CONSTRAINT [Def_UserTourMap_UserEmailGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    [UserTextGuid]  UNIQUEIDENTIFIER CONSTRAINT [Def_UserTourMap_UserTextGuid] DEFAULT ('00000000-0000-0000-0000-000000000000') NULL,
    CONSTRAINT [PK_UserTourMap] PRIMARY KEY CLUSTERED ([UserTourMapID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_UserTourMap_Tours] FOREIGN KEY ([TourID]) REFERENCES [dbo].[Tours] ([TourID]),
    CONSTRAINT [FK_UserTourMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserTourMap_UserID_TourID]
    ON [dbo].[UserTourMap]([UserID] ASC)
    INCLUDE([TourID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_UserTourMap_missing_23]
    ON [dbo].[UserTourMap]([TourID] ASC);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20171109-125200]
    ON [dbo].[UserTourMap]([TourID] ASC, [UserID] ASC)
    INCLUDE([UserTourMapID]);

