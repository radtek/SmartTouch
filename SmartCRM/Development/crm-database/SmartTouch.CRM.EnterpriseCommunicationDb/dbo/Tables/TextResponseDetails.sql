CREATE TABLE [dbo].[TextResponseDetails] (
    [TextResponseDetailsID] INT           IDENTITY (1, 1) NOT NULL,
    [TextResponseID]        INT           NOT NULL,
    [From]                  VARCHAR (75)  NULL,
    [To]                    VARCHAR (MAX) NULL,
    [SenderID]              VARCHAR (75)  NULL,
    [Message]               VARCHAR (200) NULL,
    [Status]                TINYINT       NULL,
    [ServiceResponse]       VARCHAR (200) NULL,
    CONSTRAINT [PK_TextResponseDetails] PRIMARY KEY CLUSTERED ([TextResponseDetailsID] ASC),
    CONSTRAINT [FK_TextResponseDetails_CommunicationStatus] FOREIGN KEY ([Status]) REFERENCES [dbo].[CommunicationStatus] ([CommunicationStatusID]),
    CONSTRAINT [FK_TextResponseDetails_TextResponse] FOREIGN KEY ([TextResponseID]) REFERENCES [dbo].[TextResponse] ([TextResponseID]),
    CONSTRAINT [FK_TextResponseDetails_TextResponseDetails] FOREIGN KEY ([TextResponseDetailsID]) REFERENCES [dbo].[TextResponseDetails] ([TextResponseDetailsID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TextResponseDetails_TextResponseID]
    ON [dbo].[TextResponseDetails]([TextResponseID] ASC)
    INCLUDE([From], [To], [SenderID], [Message], [Status], [ServiceResponse]);

