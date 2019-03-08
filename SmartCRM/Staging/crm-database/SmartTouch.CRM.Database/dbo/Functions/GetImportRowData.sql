CREATE FUNCTION [dbo].[GetImportRowData]
(@url NVARCHAR (MAX) NULL)
RETURNS NVARCHAR (MAX)
AS
 EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].UserDefinedFunctions.GetImportRowData