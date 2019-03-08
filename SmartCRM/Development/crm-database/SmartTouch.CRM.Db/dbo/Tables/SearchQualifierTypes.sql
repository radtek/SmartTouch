CREATE TABLE [dbo].[SearchQualifierTypes] (
    [SearchQualifierTypeID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [QualifierName]         VARCHAR (60) NOT NULL,
    CONSTRAINT [PK_SearchQualifierTypes] PRIMARY KEY CLUSTERED ([SearchQualifierTypeID] ASC) WITH (FILLFACTOR = 90)
);

