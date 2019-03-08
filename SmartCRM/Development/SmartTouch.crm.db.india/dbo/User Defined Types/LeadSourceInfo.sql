CREATE TYPE [dbo].[LeadSourceInfo] AS TABLE (
    [UserID]         INT           NULL,
    [OwnerName]      VARCHAR (151) NULL,
    [LeadSourceName] VARCHAR (50)  NULL,
    [TotalContacts]  INT           NULL);

