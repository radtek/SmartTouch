CREATE  PROCEDURE [dbo].[GetUserActivities]
	@UserID INT,
	@AccountID INT,
	@ModuleIds dbo.Contact_List READONLY,
	@Skip INT,
	@Take INT
	
AS
BEGIN
	DECLARE @LocUSerID INT = @UserID
	DECLARE @LocAccountID INT = @AccountID

	if(@LocAccountID = 1)
	BEGIN
		SELECT ACL.* FROM UserActivityLogs (NOLOCK) ACL 
		JOIN @ModuleIds M ON M.ContactId = ACL.ModuleID 
		WHERE ACL.UserID = @LocUSerID
		ORDER BY ACL.LogDate DESC OFFSET @skip ROWS
		FETCH NEXT @take ROWS ONLY
	END
	ELSE
	BEGIN
		SELECT ACL.* FROM UserActivityLogs (NOLOCK) ACL 
		JOIN @ModuleIds M ON M.ContactId = ACL.ModuleID 
		WHERE ACL.UserID = @LocUSerID AND ACL.AccountID = @LocAccountID 
		ORDER BY ACL.LogDate DESC OFFSET @skip ROWS
		FETCH NEXT @take ROWS ONLY	
	END

	
END