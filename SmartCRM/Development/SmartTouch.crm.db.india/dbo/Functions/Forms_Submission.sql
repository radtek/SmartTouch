

CREATE FUNCTION [dbo].[Forms_Submission] 
	(
		@FormID int
	)
	RETURNS		int 
AS
BEGIN
	DECLARE @FormSubmission int

 SELECT @FormSubmission = COUNT(FormSubmissionID)
		FROM [dbo].[FormSubmissions]
		WHERE  FormID = @FormID
	
	RETURN ISNULL(@FormSubmission, 0)
END






