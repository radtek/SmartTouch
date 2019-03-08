CREATE TABLE [dbo].[MailResponseDetails] (
    [MailResponseDetailsID] INT              IDENTITY (1, 1) NOT NULL,
    [MailResponseID]        INT              NULL,
    [MailGuid]              UNIQUEIDENTIFIER NULL,
    [To]                    NVARCHAR (75)    NULL,
    [From]                  NVARCHAR (75)    NULL,
    [CC]                    NVARCHAR (75)    NULL,
    [BCC]                   NVARCHAR (75)    NULL,
    [Status]                TINYINT          NULL,
    [ServiceResponse]       NVARCHAR (50)    NULL,
    [CreatedDate]           DATETIME         NULL,
    CONSTRAINT [PK_MailResponseDetails] PRIMARY KEY CLUSTERED ([MailResponseDetailsID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_MailResponseDetails_CommunicationStatus] FOREIGN KEY ([Status]) REFERENCES [dbo].[CommunicationStatus] ([CommunicationStatusID]),
    CONSTRAINT [FK_MailResponseDetails_MailResponse] FOREIGN KEY ([MailResponseID]) REFERENCES [dbo].[MailResponse] ([MailResponseID])
);

