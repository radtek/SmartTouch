CREATE PROCEDURE [dbo].[GET_LogIn_Frequency_Report]
(
	@StartDate DATETIME,
	@EndDate DATETIME ,
	@AccountIds Nvarchar(max),
	@LoginReportType TINYINT
)
AS
BEGIN


DECLARE @AccountID TABLE(AccountID INT)

INSERT INTO @AccountID (AccountID)
SELECT Datavalue  From dbo.Split (@AccountIds ,',')


	-- Most Active Users ------
	IF(@LoginReportType = 1)
	BEGIN
		select * INTO #MostActiveUsers
		FROM
		(
			SELECT DISTINCT a.AccountName,u.[FirstName],u.[LastName],u.PrimaryEmail AS Email,MAX([AuditedOn])AS RecentLoginDate,Count(LA.LoginAuditID) AS [LoggedInCount]
			FROM [dbo].[LoginAudit]  LA 
			INNER JOIN Users U ON LA.[UserID] = U.[UserID]
			INNER JOIN Accounts A on A.AccountID = U.AccountID
			INNER JOIN  @AccountID AA  ON  A.AccountID = AA.AccountID
			WHERE [AuditedOn] BETWEEN @StartDate AND @EndDate AND U.AccountID = AA.AccountID AND LA.AccountID = AA.AccountID AND LA.SignInActivity = 1
			AND U.Status = 1
			GROUP BY a.AccountName,u.[FirstName],u.[LastName],u.PrimaryEmail
		)T

		SELECT * FROM #MostActiveUsers M ORDER BY RecentLoginDate DESC
	END

	--- Most Recent Active Users ------

	ELSE 
	BEGIN
			;WITH ActiveUsers AS
			(
				SELECT DISTINCT MAX([AuditedOn]) AS RecentLoginDate, LA.AccountID, La.UserID
				FROM [dbo].[LoginAudit]  LA 
				INNER JOIN Accounts A on A.AccountID = LA.AccountID
			    INNER JOIN  @AccountID AA  ON  A.AccountID = AA.AccountID
				WHERE [AuditedOn] BETWEEN @StartDate AND @EndDate AND LA.AccountID = AA.AccountID AND LA.SignInActivity = 1
				GROUP BY LA.UserID, LA.AccountID
			)

			SELECT a.AccountName ,U.[FirstName],U.[LastName],U.PrimaryEmail AS Email, AU.RecentLoginDate FROM ActiveUsers AU
			JOIN Users (NOLOCK) U ON AU.UserID = U.UserID
			INNER JOIN Accounts A on A.AccountID = U.AccountID
			 INNER JOIN  @AccountID AA  ON  A.AccountID = AA.AccountID
			WHERE U.AccountID = AA.AccountID
		    ORDER BY AU.RecentLoginDate DESC
	END

END
GO

