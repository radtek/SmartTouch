CREATE PROCEDURE [dbo].[Monthly_User_Activity_Report]
(
	@Date1  DATETIME,
	@Date2  DATETIME,
	@AccountID  VARCHAR(MAX) 
)
AS
BEGIN

 DECLARE @AccountIDS TABLE(AccountID INT)


	 INSERT INTO @AccountIDS (AccountID)
	 SELECT Datavalue  From dbo.Split (@AccountID ,',')


 DECLARE @NewId INT

     SELECT TOP 1 @NewId = AccountID FROM @AccountIds


 
WHILE @NewId IS NOT NULL

  BEGIN

		SELECT  AccountName,UserActivityLogID,EntityID,REPLACE(REPLACE(REPLACE(EntityName, CHAR(13), ''), CHAR(10), ''), CHAR(9), '') EntityName,UAL.UserActivityID,ActivityName,UAL.ModuleID,ModuleName,UAL.UserID,u.FirstName+u.LastName as UserName,LogDate
		FROM [dbo].[UserActivityLogs] UAL  WITH (NOLOCK)
		INNER JOIN Accounts A  WITH (NOLOCK) ON  A.AccountID = UAL.AccountID
		INNER JOIN Modules M  WITH (NOLOCK) ON M.ModuleID = UAL.ModuleID
		INNER JOIN Users U  WITH (NOLOCK) ON  UAL.UserID = U.UserID
		INNER JOIN UserActivities UA  WITH (NOLOCK) ON UA.UserActivityID = UAL.UserActivityID 
		WHERE UAL.Accountid = @NewId AND LogDate between  @Date1  and @Date2
		ORDER BY LogDate ASC
 
--PRINT @NewId

		DELETE FROM @AccountIds WHERE AccountID = @NewId;
		SET @NewId = null;
		SELECT TOP 1 @NewId = AccountID FROM @AccountIds;      
  END


END