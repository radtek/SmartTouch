CREATE TYPE [dbo].[ContactLeadSourceMapType] AS TABLE (
    [ContactLeadSourceMapID] INT      NULL,
    [ContactId]              INT      NOT NULL,
    [LeadSouceID]            SMALLINT NOT NULL,
    [IsPrimary]              BIT      NOT NULL,
    [LastUpdatedDate]        DATETIME NOT NULL);

