CREATE TABLE [dbo].[DropdownValueTypes] (
    [DropdownValueTypeID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [DropdownValueType]   SMALLINT     NOT NULL,
    [DefaultDescription]  VARCHAR (50) NULL,
    CONSTRAINT [PK_DropdownValueTypes] PRIMARY KEY CLUSTERED ([DropdownValueTypeID] ASC) WITH (FILLFACTOR = 90)
);

