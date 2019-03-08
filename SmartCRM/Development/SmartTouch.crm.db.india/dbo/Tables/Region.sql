CREATE TABLE [dbo].[Region] (
    [RegionId]     BIGINT        NOT NULL,
    [CountryId]    INT           NOT NULL,
    [RegionName]   VARCHAR (200) NOT NULL,
    [RegionCode]   VARCHAR (30)  NOT NULL,
    [Description]  VARCHAR (500) NULL,
    [Status]       BIT           NOT NULL,
    [CreatedBy]    VARCHAR (100) NOT NULL,
    [CreatedDate]  DATETIME      NOT NULL,
    [ModifiedBy]   VARCHAR (100) NULL,
    [ModifiedDate] DATETIME      NULL
);

