CREATE TABLE [dbo].[ContactWebVisits](
	[ContactWebVisitID] [int] IDENTITY(1,1) NOT NULL,
	[ContactID] [int] NOT NULL,
	[PageVisited] [nvarchar](2100) NULL,
	[VisitedOn] [datetime] NOT NULL,
	[ProviderVisitID] [int] NULL,
	[Duration] [smallint] NULL,
	[IPAddress] [nvarchar](1000) NOT NULL,
	[IsVisit] [bit] NOT NULL,
	[VisitReference] [nvarchar](1000) NULL,
	[ContactReference] [nvarchar](1000) NULL,
	[City] [nvarchar](1000) NULL,
	[Region] [nvarchar](1000) NULL,
	[Country] [nvarchar](1000) NULL,
	[ISPName] [nvarchar](1000) NULL,
    CONSTRAINT [PK_ContactWebVisits] PRIMARY KEY CLUSTERED ([ContactWebVisitID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ContactWebVisits_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID]
    ON [dbo].[ContactWebVisits]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_ContactWebVisitID]
    ON [dbo].[ContactWebVisits]([ContactID] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [ProviderVisitID], [Duration], [IPAddress], [IsVisit]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_IPAddress]
    ON [dbo].[ContactWebVisits]([ContactID] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [Duration], [IPAddress], [IsVisit], [VisitReference], [ContactReference]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_ContactID_IsVisit]
    ON [dbo].[ContactWebVisits]([ContactID] ASC, [IsVisit] ASC)
    INCLUDE([ContactWebVisitID], [PageVisited], [VisitedOn], [ProviderVisitID], [Duration], [IPAddress]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO



CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_VisitedOn]
    ON [dbo].[ContactWebVisits]([VisitedOn] ASC)
    INCLUDE([ContactID], [Duration], [VisitReference], [City], [Region], [ISPName]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactWebVisits_VisitReference]
    ON [dbo].[ContactWebVisits]([VisitReference] ASC)
    INCLUDE([ContactID], [VisitedOn], [Duration], [City], [Region], [ISPName]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];

