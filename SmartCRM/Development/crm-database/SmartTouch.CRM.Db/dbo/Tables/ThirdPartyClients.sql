CREATE TABLE [dbo].[ThirdPartyClients] (
    [ID]                   VARCHAR (200)  NOT NULL,
    [Name]                 VARCHAR (200)  NOT NULL,
    [IsActive]             BIT            NOT NULL,
    [RefreshTokenLifeTime] INT            NOT NULL,
    [AllowedOrigin]        VARCHAR (1024) NOT NULL,
    [LastUpdatedBy]        INT            NOT NULL,
    [LastUpdatedOn]        DATETIME       NOT NULL,
    [AccountID]            INT            NOT NULL,
    CONSTRAINT [PK_ThirdPartyClients] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ThirdPartyClients_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_ThirdPartyClients_Users] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

