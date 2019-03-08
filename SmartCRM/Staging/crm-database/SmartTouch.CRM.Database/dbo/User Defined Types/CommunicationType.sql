CREATE TYPE [dbo].[CommunicationType] AS TABLE (
    [CommunicationID]         INT             NULL,
    [SecondaryEmails]         NVARCHAR (2000) NULL,
    [FacebookUrl]             VARCHAR (2000)  NULL,
    [TwitterUrl]              VARCHAR (2000)  NULL,
    [GooglePlusUrl]           VARCHAR (2000)  NULL,
    [LinkedInUrl]             VARCHAR (2000)  NULL,
    [BlogUrl]                 VARCHAR (2000)  NULL,
    [WebSiteUrl]              VARCHAR (2000)  NULL,
    [FacebookAccessToken]     VARCHAR (2000)  NULL,
    [TwitterOAuthToken]       VARCHAR (2000)  NULL,
    [TwitterOAuthTokenSecret] VARCHAR (2000)  NULL);

