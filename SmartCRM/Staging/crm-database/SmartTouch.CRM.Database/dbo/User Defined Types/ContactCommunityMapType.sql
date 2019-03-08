CREATE TYPE [dbo].[ContactCommunityMapType] AS TABLE (
    [ContactCommunityMapID] INT      NOT NULL,
    [ContactId]             INT      NOT NULL,
    [CommunityID]           SMALLINT NOT NULL,
    [CreatedOn]             DATETIME NULL,
    [CreatedBy]             INT      NULL,
    [LastModifiedOn]        DATETIME NULL,
    [LastModifiedBy]        INT      NULL,
    [IsDeleted]             BIT      NOT NULL);

