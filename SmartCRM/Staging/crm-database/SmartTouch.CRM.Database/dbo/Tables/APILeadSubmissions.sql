CREATE TABLE [dbo].[APILeadSubmissions] (
    [APILeadSubmissionID] INT           IDENTITY (1, 1) NOT NULL,
    [ContactID]           INT           NULL,
    [AccountID]           INT           NOT NULL,
    [OwnerID]             INT           NULL,
    [SubmittedData]       VARCHAR (MAX) NOT NULL,
    [SubmittedOn]         DATETIME      NULL,
    [IsProcessed]         TINYINT       NOT NULL,
    [Remarks]             VARCHAR (MAX) NULL,
    [FormID]              INT           DEFAULT ((0)) NOT NULL,
    [IPAddress]           NVARCHAR (15) NULL,
    [FieldUpdatedOn]      DATETIME      NULL,
    [FieldUpdatedBy]      INT           NULL,
    PRIMARY KEY CLUSTERED ([APILeadSubmissionID] ASC) WITH (FILLFACTOR = 90),
    FOREIGN KEY ([FieldUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

