
CREATE TABLE [dbo].[InvalidCouponsEngagedContacts](
	[InvalidCoupanId] [int] IDENTITY(1,1) NOT NULL,
	[FormSubmissionID] [int] NOT NULL,
	[ContactID] [int] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_InvalidCouponsEngagedContacts] PRIMARY KEY CLUSTERED (InvalidCoupanId ASC) WITH (FILLFACTOR = 90));

GO


CREATE NONCLUSTERED INDEX [IX_InvalidCouponsEngagedContacts_FormSubmissionID_ContactID] ON [dbo].[InvalidCouponsEngagedContacts]
(
	[FormSubmissionID] ASC,
	[ContactID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO





