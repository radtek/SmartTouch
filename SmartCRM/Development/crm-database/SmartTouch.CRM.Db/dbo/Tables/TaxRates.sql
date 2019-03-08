CREATE TABLE [dbo].[TaxRates] (
    [TaxRateId]              BIGINT        NOT NULL,
    [ZIPCode]                VARCHAR (20)  NULL,
    [CountyName]             VARCHAR (200) NULL,
    [StateCode]              CHAR (10)     NULL,
    [CityName]               VARCHAR (200) NULL,
    [CountySalesTax]         FLOAT (53)    NULL,
    [CountyUseTax]           FLOAT (53)    NULL,
    [StateSalesTax]          FLOAT (53)    NULL,
    [StateUseTax]            FLOAT (53)    NULL,
    [CitySalesTax]           FLOAT (53)    NULL,
    [CityUseTax]             FLOAT (53)    NULL,
    [TotalSalesTax]          FLOAT (53)    NULL,
    [TotalUseTax]            FLOAT (53)    NULL,
    [TaxShippingAlone]       BIT           NULL,
    [ShippingAndHandlingTax] BIT           NULL,
    [CountryID]              BIGINT        NULL,
    [CountryName]            VARCHAR (50)  NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_TaxRates_ZIPCode]
    ON [dbo].[TaxRates]([ZIPCode] ASC) WITH (FILLFACTOR = 90);

