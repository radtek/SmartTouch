CREATE TYPE [dbo].[CustomFieldList] AS TABLE (
    [MergeCodeID] INT           IDENTITY (1, 1) NOT NULL,
    [MergeCode]   VARCHAR (100) NULL,
    [FieldType]   VARCHAR (1)   NULL,
    [FieldID]     INT           NULL,
    PRIMARY KEY CLUSTERED ([MergeCodeID] ASC));

