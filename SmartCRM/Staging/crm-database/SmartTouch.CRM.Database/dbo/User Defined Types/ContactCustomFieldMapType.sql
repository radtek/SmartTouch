CREATE TYPE [dbo].[ContactCustomFieldMapType] AS TABLE (
    [ContactCustomFieldMapId] INT            NULL,
    [ContactId]               INT            NULL,
    [CustomFieldId]           INT            NULL,
    [Value]                   NVARCHAR (MAX) NULL);

