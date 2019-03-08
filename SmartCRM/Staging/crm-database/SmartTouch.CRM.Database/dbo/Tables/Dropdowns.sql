CREATE TABLE [dbo].[Dropdowns] (
    [DropdownID]   TINYINT      IDENTITY (1, 1) NOT NULL,
    [DropdownName] VARCHAR (25) NOT NULL,
    CONSTRAINT [PK_Dropdowns] PRIMARY KEY CLUSTERED ([DropdownID] ASC) WITH (FILLFACTOR = 90)
);

