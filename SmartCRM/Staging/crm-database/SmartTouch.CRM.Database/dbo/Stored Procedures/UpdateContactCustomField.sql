CREATE PROC [dbo].[UpdateContactCustomField]
@ContactId INT,
@FieldId INT,
@Value NVARCHAR(MAX) = null
AS
BEGIN
	DECLARE @MappingId INT = 0
	SET @MappingId = (SELECT TOP 1 ContactCustomFieldMapID FROM ContactCustomFieldMap (NOLOCK) CCFM
		WHERE ContactID = @ContactId AND CustomFieldID = @FieldId)

	IF (@MappingId > 0)
	BEGIN
		UPDATE ContactCustomFieldMap SET Value = @Value 
			WHERE ContactID = @ContactId AND CustomFieldID = @FieldId
		UPDATE Contacts SET ContactSource=4 WHERE ContactID=@ContactId
		PRINT @MappingId
	END
	ELSE
	BEGIN
		INSERT INTO ContactCustomFieldMap VALUES(@ContactId,@FieldId,@Value)
		SET @MappingId = (SELECT SCOPE_IDENTITY())
		UPDATE Contacts SET ContactSource=4 WHERE ContactID=@ContactId
		PRINT @MappingId
	END
	SELECT @MappingId
END

--EXEC UpdateContactCustomField 1760140,19882,'AdBC' SELECT * FROM ContactCustomFieldMap (NOLOCK) CCFM where contactid = 1760140

--SP_HELP ContactCustomFieldMap