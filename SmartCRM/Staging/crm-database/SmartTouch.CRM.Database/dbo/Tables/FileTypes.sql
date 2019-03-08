﻿CREATE TABLE [dbo].[FileTypes] (
    [FileTypeID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [FileTypeName] NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_FileTypes] PRIMARY KEY CLUSTERED ([FileTypeID] ASC) WITH (FILLFACTOR = 90)
);

