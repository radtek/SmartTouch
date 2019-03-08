CREATE TABLE [dbo].[Jobs] (
    [JobID]          INT           IDENTITY (1, 1) NOT NULL,
    [JobName]        VARCHAR (75)  NOT NULL,
    [Description]    VARCHAR (500) NULL,
    [ActivatorClass] VARCHAR (50)  NULL,
    [IsActive]       BIT           CONSTRAINT [DF_Jobs_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Jobs] PRIMARY KEY CLUSTERED ([JobID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);

