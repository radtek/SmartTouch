CREATE TABLE [dbo].[Communities] (
    [CommunityID]   INT           IDENTITY (1, 1) NOT NULL,
    [CommunityName] NVARCHAR (75) NOT NULL,
    [AccountID]     INT           NOT NULL,
    [Street]        NVARCHAR (60) NULL,
    [City]          VARCHAR (35)  NOT NULL,
    [State]         VARCHAR (50)  NOT  NULL,
    [Country]       NCHAR (2)     NOT NULL,
    CONSTRAINT [PK_Communities] PRIMARY KEY CLUSTERED ([CommunityID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Communities_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Communities_Countries] FOREIGN KEY ([Country]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_Communities_States] FOREIGN KEY ([State]) REFERENCES [dbo].[States] ([StateID])
);

