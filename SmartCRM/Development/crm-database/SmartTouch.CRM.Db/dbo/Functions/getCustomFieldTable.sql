




CREATE FUNCTION [dbo].[getCustomFieldTable](@contactId int, @CFInputData varchar(2000))
returns @customFields table (ContactId int, CustomFieldID int, Value varchar(200))
as
BEGIN
	DECLARE @DataCustomFields TABLE (LoopID int IDENTITY(1,1), InputData NVARCHAR(MAX))
	declare @FieldID int = 0;
	declare @FieldInputTypeID smallint = 0;
	declare @CFVID VARCHAR(2000) = '0'			
	declare @CFieldName				NVARCHAR(100) = '';
	declare @CFValue				NVARCHAR(MAX) = '';
	declare @Count					int = 0,
			@Index					int = 0,
			@Data					NVARCHAR(MAX) = '',
			@LoopID					int = 1,
			@InputData				NVARCHAR(MAX) = ''

	SELECT @Index = 1
		WHILE @Index != 0
	BEGIN
		SELECT @Index = CHARINDEX('~', @CFInputData)					
	IF @Index != 0
		BEGIN
			SELECT @Data = LEFT(@CFInputData, @Index-1)
		END
	ELSE
		BEGIN
			SELECT @Data = @CFInputData
		END
	
	INSERT INTO @DataCustomFields (InputData)
		VALUES( @Data )	
	
	SELECT @CFInputData = RIGHT(@CFInputData, LEN(@CFInputData) - @Index)
	
	IF LEN(@CFInputData) = 0
	BEGIN BREAK
		END
	END

	DECLARE @LoopValueCount2 int = (SELECT MAX(LoopID) FROM @DataCustomFields)

		WHILE (@LoopID <= @LoopValueCount2)
		BEGIN

			SET @InputData = (SELECT InputData FROM @DataCustomFields WHERE LoopID = @LoopID)

			SELECT @CFieldName	= REPLACE(SUBSTRING(@InputData, 1, CHARINDEX('|', @InputData)-1), ' ', '')
			SELECT @CFValue		= SUBSTRING(@InputData, CHARINDEX('|', @InputData)+1, LEN(@InputData))
							
			SET @FieldID = REPLACE(SUBSTRING(@CFieldName, 1, CHARINDEX('##$##', @CFieldName)-1), ' ', '')
			SET @Fieldinputtypeid = SUBSTRING(@CFieldName, CHARINDEX('##$##', @CFieldName)+5, LEN(@CFieldName))	

			IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID IN (1,6,11,12))
			BEGIN
					IF (@FieldInputTypeID IN (6,11))
					BEGIN
						SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
										WHERE CustomFieldID = @FieldID AND Value = @CFValue AND IsDeleted = 0)), '')
								
						INSERT INTO @customFields (ContactID, CustomFieldID, Value) SELECT @ContactID, @FieldID, @CFVID	
					END
					ELSE IF (@FieldInputTypeID IN (1,12))
						BEGIN
							DECLARE @CFValueList1 TABLE (ID int IDENTITY(1,1), Value NVARCHAR(MAX))
							DECLARE @I1				int = 1,
									@CFNewValue1	NVARCHAR(MAX) = '',
									@CFVIDs1		NVARCHAR(MAX) = ''										

							INSERT INTO @CFValueList1 (Value)
							SELECT DISTINCT RTRIM(LTRIM(DataValue)) FROM dbo.Split(@CFValue, ',') WHERE LEN(DataValue) > 0

							DECLARE @LoopValueCount1 int = (SELECT MAX(ID) FROM @CFValueList1)

							WHILE (@I1 <= @LoopValueCount1)
							BEGIN
								SET @CFNewValue1 = (SELECT Value FROM @CFValueList1 WHERE ID = @I1)

								SET @CFVID = ISNULL(CONVERT(VARCHAR(15),(SELECT CustomFieldValueOptionID FROM dbo.CustomFieldValueOptions
												WHERE CustomFieldID = @FieldID AND Value = @CFNewValue1 AND IsDeleted = 0)), '')
																									
								IF(@CFVID != '')
								BEGIN																					
									SET @CFVIDs1 = @CFVIDs1 + @CFVID +'|'
								end
								SET @I1 = @I1 + 1												
							END
		
							INSERT INTO @customFields (ContactID, CustomFieldID, Value)
							SELECT @ContactID, @FieldID, @CFVIDs1;
						END		
					END
					ELSE IF (ISNULL(@FieldID,0) != 0 AND @FieldInputTypeID NOT IN (1,6,11,12))
						BEGIN									
							INSERT INTO @customFields (ContactID, CustomFieldID, Value)
							SELECT @ContactID, @FieldID, @CFValue;
						END
		SET @LoopID = @LoopID + 1
			END
	return;
end






