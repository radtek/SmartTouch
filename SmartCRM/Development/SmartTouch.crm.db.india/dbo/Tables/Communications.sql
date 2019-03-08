CREATE TABLE [dbo].[Communications] (
    [CommunicationID]         INT             IDENTITY (1, 1) NOT NULL,
    [SecondaryEmails]         NVARCHAR (2100) NULL,
    [FacebookUrl]             VARCHAR (2000)  NULL,
    [TwitterUrl]              VARCHAR (2000)  NULL,
    [GooglePlusUrl]           VARCHAR (2000)  NULL,
    [LinkedInUrl]             VARCHAR (2000)  NULL,
    [BlogUrl]                 VARCHAR (2000)  NULL,
    [WebSiteUrl]              VARCHAR (2000)  NULL,
    [FacebookAccessToken]     VARCHAR (MAX)   NULL,
    [TwitterOAuthToken]       VARCHAR (MAX)   NULL,
    [TwitterOAuthTokenSecret] VARCHAR (MAX)   NULL,
    CONSTRAINT [PK_Communications] PRIMARY KEY CLUSTERED ([CommunicationID] ASC) WITH (FILLFACTOR = 90)
);

