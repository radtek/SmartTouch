--USE SmartCRM
--GO


--ALTER DATABASE [SmartCRM] SET TRUSTWORTHY ON

--DROP FUNCTION [Workflow].[getNextExecutionTime]
--DROP FUNCTION [dbo].[GetCampaignRecipientsById]
--DROP FUNCTION [dbo].[GetImportRowData]
--DROP FUNCTION [dbo].[InsertCampaignRecipients]

--DROP ASSEMBLY [SmartTouch.CRM.SqlClrDb]



----==================================================================================================================

--USE SmartCRM
--GO


--CREATE ASSEMBLY [SmartTouch.CRM.SqlClrDb]
--    AUTHORIZATION [dbo]
--    FROM 'E:\MSSQL\Sqlclr\SmartTouch.CRM.SqlClrDb.dll'
--    WITH PERMISSION_SET = Unsafe;

--GO

--CREATE FUNCTION [Workflow].[getNextExecutionTime]
--(@timerType TINYINT, @delayPeriod INT, @delayUnit TINYINT, @runOn TINYINT, @runAt DATETIME, @runType TINYINT, @runOnDate DATETIME, @startDate DATETIME, @endDate DATETIME, @runOnDay NVARCHAR (MAX), @daysOfWeek NVARCHAR (MAX), @previousActionTime DATETIME)
--RETURNS DATETIME
--AS
-- EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].[UserDefinedFunctions].[GetNextTime]

-- GO
--CREATE FUNCTION [dbo].[GetCampaignRecipientsById]
--(@url NVARCHAR (MAX))
--RETURNS Table(ContactId INT)
--AS
-- EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].[UserDefinedFunctions].[GetCampaignRecipientsById]


-- GO
-- CREATE FUNCTION [dbo].[GetImportRowData]
--(@url NVARCHAR (MAX))
--returns NVARCHAR (MAX)
--AS
-- EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].[UserDefinedFunctions].[GetImportRowData]

-- GO


--CREATE FUNCTION [dbo].[InsertCampaignRecipients] (@url [nvarchar](4000))
--RETURNS [nvarchar](4000)
--AS EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].[UserDefinedFunctions].[InsertCampaignRecipients];

----GO