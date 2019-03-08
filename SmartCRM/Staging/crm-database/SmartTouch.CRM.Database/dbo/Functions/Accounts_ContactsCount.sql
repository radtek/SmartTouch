
CREATE FUNCTION [dbo].[Accounts_ContactsCount] 
	(
		@AccountID int
	)
	RETURNS		int 
AS
BEGIN
	DECLARE @ContactCount int

	SELECT @ContactCount = SUM(Counts) 
	FROM
		(	
			SELECT COUNT(AccountID) Counts FROM [dbo].[Contacts] WITH (NOLOCK) WHERE AccountID = @AccountID AND IsDeleted = 0 
				
		) TempTags
	 
	RETURN ISNULL(@ContactCount, 0)
END

