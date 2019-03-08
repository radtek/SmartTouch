CREATE TABLE [dbo].[ImportContactsEmailStatuses] (
    [MailGunVerificationID] INT              IDENTITY (1, 1) NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NULL,
    [EmailStatus]           SMALLINT         NULL,
    [LeadadapterJobLogID]   INT              NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactsEmailStatuses_missing_94]
    ON [dbo].[ImportContactsEmailStatuses]([ReferenceID] ASC)
    INCLUDE([EmailStatus]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-20161026-125736]
    ON [dbo].[ImportContactsEmailStatuses];

