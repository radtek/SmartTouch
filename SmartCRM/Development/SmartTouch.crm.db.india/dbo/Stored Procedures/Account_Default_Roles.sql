
CREATE PROC [dbo].[Account_Default_Roles](

  @AccountID int

)
AS
BEGIN

DECLARE  @AccountAdmin       int,
         @Sales              int,
		 @Marketing          int,
		 @MAdmin             int,
		 @SAdmin             int


     /* Default Sales */
    INSERT INTO dbo.Roles (RoleName,AccountID)
    SELECT RoleName, @AccountID FROM dbo.Roles(NOLOCK) WHERE AccountID IS NULL

        SET @AccountAdmin = (SELECT TOP 1 RoleID FROM Roles WHERE AccountID = @AccountID AND RoleName = 'Account Administrator')

     /* Default Account Administrator */
    INSERT INTO dbo.RoleModuleMap (RoleID,ModuleID)
    SELECT @AccountAdmin, ModuleID FROM DBO.RoleModuleMap(NOLOCK) WHERE RoleID IN (SELECT RoleID FROM dbo.Roles WHERE RoleID = 6)

        SET @Sales = (SELECT TOP 1 RoleID FROM Roles WHERE AccountID = @AccountID AND RoleName = 'Sales')

     /* Default Marketing */
    INSERT INTO dbo.RoleModuleMap (RoleID,ModuleID)
    SELECT @Sales, ModuleID FROM DBO.RoleModuleMap(NOLOCK) WHERE RoleID IN (SELECT RoleID FROM dbo.Roles WHERE RoleID = 2)

        SET @Marketing = (SELECT TOP 1 RoleID FROM Roles WHERE AccountID = @AccountID AND RoleName = 'Marketing')

     /* Default Marketing Administrator */
    INSERT INTO dbo.RoleModuleMap (RoleID,ModuleID)
    SELECT @Marketing, ModuleID FROM DBO.RoleModuleMap(NOLOCK) WHERE RoleID IN (SELECT RoleID FROM dbo.Roles WHERE RoleID = 357) 

        SET @MAdmin = (SELECT TOP 1 RoleID FROM Roles WHERE AccountID = @AccountID AND RoleName = 'Marketing Administrator')

     /* Default Sales Administrator */
    INSERT INTO dbo.RoleModuleMap (RoleID,ModuleID)
    SELECT @MAdmin, ModuleID FROM DBO.RoleModuleMap(NOLOCK) WHERE RoleID IN (SELECT RoleID FROM dbo.Roles WHERE RoleID = 4) 

        SET @SAdmin = (SELECT TOP 1 RoleID FROM Roles WHERE AccountID = @AccountID AND RoleName = 'Sales Administrator')

    INSERT INTO dbo.RoleModuleMap (RoleID,ModuleID)
    SELECT @SAdmin, ModuleID FROM DBO.RoleModuleMap(NOLOCK) WHERE RoleID IN (SELECT RoleID FROM dbo.Roles WHERE RoleID = 5)

END


/*

   EXEC Account_Default_Roles
     @AccountID = 2

*/

    

GO


