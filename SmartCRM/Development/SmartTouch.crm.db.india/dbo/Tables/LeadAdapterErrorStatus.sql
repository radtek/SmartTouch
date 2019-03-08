CREATE TABLE [dbo].[LeadAdapterErrorStatus] (
    [LeadAdapterErrorStatusID] TINYINT      IDENTITY (1, 1) NOT NULL,
    [LeadAdapterErrorStatus]   VARCHAR (25) NOT NULL,
    CONSTRAINT [PK_LeadAdapterErrorStatus] PRIMARY KEY CLUSTERED ([LeadAdapterErrorStatusID] ASC) WITH (FILLFACTOR = 90)
);

