CREATE TABLE [dbo].[EmailStatistics](
	[EmailTrackID] [int] IDENTITY(1,1) NOT NULL,
	[SentMailDetailID] [int] NOT NULL,
	[ContactID] [int] NULL,
	[EmailLinkID] [int] NULL,
	[ActivityType] [tinyint] NOT NULL,
	[ActivityDate] [datetime] NOT NULL,
	[IPAddress] [nvarchar](50) NULL,
	CONSTRAINT [PK_EmailStatistics] PRIMARY KEY CLUSTERED ([EmailTrackID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_EmailStatistics_EmailLinks] FOREIGN KEY ([EmailLinkID]) REFERENCES [dbo].[EmailLinks] ([EmailLinkID])

	)
GO



