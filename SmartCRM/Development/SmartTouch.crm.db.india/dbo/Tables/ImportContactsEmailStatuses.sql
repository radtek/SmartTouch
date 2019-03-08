CREATE TABLE [dbo].[ImportContactsEmailStatuses] (
    [MailGunVerificationID] INT              IDENTITY (1, 1) NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NULL,
    [EmailStatus]           SMALLINT         NULL,
    CONSTRAINT [PK_ImportContactsEmailStatuses] PRIMARY KEY CLUSTERED ([MailGunVerificationID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactsEmailStatuses_ReferenceID]
    ON [dbo].[ImportContactsEmailStatuses]([ReferenceID] ASC)
    INCLUDE([EmailStatus]) WITH (FILLFACTOR = 90);

