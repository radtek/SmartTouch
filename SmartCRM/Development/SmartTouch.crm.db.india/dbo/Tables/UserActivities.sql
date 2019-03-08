CREATE TABLE [dbo].[UserActivities] (
    [UserActivityID] TINYINT      IDENTITY (1, 1) NOT NULL,
    [ActivityName]   VARCHAR (15) NULL,
    CONSTRAINT [PK_UserActivities] PRIMARY KEY CLUSTERED ([UserActivityID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);

