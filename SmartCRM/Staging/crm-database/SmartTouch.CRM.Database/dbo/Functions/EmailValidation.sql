
CREATE FUNCTION [dbo].[EmailValidation]
(
    @Email		NVARCHAR(MAX),
	@FirstName	NVARCHAR(MAX),
	@LastName	NVARCHAR(MAX)
)
RETURNS bit
AS
BEGIN 
    DECLARE @Result bit = 0

	IF (LEN(@Email) > 0 AND @Email LIKE '%[!#$%^&*()+ <>?;:"{}]%')
		BEGIN
			SET @Result = 0
		END
	ELSE IF (LEN(@Email) > 0)
		BEGIN
			DECLARE @pattern varchar(4000)
			SET @pattern = '[a-zA-Z0-9_\-]+@([a-zA-Z0-9_\-]+\.)+[a-zA-Z0-9_\-]'	
			
			--SELECT @Result = CASE WHEN @Email LIKE @pattern
			--	THEN 0 ELSE 1	END	

			DECLARE @objRegexExp INT
			EXEC sp_OACreate 'VBScript.RegExp', @objRegexExp OUT

			EXEC sp_OASetProperty @objRegexExp, 'Pattern', @pattern
			EXEC sp_OASetProperty @objRegexExp, 'IgnoreCase', 1
			EXEC sp_OASetProperty @objRegexExp, 'MultiLine', 0
			EXEC sp_OASetProperty @objRegexExp, 'Global', false
			EXEC sp_OASetProperty @objRegexExp, 'CultureInvariant', true

			EXEC sp_OAMethod @objRegexExp, 'Test', @Result OUT, @Email

			EXEC sp_OADestroy @objRegexExp
		END
	ELSE IF (LEN(@Email) = 0 AND ((LEN(@FirstName) = 0 OR @FirstName IS NULL) AND (LEN(@LastName) = 0 OR @LastName IS NULL)))
		BEGIN
			SET @Result = 0
		END
	ELSE IF (LEN(@Email) = 0 AND LEN(@FirstName) > 0 AND LEN(@LastName) > 0)
		BEGIN
			SET @Result = 1
		END
	ELSE  
		BEGIN
			SET @Result = 0
		END

    RETURN @Result
END


