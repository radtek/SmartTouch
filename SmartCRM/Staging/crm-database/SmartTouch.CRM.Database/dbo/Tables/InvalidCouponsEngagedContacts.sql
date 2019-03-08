CREATE TABLE [dbo].[InvalidCouponsEngagedContacts] (
    [InvalidCoupanId]  INT      IDENTITY (1, 1) NOT NULL,
    [FormSubmissionID] INT      NOT NULL,
    [ContactID]        INT      NOT NULL,
    [LastUpdatedDate]  DATETIME NOT NULL,
    CONSTRAINT [PK_InvalidCouponsEngagedContacts] PRIMARY KEY CLUSTERED ([InvalidCoupanId] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_InvalidCouponsEngagedContacts_FormSubmissionID_ContactID]
    ON [dbo].[InvalidCouponsEngagedContacts]([FormSubmissionID] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90);

