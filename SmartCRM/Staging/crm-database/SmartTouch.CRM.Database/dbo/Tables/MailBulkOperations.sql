CREATE TABLE [dbo].[MailBulkOperations] (
    [MailBulkOperationID] INT             IDENTITY (1, 1) NOT NULL,
    [From]                VARCHAR (500)   NULL,
    [To]                  TEXT            NULL,
    [CC]                  TEXT            NULL,
    [BCC]                 TEXT            NULL,
    [Subject]             NVARCHAR (4000) NULL,
    [Body]                NVARCHAR (MAX)  NULL,
    PRIMARY KEY CLUSTERED ([MailBulkOperationID] ASC)
);

