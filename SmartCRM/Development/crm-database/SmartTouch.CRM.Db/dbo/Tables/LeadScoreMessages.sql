CREATE TABLE [dbo].[LeadScoreMessages] (
    [LeadScoreMessageID]       UNIQUEIDENTIFIER NOT NULL,
    [EntityID]                 INT              NOT NULL,
    [UserID]                   INT              NOT NULL,
    [LeadScoreConditionType]   TINYINT          NOT NULL,
    [ContactID]                INT              NOT NULL,
    [AccountID]                INT              NOT NULL,
    [LinkedEntityID]           INT              NULL,
    [ConditionValue]           NVARCHAR (1024)  NULL,
    [CreatedOn]                DATETIME         NOT NULL,
    [ProcessedOn]              DATETIME         NULL,
    [Remarks]                  VARCHAR (5000)   NULL,
    [LeadScoreProcessStatusID] SMALLINT         CONSTRAINT [DF__LeadScore__LeadS__1EEF72A2] DEFAULT ((701)) NULL,
    CONSTRAINT [PK_LeadScoreMessages] PRIMARY KEY CLUSTERED ([LeadScoreMessageID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadScoreMessages_Statuses] FOREIGN KEY ([LeadScoreProcessStatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreMessages_LeadScoreProcessStatusID]
    ON [dbo].[LeadScoreMessages]([LeadScoreProcessStatusID] ASC) WITH (FILLFACTOR = 90);

