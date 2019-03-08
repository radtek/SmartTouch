CREATE TABLE [dbo].[NotificationStatuses] (
    [NotificationStatusID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [Name]                 VARCHAR (50) NULL,
    CONSTRAINT [PK_NotificationStatuses] PRIMARY KEY CLUSTERED ([NotificationStatusID] ASC) WITH (FILLFACTOR = 90)
);

