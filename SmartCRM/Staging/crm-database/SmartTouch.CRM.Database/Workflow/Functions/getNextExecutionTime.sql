CREATE FUNCTION [Workflow].[getNextExecutionTime]
(@timerType TINYINT NULL, @delayPeriod INT NULL, @delayUnit TINYINT NULL, @runOn TINYINT NULL, @runAt DATETIME NULL, @runType TINYINT NULL, @runOnDate DATETIME NULL, @startDate DATETIME NULL, @endDate DATETIME NULL, @runOnDay NVARCHAR (MAX) NULL, @daysOfWeek NVARCHAR (MAX) NULL, @previousActionTime DATETIME NULL)
RETURNS DATETIME
AS
 EXTERNAL NAME [SmartTouch.CRM.SqlClrDb].UserDefinedFunctions.GetNextTime