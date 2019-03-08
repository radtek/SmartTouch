CREATE TABLE [dbo].[ImportPhoneData] (
    [ImportPhoneDataID] INT              IDENTITY (1, 1) NOT NULL,
    [PhoneType]         INT              NULL,
    [PhoneNumber]       NVARCHAR (50)    NULL,
    [ReferenceID]       UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_ImportPhoneData] PRIMARY KEY CLUSTERED ([ImportPhoneDataID] ASC) WITH (FILLFACTOR = 90)
);

