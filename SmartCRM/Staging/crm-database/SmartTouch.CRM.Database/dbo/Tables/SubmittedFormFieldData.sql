CREATE TABLE [dbo].[SubmittedFormFieldData] (
    [SubmittedFormFieldDataID] INT            IDENTITY (1, 1) NOT NULL,
    [SubmittedFormDataID]      INT            NOT NULL,
    [Field]                    NVARCHAR (MAX) NOT NULL,
    [Value]                    NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_SubmittedFormFieldData] PRIMARY KEY CLUSTERED ([SubmittedFormFieldDataID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_SubmittedFormFieldData_missing_11]
    ON [dbo].[SubmittedFormFieldData]([SubmittedFormDataID] ASC);

