CREATE TABLE [dbo].[MailDetails] (
    [MailDetailID] BIGINT           IDENTITY (1, 1) NOT NULL,
    [MailGuidID]   UNIQUEIDENTIFIER NOT NULL,
    [Priority]     TINYINT          NULL,
    [To]           NVARCHAR (256)   NULL,
    [CC]           NVARCHAR (256)   NULL,
    [BCC]          NVARCHAR (256)   NULL,
    [Body]         NVARCHAR (MAX)   NULL,
    [DisplayName]  NVARCHAR (75)    NULL,
    [ReplyTo]      NVARCHAR (75)    NULL,
    [IsBodyHtml]   BIT              NULL,
    CONSTRAINT [PK_MailDetails] PRIMARY KEY CLUSTERED ([MailDetailID] ASC) WITH (FILLFACTOR = 90)
);

