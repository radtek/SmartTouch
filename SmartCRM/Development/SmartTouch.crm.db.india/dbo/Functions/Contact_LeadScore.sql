CREATE FUNCTION [dbo].[Contact_LeadScore] 
	(
		@ContactID int
	)
	RETURNS		int 
AS
BEGIN
	DECLARE @LeadScore int

	SELECT @LeadScore = SUM(Score) 
		FROM [dbo].[LeadScores]
		WHERE  ContactID = @ContactID
	
	RETURN ISNULL(@LeadScore, 0)
END




