CREATE  PROCEDURE [dbo].[Monthly_Active_Accounts_Users_List]
(
  @AccountID  VARCHAR(MAX) 
)
AS
BEGIN 


  DECLARE @AccountIDS TABLE(AccountID INT)
 
	 INSERT INTO @AccountIDS (AccountID)
	 SELECT Datavalue  FROM dbo.Split (@AccountID ,',')

  DECLARE @NewId INT

	 SELECT TOP 1 @NewId = AccountID FROM @AccountIds


 
  WHILE @NewId IS NOT NULL

     BEGIN 
            select AccountName,(U.FirstName+U.LastName) AS Name,U.PrimaryEmail,RoleName FROM users u WITH (NOLOCK) 
			INNER JOIN Accounts A  WITH (NOLOCK)  ON A.AccountID = U.AccountID 
			INNER JOIN Roles R  WITH (NOLOCK) ON R.RoleID = U.RoleID 
			WHERE U.AccountID = @NewId AND  U.Isdeleted = 0 AND A.Isdeleted = 0 and U.STATUS = 1 AND A.STATUS = 1

			DELETE FROM @AccountIds WHERE AccountID = @NewId;
			SET @NewId = null;
			SELECT TOP 1 @NewId = AccountID FROM @AccountIds;      
	 END





END