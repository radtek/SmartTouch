﻿CREATE TABLE [dbo].[TagTypes] (
    [TagTypeID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [TagTypeName] NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_TagTypes] PRIMARY KEY CLUSTERED ([TagTypeID] ASC) WITH (FILLFACTOR = 90)
);

