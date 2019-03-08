CREATE TYPE [dbo].[ReportInfo] AS TABLE (
    [ID]             INT           NULL,
    [GridValue]      VARCHAR (255) NULL,
    [ContactID]      INT           NULL,
    [DateVal]        DATE          NULL,
    [DateRange]      VARCHAR (1)   NULL,
    [GroupDateRange] INT           NULL);

