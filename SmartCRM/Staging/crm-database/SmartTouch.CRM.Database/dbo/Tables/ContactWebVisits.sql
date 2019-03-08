CREATE TABLE [dbo].[ContactWebVisits] (
    [ContactWebVisitID] INT             IDENTITY (1, 1) NOT NULL,
    [ContactID]         INT             NOT NULL,
    [PageVisited]       NVARCHAR (2100) NULL,
    [VisitedOn]         DATETIME        NOT NULL,
    [ProviderVisitID]   INT             NULL,
    [Duration]          SMALLINT        NULL,
    [IPAddress]         NVARCHAR (1000) NOT NULL,
    [IsVisit]           BIT             NOT NULL,
    [VisitReference]    NVARCHAR (1000) NULL,
    [ContactReference]  NVARCHAR (1000) NULL,
    [City]              NVARCHAR (1000) NULL,
    [Region]            NVARCHAR (1000) NULL,
    [Country]           NVARCHAR (1000) NULL,
    [ISPName]           NVARCHAR (1000) NULL,
    CONSTRAINT [PK_ContactWebVisits] PRIMARY KEY CLUSTERED ([ContactWebVisitID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ContactWebVisits_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID]
    ON [dbo].[ContactWebVisits]([ContactID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_ContactWebVisitID]
    ON [dbo].[ContactWebVisits]([ContactID] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [ProviderVisitID], [Duration], [IPAddress], [IsVisit]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_IPAddress]
    ON [dbo].[ContactWebVisits]([ContactID] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [Duration], [IPAddress], [IsVisit], [VisitReference], [ContactReference]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_IsVisit]
    ON [dbo].[ContactWebVisits]([ContactID] ASC, [IsVisit] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [ProviderVisitID], [Duration], [IPAddress]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_VisitedOn]
    ON [dbo].[ContactWebVisits]([VisitedOn] ASC)
    INCLUDE([ContactID], [Duration], [VisitReference], [City], [Region], [ISPName]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_VisitReference]
    ON [dbo].[ContactWebVisits]([VisitReference] ASC)
    INCLUDE([ContactID], [VisitedOn], [Duration], [City], [Region], [ISPName]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

