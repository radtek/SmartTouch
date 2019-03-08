CREATE PROCEDURE [dbo].[ValidateUserLimit]
(
	@AccountId INT,
	@UserIds dbo.Contact_List ReadOnly,
	@TargetRoleId INT,
	@UserLimit INT
)
AS
BEGIN
	DECLARE @Users TABLE (Id INT, RoleId SMALLINT, Excluded BIT)


	DECLARE @ExcludedRoles TABLE (Id INT)
	DECLARE @ER NVARCHAR(MAX)
	DECLARE @IsRoleExcluded BIT = 0
	DECLARE @LimitExceeded bit=0
	
	SELECT @ER = ExcludedRoles FROM SubscriptionModuleMap (NOLOCK) WHERE AccountId = @AccountId AND ModuleID = 2

	INSERT INTO @ExcludedRoles
	SELECT DataValue FROM dbo.Split(@ER, ',')

	SELECT @IsRoleExcluded = 1 FROM @ExcludedRoles WHERE Id = @TargetRoleId

	INSERT INTO @Users
	SELECT u.UserId,  u.[RoleId], IIF(ER.Id > 0,1, 0) AS Excluded
	FROM Users (NOLOCK) U
	LEFT JOIN @ExcludedRoles ER ON ER.Id = u.RoleID
	WHERE u.AccountID IN (@AccountId) AND U.IsDeleted = 0 AND u.[Status] = 1

	UPDATE @Users
	SET RoleId = @TargetRoleId, Excluded = @IsRoleExcluded
	WHERE Id IN (SELECT ContactId FROM @UserIds)
	DECLARE @Limit INT

	

	SELECT @Limit = COUNT(1) FROM @Users WHERE Excluded = 0
	
	IF (@Limit > @UserLimit)
		BEGIN
			SET @LimitExceeded=1
		END
	ELSE
		BEGIN
			SET @LimitExceeded=0
		END

	SELECT @LimitExceeded
END

/*
	DECLARE @tbl dbo.Contact_List
	INSERT INTO @tbl SELECT 8980 UNION SELECT 7999
	EXEC dbo.ValidateUserLimit 4218, @tbl, 1011 ,3
*/

