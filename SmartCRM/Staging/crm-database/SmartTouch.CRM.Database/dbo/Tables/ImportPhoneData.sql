CREATE TABLE [dbo].[ImportPhoneData] (
    [ImportPhoneDataID] INT              IDENTITY (1, 1) NOT NULL,
    [PhoneType]         INT              NULL,
    [PhoneNumber]       NVARCHAR (50)    NULL,
    [ReferenceID]       UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_ImportPhoneData] PRIMARY KEY CLUSTERED ([ImportPhoneDataID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportPhoneData_missing_92]
    ON [dbo].[ImportPhoneData]([ReferenceID] ASC)
    INCLUDE([ImportPhoneDataID], [PhoneType], [PhoneNumber]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

