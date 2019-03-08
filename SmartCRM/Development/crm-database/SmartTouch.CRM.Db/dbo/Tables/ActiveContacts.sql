﻿CREATE TABLE [dbo].[ActiveContacts] (
    [AccountID]   INT      NULL,
    [ContactID]   INT      NULL,
    [EmailStatus] SMALLINT NULL,
    [IsDeleted]   BIT      NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_ActiveContacts_AccountID]
    ON [dbo].[ActiveContacts]([AccountID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ActiveContacts_EmailStatus]
    ON [dbo].[ActiveContacts]([ContactID] ASC, [EmailStatus] ASC) WITH (FILLFACTOR = 90);
