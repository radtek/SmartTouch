CREATE TABLE [dbo].[SubmittedFormData] (
    [SubmittedFormDataID] INT            IDENTITY (1, 1) NOT NULL,
    [FormID]              INT            NOT NULL,
    [AccountID]           INT            NOT NULL,
    [IPAddress]           NVARCHAR (MAX) NOT NULL,
    [CreatedOn]           DATETIME       NOT NULL,
    [Status]              INT            NOT NULL,
    [LeadSourceID]        SMALLINT       NOT NULL,
    [STITrackingID]       NVARCHAR (MAX) NULL,
    [CreatedBy]           INT            NULL,
    [OwnerID]             INT            NULL,
    [Remarks]             VARCHAR (MAX)  NULL,
    [FormSubmissionID]    INT            NULL,
    [FieldUpdatedOn]      DATETIME       NULL,
    [FieldUpdatedBy]      INT            NULL,
    CONSTRAINT [PK_SubmittedFormData] PRIMARY KEY CLUSTERED ([SubmittedFormDataID] ASC) WITH (FILLFACTOR = 90),
    FOREIGN KEY ([FieldUpdatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    FOREIGN KEY ([FormSubmissionID]) REFERENCES [dbo].[FormSubmissions] ([FormSubmissionID])
);

