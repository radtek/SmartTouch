
CREATE FUNCTION [dbo].[fnReplaceCharAtPos](@input varchar(8000), @startPosition int, @numberOfChars INT, @newvalue varchar(8000))
RETURNS varchar(8000) AS  
BEGIN 
declare @Res varchar(8000)
	set @Res=left(@input, @startPosition-1) + @newvalue  + right(@input, len(@input)- @startPosition - @numberOfChars+1  )
return @Res
END
