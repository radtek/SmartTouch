

CREATE FUNCTION [dbo].[IsNullOrEmpty] 
(
	@x varchar(max)
)
RETURNS BIT
AS
BEGIN
	DECLARE @isNullOrEmpty BIT
	IF @x IS NOT NULL AND LEN(@x) > 0
		SET @isNullOrEmpty = 0
	ELSE
		SET @isNullOrEmpty = 1
	RETURN @isNullOrEmpty		
END


