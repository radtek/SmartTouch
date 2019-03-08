
CREATE FUNCTION [dbo].[GET_Imports_CustomFieldValue](@FieldID int, @FieldTypeID int, @FieldValue VARCHAR(4000))
RETURNS VARCHAR(MAX)
BEGIN
	
	DECLARE @CFVID			VARCHAR(MAX) = '',
			@FieldValueIDs	VARCHAR(MAX) = ''

	IF (@FieldTypeID IN (6,11))
		BEGIN
			SET @FieldValueIDs = ISNULL(CONVERT(VARCHAR(MAX),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
							WHERE CustomFieldID = @FieldID AND Value = @FieldValue AND IsDeleted = 0)), '')								
		END
	ELSE IF (@FieldTypeID IN (1,12))
		BEGIN
			DECLARE @CFValueList1 TABLE (ID int IDENTITY(1,1), Value NVARCHAR(MAX))
			DECLARE @I1				int = 1,
					@CFNewValue1	NVARCHAR(MAX) = '',
					@CFVIDs1		NVARCHAR(MAX) = ''										

			INSERT INTO @CFValueList1 (Value)
			SELECT DISTINCT RTRIM(LTRIM(DataValue)) FROM dbo.Split(@FieldValue, ',') WHERE LEN(DataValue) > 0

			DECLARE @LoopValueCount1 int = (SELECT MAX(ID) FROM @CFValueList1)

			WHILE (@I1 <= @LoopValueCount1)
			BEGIN
				SET @CFNewValue1 = (SELECT Value FROM @CFValueList1 WHERE ID = @I1)

				SET @CFVID = ISNULL(CONVERT(VARCHAR(MAX),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
								WHERE CustomFieldID = @FieldID AND Value = @CFNewValue1 AND IsDeleted = 0)), '')
																									
				IF(@CFVID != '')
					BEGIN																					
						SET @FieldValueIDs = @FieldValueIDs + @CFVID +'|'
					END
				SET @I1 = @I1 + 1												
			END		
		END		
	RETURN @FieldValueIDs
END

