
CREATE FUNCTION [dbo].[isValidEmailFormat_New]
(
    @Email varchar(500)
)
RETURNS bit
AS
BEGIN
    DECLARE @Result bit

    SET @Email = LTRIM(RTRIM(@Email));
    SELECT @Result =
    CASE WHEN
    CHARINDEX(' ',LTRIM(RTRIM(@Email))) = 0
    AND LEFT(LTRIM(@Email),1) <> '@'
    AND RIGHT(RTRIM(@Email),1) <> '.'
    AND LEFT(LTRIM(@Email),1) <> '-'
    AND CHARINDEX('.',@Email,CHARINDEX('@',@Email)) - CHARINDEX('@',@Email) > 2    
    AND LEN(LTRIM(RTRIM(@Email))) - LEN(REPLACE(LTRIM(RTRIM(@Email)),'@','')) = 1
    AND CHARINDEX('.',REVERSE(LTRIM(RTRIM(@Email)))) >= 3
    AND (CHARINDEX('.@',@Email) = 0 AND CHARINDEX('..',@Email) = 0)
    AND (CHARINDEX('-@',@Email) = 0 AND CHARINDEX('..',@Email) = 0)
    AND (CHARINDEX('_@',@Email) = 0 AND CHARINDEX('..',@Email) = 0)
    AND ISNUMERIC(SUBSTRING(@Email, 1, 1)) = 0
    AND CHARINDEX(',', @Email) = 0
    AND CHARINDEX('!', @Email) = 0
    AND CHARINDEX('-.', @Email)=0
    AND CHARINDEX('%', @Email)=0
    AND CHARINDEX('#', @Email)=0
    AND CHARINDEX('$', @Email)=0
    AND CHARINDEX('&', @Email)=0
    AND CHARINDEX('^', @Email)=0
    AND CHARINDEX('''', @Email)=0
    AND CHARINDEX('\', @Email)=0
    AND CHARINDEX('/', @Email)=0
    AND CHARINDEX('*', @Email)=0
    AND CHARINDEX('+', @Email)=0
    AND CHARINDEX('(', @Email)=0
    AND CHARINDEX(')', @Email)=0
    AND CHARINDEX('[', @Email)=0
    AND CHARINDEX(']', @Email)=0
    AND CHARINDEX('{', @Email)=0
    AND CHARINDEX('}', @Email)=0
    AND CHARINDEX('?', @Email)=0
    AND CHARINDEX('<', @Email)=0
    AND CHARINDEX('>', @Email)=0
    AND CHARINDEX('=', @Email)=0
    AND CHARINDEX('~', @Email)=0
    AND CHARINDEX('`', @Email)=0 
    AND CHARINDEX('.', SUBSTRING(@Email, CHARINDEX('@', @Email)+1, 2))=0
    AND CHARINDEX('.', SUBSTRING(@Email, CHARINDEX('@', @Email)-1, 2))=0
    AND LEN(SUBSTRING(@Email, 0, CHARINDEX('@', @Email)))>1
    AND CHARINDEX('.', REVERSE(@Email)) > 2
    AND CHARINDEX('.', REVERSE(@Email)) < 5  
    THEN 1 ELSE  0 END


    RETURN @Result
END
