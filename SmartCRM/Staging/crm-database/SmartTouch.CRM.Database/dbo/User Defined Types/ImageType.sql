CREATE TYPE [dbo].[ImageType] AS TABLE (
    [ImageID]      INT             NULL,
    [FriendlyName] NVARCHAR (2000) NULL,
    [StorageName]  VARCHAR (2000)  NULL,
    [OriginalName] NVARCHAR (2000) NULL,
    [CreatedBy]    INT             NULL,
    [CreatedDate]  DATETIME        NULL,
    [CategoryId]   TINYINT         NULL,
    [AccountID]    INT             NULL);

