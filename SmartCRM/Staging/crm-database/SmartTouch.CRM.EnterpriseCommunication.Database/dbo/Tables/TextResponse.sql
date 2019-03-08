CREATE TABLE [dbo].[TextResponse] (
    [TextResponseID] INT              IDENTITY (1, 1) NOT NULL,
    [Token]          UNIQUEIDENTIFIER NULL,
    [RequestGuid]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]    DATETIME         NOT NULL,
    CONSTRAINT [PK_TextResponse] PRIMARY KEY CLUSTERED ([TextResponseID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_TextResponse_RequestGuid]
    ON [dbo].[TextResponse]([RequestGuid] ASC)
    INCLUDE([TextResponseID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

