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
	[Remarks] [varchar](MAX) NULL,
    CONSTRAINT [PK_SubmittedFormData] PRIMARY KEY CLUSTERED ([SubmittedFormDataID] ASC) WITH (FILLFACTOR = 90)
);
GO

CREATE NONCLUSTERED INDEX [IX_SubmittedFormData_Status]
    ON [dbo].[SubmittedFormData]([Status] ASC) WITH (FILLFACTOR = 90);
	GO

Create NonClustered Index IX_SubmittedFormData_AccountID On [dbo].[SubmittedFormData] ([AccountID]) Include ([IPAddress], [CreatedOn]);
GO