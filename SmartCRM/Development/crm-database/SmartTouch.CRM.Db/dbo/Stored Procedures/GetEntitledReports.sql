CREATE PROCEDURE [dbo].[GetEntitledReports]
(
	@AccountID INT,
	@Modules VARCHAR(MAX),
	@UserID INT
)
AS
BEGIN
	DECLARE @EntitledReports TABLE (ReportType INT)

	IF @AccountId > 1
		BEGIN
			INSERT INTO @EntitledReports
			SELECT ReportType FROM ReportModuleMap (NOLOCK) WHERE ModuleID IN (SELECT DataValue FROM dbo.Split(@Modules,','))
			UNION
			SELECT 13 UNION SELECT 14
		END
	ELSE
		BEGIN
			INSERT INTO @EntitledReports
			SELECT 21 UNION SELECT 22 UNION SELECT 23 UNION SELECT 24
		END

	SELECT Id, ReportName, ReportType, MAX(LastRunOn)  LastRunOn 
	FROM (
		SELECT R.ReportId AS Id, R.ReportName, R.ReportType, UAL.LogDate AS LastRunOn
		FROM Reports (NOLOCK) R
		INNER JOIN UserActivityLogs (NOLOCK) UAL ON UAL.EntityID = R.ReportID AND R.AccountID = UAL.AccountId
		INNER JOIN @EntitledReports ER ON ER.ReportType = R.ReportType
		WHERE UAL.AccountID = @AccountID AND UAL.UserID = @UserId AND UAL.ModuleID = 24 AND UAL.UserActivityID = 6
		UNION
		SELECT R.ReportId, R.ReportName, R.ReportType, NULL LogDate FROM Reports (NOLOCK) R 
		INNER JOIN @EntitledReports ER ON ER.ReportType = R.ReportType
		WHERE R.AccountID = @AccountID) x
	GROUP BY Id, ReportName, ReportType
END