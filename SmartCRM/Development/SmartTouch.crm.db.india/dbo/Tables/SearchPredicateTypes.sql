CREATE TABLE [dbo].[SearchPredicateTypes] (
    [SearchPredicateTypeID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [PredicateType]         VARCHAR (10) NOT NULL,
    CONSTRAINT [PK_SearchPredicateTypes] PRIMARY KEY CLUSTERED ([SearchPredicateTypeID] ASC) WITH (FILLFACTOR = 90)
);

