



CREATE FUNCTION [dbo].[GetCustomFieldValue_test](
	@FieldTypeID int,
	@FieldValue nvarchar(4000)
) RETURNS nvarchar(4000)
AS
BEGIN
	DECLARE @CFList varchar(4000),
			@CFValue varchar(2000) = ''

	IF( @FieldTypeID = 6 OR @FieldTypeID = 11 )
	BEGIN
		IF( ISNUMERIC(@FieldValue) = 1 )
		BEGIN
			SELECT	@CFValue = Value
			FROM	CustomFieldValueOptions
			WHERE	CustomFieldValueOptionID = @FieldValue
		END

		SET @FieldValue = @CFValue
	END
	IF( @FieldTypeID = 1 OR @FieldTypeID = 12 )
	BEGIN
		SELECT	@CFList = ISNULL(@CFList, '') + isnull(Value, '') + ','
		FROM	CustomFieldValueOptions
			WHERE	CustomFieldValueOptionID IN (
					SELECT	DataValue
					FROM	Split_2( @FieldValue, '|' )
				)
		SET @FieldValue = LEFT(@CFList, LEN(@CFList) -1)
	END

	RETURN @FieldValue
END






