CREATE TABLE [dbo].[SignInActivity] (
    [SignInActivityID]   TINYINT        IDENTITY (1, 1) NOT NULL,
    [SignInActivityName] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_SignInActivity] PRIMARY KEY CLUSTERED ([SignInActivityID] ASC) WITH (FILLFACTOR = 90)
);

