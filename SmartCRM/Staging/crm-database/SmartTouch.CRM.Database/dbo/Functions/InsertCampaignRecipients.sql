CREATE FUNCTION [dbo].[InsertCampaignRecipients] (@url NVARCHAR (4000) NULL)
RETURNS NVARCHAR (4000)
AS
  EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].UserDefinedFunctions.InsertCampaignRecipients