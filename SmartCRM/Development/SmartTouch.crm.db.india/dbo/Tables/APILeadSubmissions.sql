
CREATE TABLE [dbo].[APILeadSubmissions](
	[APILeadSubmissionID] [int] IDENTITY(1,1) NOT NULL,
	[ContactID] [int] NULL,
	[AccountID] [int] NOT NULL,
	[OwnerID] [int] NULL,
	[SubmittedData] [varchar](max) NOT NULL,
	[SubmittedOn] [datetime] NULL,
	[IsProcessed] [tinyint] NOT NULL,
	[Remarks] [varchar](max) NULL,
	 CONSTRAINT [PK_APILeadSubmissions] PRIMARY KEY CLUSTERED ([APILeadSubmissionID] ASC) WITH (FILLFACTOR = 90) ON [PRIMARY]);
GO


