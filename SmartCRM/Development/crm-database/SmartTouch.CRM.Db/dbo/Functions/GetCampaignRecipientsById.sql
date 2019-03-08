CREATE FUNCTION [dbo].[GetCampaignRecipientsById]
(@url NVARCHAR (MAX) NULL)
RETURNS 
     TABLE (
        [ContactId] INT NULL)
AS
 EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].[UserDefinedFunctions].[GetCampaignRecipientsById]
GO
