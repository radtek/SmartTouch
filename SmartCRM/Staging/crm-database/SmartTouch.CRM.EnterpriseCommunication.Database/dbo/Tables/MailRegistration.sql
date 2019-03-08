CREATE TABLE [dbo].[MailRegistration] (
    [MailRegistrationID] INT              IDENTITY (1, 1) NOT NULL,
    [Guid]               UNIQUEIDENTIFIER NOT NULL,
    [Name]               NVARCHAR (75)    NOT NULL,
    [Host]               NVARCHAR (75)    NOT NULL,
    [APIKey]             NVARCHAR (75)    NULL,
    [UserName]           NVARCHAR (75)    NOT NULL,
    [Password]           NVARCHAR (75)    NOT NULL,
    [Port]               INT              NULL,
    [IsSSLEnabled]       BIT              NOT NULL,
    [MailProviderID]     TINYINT          NOT NULL,
    [VMTA]               VARCHAR (50)     NULL,
    [SenderDomain]       VARCHAR (100)    NULL,
    [ImageDomain]        VARCHAR (100)    NULL,
    [MailChimpListID]    NVARCHAR (25)    NULL,
    CONSTRAINT [PK_MailRegistration] PRIMARY KEY CLUSTERED ([MailRegistrationID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_MailRegistration_Guid]
    ON [dbo].[MailRegistration]([Guid] ASC)
    INCLUDE([Name], [Host], [APIKey], [UserName], [Password], [Port], [IsSSLEnabled], [MailProviderID], [VMTA], [SenderDomain]) WITH (FILLFACTOR = 90);

