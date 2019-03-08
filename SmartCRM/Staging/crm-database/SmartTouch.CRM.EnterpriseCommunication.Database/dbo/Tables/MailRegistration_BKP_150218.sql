CREATE TABLE [dbo].[MailRegistration_BKP_150218] (
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
    [MailChimpListID]    NVARCHAR (25)    NULL
);

