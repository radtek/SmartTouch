CREATE TABLE [dbo].[TextRegistration] (
    [TextRegistrationID] INT              IDENTITY (1, 1) NOT NULL,
    [Guid]               UNIQUEIDENTIFIER NOT NULL,
    [Name]               NVARCHAR (75)    NULL,
    [Address]            NVARCHAR (75)    NULL,
    [APIKey]             NVARCHAR (75)    NULL,
    [Token]              NVARCHAR (75)    NULL,
    [UserName]           NVARCHAR (75)    NULL,
    [Password]           NVARCHAR (75)    NULL,
    [TextProviderID]     TINYINT          NOT NULL,
    CONSTRAINT [PK_TextRegistration] PRIMARY KEY CLUSTERED ([TextRegistrationID] ASC) WITH (FILLFACTOR = 90)
);

