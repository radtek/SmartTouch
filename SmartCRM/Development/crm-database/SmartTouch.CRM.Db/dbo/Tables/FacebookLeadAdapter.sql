
CREATE TABLE [dbo].[FacebookLeadAdapter](
	[FacebookLeadAdapterID] [int] IDENTITY(1,1) NOT NULL,
	[PageAccessToken] [varchar](250) NOT NULL,
	[AddID] [bigint] NOT NULL,
	[LeadAdapterAndAccountMapID] [int] NOT NULL,
	[Name] [varchar](250) NULL,
	[PageID] [bigint] NOT NULL,
	TokenUpdatedOn datetime null,
	CONSTRAINT [PK_FacebookLeadAdapter] PRIMARY KEY CLUSTERED ([FacebookLeadAdapterID] ASC) WITH (FILLFACTOR = 90) ON [PRIMARY],
	CONSTRAINT [FK_FacebookLeadAdapter_LeadAdapterAndAccountMap] FOREIGN KEY ([LeadAdapterAndAccountMapID]) REFERENCES [dbo].[LeadAdapterAndAccountMap] ([LeadAdapterAndAccountMapID]));

GO





