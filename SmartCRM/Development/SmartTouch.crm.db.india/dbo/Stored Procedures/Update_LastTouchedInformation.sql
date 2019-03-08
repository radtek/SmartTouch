

/*
		Purpose		: Update LastTouched Information
		Input		: ModuleID, ContactID , LastTouchedDate
		Output		: Return Last Touhced Details
		Created By	: Krishna
		Created On	: Dec 17, 2014
		Modified On	: 
*/
CREATE PROCEDURE [dbo].[Update_LastTouchedInformation]
	(
		@Moduleid    smallint,
		@LastTouchedInformation  AS [dbo].[LastTouchedDetails] readonly         
	)
AS
BEGIN
	SET NOCOUNT ON
	 
	 DECLARE	@Contacts		TABLE (RowNum INT primary key, ContactID int, LastTouchedDate datetime,ActionID int)
	 DECLARE	@StartLoopID	int = 0,
				@MaxLoopID		int = 0
	
	INSERT INTO @Contacts (RowNum, ContactID, LastTouchedDate,ActionID)
	SELECT ROW_NUMBER() OVER ( ORDER BY ContactId), ContactID, LastTouchedDate,ActionID
		FROM @LastTouchedInformation

		DECLARE @batchSize INT = 1000
		DECLARE @iternationNumber INT = 0

		WHILE 1 = 1
		BEGIN
			UPDATE dbo.Contacts
				SET LastContactedThrough = @Moduleid,
					ContactSource = NULL,
					SourceType = NULL,
					LastContacted = LI.LastTouchedDate
			  FROM @Contacts LI
				  INNER JOIN dbo.Contacts C ON C.ContactID = LI.ContactID
				WHERE RowNum > (@iternationNumber * @BatchSize)

			IF @@ROWCOUNT < @batchSize BREAK
			set @iternationNumber = @iternationNumber + 1;    
		END
	SET NOCOUNT OFF
END




