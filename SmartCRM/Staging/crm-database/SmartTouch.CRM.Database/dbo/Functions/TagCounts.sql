


CREATE FUNCTION [dbo].[TagCounts] 
	(
		@TagID int
	)
	RETURNS		int 
AS
BEGIN
	DECLARE @TagCount int
	

	SELECT @TagCount = SUM(Count)
	FROM
		Tag_Counts WITH (NOLOCK) WHERE TagID = @TagID
	 

	RETURN @TagCount
END
