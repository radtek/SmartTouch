CREATE TABLE [dbo].[ServiceProviders] (
    [ServiceProviderID]   INT              IDENTITY (1, 1) NOT NULL,
    [CommunicationTypeID] TINYINT          NOT NULL,
    [LoginToken]          UNIQUEIDENTIFIER NOT NULL,
    [CreatedBy]           INT              NOT NULL,
    [CreatedDate]         DATETIME         NOT NULL,
    [AccountID]           INT              NOT NULL,
    [ProviderName]        NVARCHAR (50)    NULL,
    [EmailType]           TINYINT          NOT NULL,
    [IsDefault]           BIT              NOT NULL,
    [SenderPhoneNumber]   VARCHAR (20)     NULL,
    [ImageDomainID]       TINYINT          NULL,
    CONSTRAINT [PK_ServiceProviders] PRIMARY KEY CLUSTERED ([ServiceProviderID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ServiceProviders_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID])
);

