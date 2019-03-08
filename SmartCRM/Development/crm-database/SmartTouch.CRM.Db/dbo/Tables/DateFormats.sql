CREATE TABLE [dbo].[DateFormats] (
    [DateFormatID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [FormatName]   VARCHAR (200) NOT NULL,
    CONSTRAINT [PK_DateFormats] PRIMARY KEY CLUSTERED ([DateFormatID] ASC) WITH (FILLFACTOR = 90)
);

