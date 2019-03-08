CREATE TABLE [dbo].[ReceivedMailInfo] (
    [ReceivedMailID]   INT            IDENTITY (1, 1) NOT NULL,
    [FromEmail]        VARCHAR (500)  NOT NULL,
    [Recipient]        VARCHAR (500)  NOT NULL,
    [RecipientType]    SMALLINT       NOT NULL,
    [Subject]          VARCHAR (1000) NOT NULL,
    [Body]             VARCHAR (MAX)  NOT NULL,
    [ReferenceID]      NVARCHAR (50)  NOT NULL,
    [ReceivedOn]       DATETIME       NOT NULL,
    [TrackedOn]        DATETIME       NOT NULL,
    [EmailReferenceID] NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_ReceivedMailID] PRIMARY KEY CLUSTERED ([ReceivedMailID] ASC)
);

