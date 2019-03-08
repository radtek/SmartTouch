
CREATE PROC [dbo].[Account_Default_Roles_v1](
	@AccountID int,
	@AddRemaining bit = 0
)
AS
BEGIN

	DECLARE @TotalMappings int, @IsAccountExists bit

	SET NOCOUNT ON

	SELECT	@IsAccountExists = count(*)
	FROM	dbo.Accounts(NOLOCK)
	WHERE	AccountID = @AccountID

	IF(@IsAccountExists = 1)
	BEGIN
		SELECT	@TotalMappings = count(*)
		FROM	dbo.Roles(NOLOCK)
		WHERE	AccountID = @AccountID

		IF(isnull(@TotalMappings, 0) = 0)
		BEGIN
			-- ADD all default roles to new account
			INSERT INTO dbo.Roles(
				RoleName, AccountID
			)
			SELECT	RoleName, @AccountID
			FROM	dbo.Roles(NOLOCK)
			WHERE	AccountID IS NULL

			-- MAP all default modules to newly created roles
			INSERT INTO dbo.RoleModuleMap(
				RoleID,ModuleID
			)
			SELECT	r1.RoleID, m.ModuleID
			FROM	Roles(NOLOCK) r inner join RoleModuleMap(NOLOCK) rm on rm.RoleID = r.RoleID
					inner join Modules(NOLOCK) m on m.ModuleID = rm.ModuleID
					inner join Roles(NOLOCK) r1 on r.RoleName = r1.RoleName and r1.AccountID = @AccountID
			WHERE	isnull(r.AccountID, 0) = 0

			SELECT	1 Status, 'Successfully created roles' StatusMessage
		END
		ELSE
		BEGIN
			IF(@AddRemaining = 1)
			BEGIN
				-- ADD all default roles to new account
				INSERT INTO dbo.Roles(
					RoleName, AccountID
				)
				SELECT	r.RoleName, @AccountID AccountID 
				FROM	dbo.Roles(NOLOCK) r LEFT JOIN dbo.roles(NOLOCK) r1 ON r.RoleName = r1.RoleName AND r1.AccountID = @AccountID
				WHERE	ISNULL(r.AccountID, 0) = 0
						AND ISNULL(r1.AccountID, 0) = 0

				INSERT INTO dbo.RoleModuleMap(
					RoleID,ModuleID
				)
				SELECT	tmp.RoleID, tmp.ModuleID
				FROM	(
							SELECT	r1.RoleID, m.ModuleID
							FROM	Roles(NOLOCK) r INNER JOIN RoleModuleMap(NOLOCK) rm ON rm.RoleID = r.RoleID
									INNER JOIN Modules(NOLOCK) m ON m.ModuleID = rm.ModuleID
									INNER JOIN Roles(NOLOCK) r1 ON r.RoleName = r1.RoleName and r1.AccountID = @AccountID
							WHERE	ISNULL(r.AccountID, 0) = 0
						) tmp LEFT JOIN (
							SELECT	r.RoleID, rm.ModuleID
							FROM	dbo.Roles(NOLOCK) r INNER JOIN dbo.RoleModuleMap(NOLOCK) rm ON rm.RoleID = r.RoleID
							WHERE	r.AccountID = @AccountID
						) tmp1 ON tmp.RoleID = tmp1.RoleID AND tmp.ModuleID = tmp1.ModuleID
				WHERE	ISNULL(tmp1.RoleID, 0) = 0

				SELECT	1 Status, 'Successfully assigned new / missing role and module assignments' StatusMessage
			END
			ELSE
			BEGIN
				SELECT	0 Status, 'Found roles for this accout. Role creation failed.'
			END
		END
	END
	ELSE
	BEGIN
		SELECT	0 Status, 'Account does not exists. Role creation failed.'
	END

	SET NOCOUNT OFF
END


/*
	EXEC Account_Default_Roles @AccountID = 19
*/