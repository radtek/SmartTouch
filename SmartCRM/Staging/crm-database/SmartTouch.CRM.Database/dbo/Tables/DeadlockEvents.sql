CREATE TABLE [dbo].[DeadlockEvents] (
    [DeadlockID] INT      IDENTITY (1, 1) NOT NULL,
    [EventMsg]   XML      NULL,
    [EventDate]  DATETIME CONSTRAINT [df_DeadlockEvents_EventDate] DEFAULT (getdate()) NOT NULL
);

