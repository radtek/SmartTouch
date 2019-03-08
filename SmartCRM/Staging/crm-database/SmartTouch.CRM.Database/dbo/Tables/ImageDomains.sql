CREATE TABLE [dbo].[ImageDomains] (
    [ImageDomainID]  TINYINT       IDENTITY (1, 1) NOT NULL,
    [ImageDomain]    VARCHAR (256) NOT NULL,
    [Status]         BIT           NOT NULL,
    [IsDefault]      BIT           NOT NULL,
    [CreatedBy]      INT           NOT NULL,
    [CreatedOn]      DATETIME      NOT NULL,
    [LastModifiedBy] INT           NULL,
    [LastModifiedOn] DATETIME      NULL,
    CONSTRAINT [PK_ImageDomains] PRIMARY KEY CLUSTERED ([ImageDomainID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ImageDomains_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ImageDomains_Users1] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);

