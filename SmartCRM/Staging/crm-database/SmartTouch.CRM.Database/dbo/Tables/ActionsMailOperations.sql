CREATE TABLE [dbo].[ActionsMailOperations] (
    [ActionsMailOperationID] INT              IDENTITY (1, 1) NOT NULL,
    [ActionID]               INT              NOT NULL,
    [IsScheduled]            BIT              NOT NULL,
    [IsProcessed]            TINYINT          NOT NULL,
    [MailBulkOperationID]    INT              NOT NULL,
    [GroupID]                UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([ActionsMailOperationID] ASC)
);

