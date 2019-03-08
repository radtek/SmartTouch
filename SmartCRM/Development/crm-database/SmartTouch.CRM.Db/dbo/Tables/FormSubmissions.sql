CREATE TABLE [dbo].[FormSubmissions] (
    [FormSubmissionID] INT            IDENTITY (1, 1) NOT NULL,
    [FormID]           INT            NOT NULL,
    [ContactID]        INT            NOT NULL,
    [IPAddress]        NVARCHAR (50)  NULL,
    [SubmittedOn]      DATETIME       NOT NULL,
    [StatusID]         SMALLINT       NOT NULL,
    [SubmittedData]    NVARCHAR (MAX) NULL,
    [LeadSourceID]     SMALLINT       NULL,
    CONSTRAINT [PK_FormSubmissions] PRIMARY KEY CLUSTERED ([FormSubmissionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_FormSubmissions_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_FormSubmissions_DropdownValues] FOREIGN KEY ([LeadSourceID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_FormSubmissions_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID]),
    CONSTRAINT [FK_FormSubmissions_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_FormSubmissions_ContactID]
    ON [dbo].[FormSubmissions]([ContactID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_FormSubmissions_FormID]
    ON [dbo].[FormSubmissions]([FormID] ASC)
    INCLUDE([ContactID], [SubmittedOn], [LeadSourceID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_FormSubmissions_FormID_ContactID]
    ON [dbo].[FormSubmissions]([FormID] ASC)
    INCLUDE([FormSubmissionID], [ContactID], [IPAddress], [SubmittedOn], [StatusID], [SubmittedData], [LeadSourceID]) WITH (FILLFACTOR = 90);


GO


