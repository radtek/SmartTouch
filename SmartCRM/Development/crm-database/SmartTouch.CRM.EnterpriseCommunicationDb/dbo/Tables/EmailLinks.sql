CREATE TABLE [dbo].[EmailLinks](
	[EmailLinkID] [int] IDENTITY(1,1) NOT NULL,
	[SentMailDetailID] [int] NOT NULL,
	[LinkURL] [nvarchar](max) NOT NULL,
	[LinkIndex] [tinyint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_EmailLinks] PRIMARY KEY CLUSTERED ([EmailLinkID] ASC) WITH (FILLFACTOR = 90)
	)

GO

