CREATE TABLE [dbo].[Currencies] (
    [CurrencyID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Symbol]     NVARCHAR (20) NOT NULL,
    [Format]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED ([CurrencyID] ASC) WITH (FILLFACTOR = 90)
);

