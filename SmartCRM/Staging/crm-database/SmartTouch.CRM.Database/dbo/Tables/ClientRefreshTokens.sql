CREATE TABLE [dbo].[ClientRefreshTokens] (
    [ID]                 VARCHAR (200) NOT NULL,
    [IssuedTo]           INT           NOT NULL,
    [ThirdPartyClientID] VARCHAR (200) NOT NULL,
    [IssuedOn]           DATETIME      NOT NULL,
    [ExpiresOn]          DATETIME      NOT NULL,
    [ProtectedTicket]    VARCHAR (MAX) NOT NULL,
    [LastUpdatedBy]      INT           NOT NULL,
    [LastUpdatedOn]      DATETIME      NOT NULL,
    CONSTRAINT [PK_ClientRefreshTokens] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ClientRefreshTokens_ThirdPartyClients] FOREIGN KEY ([ThirdPartyClientID]) REFERENCES [dbo].[ThirdPartyClients] ([ID]),
    CONSTRAINT [FK_ClientRefreshTokens_Users] FOREIGN KEY ([IssuedTo]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ClientRefreshTokens_Users1] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

