
CREATE TYPE [dbo].[CommunicationType] AS TABLE(
	[CommunicationID] [int] NULL,
	[SecondaryEmails] [nvarchar](2000) NULL,
	[FacebookUrl] [varchar](2000) NULL,
	[TwitterUrl] [varchar](2000) NULL,
	[GooglePlusUrl] [varchar](2000) NULL,
	[LinkedInUrl] [varchar](2000) NULL,
	[BlogUrl] [varchar](2000) NULL,
	[WebSiteUrl] [varchar](2000) NULL,
	[FacebookAccessToken] [varchar](2000) NULL,
	[TwitterOAuthToken] [varchar](2000) NULL,
	[TwitterOAuthTokenSecret] [varchar](2000) NULL
)
GO


