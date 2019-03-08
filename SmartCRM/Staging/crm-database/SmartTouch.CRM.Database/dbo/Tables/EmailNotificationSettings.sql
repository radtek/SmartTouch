CREATE TABLE [dbo].[EmailNotificationSettings] (
    [ID]          INT           IDENTITY (1, 1) NOT NULL,
    [Description] VARCHAR (100) NOT NULL,
    [Key]         VARCHAR (100) NOT NULL,
    [CategoryID]  SMALLINT      NOT NULL,
    CONSTRAINT [PK_WorkflowNotificationSettings] PRIMARY KEY CLUSTERED ([ID] ASC)
);

