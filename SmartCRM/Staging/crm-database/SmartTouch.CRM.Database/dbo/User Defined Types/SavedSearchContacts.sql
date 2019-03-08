CREATE TYPE [dbo].[SavedSearchContacts] AS TABLE (
    [GroupID]            UNIQUEIDENTIFIER DEFAULT (newid()) NULL,
    [SearchDefinitionID] INT              NOT NULL,
    [ContactID]          INT              NOT NULL);

