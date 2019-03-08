

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

		DECLARE @batchSize INT = 1000, @NextBatchSize int = 1000
		DECLARE @iternationNumber INT = 0

		WHILE 1 = 1
		BEGIN
			UPDATE dbo.Contacts
				SET LastContactedThrough = @Moduleid,
					ContactSource = NULL,
					SourceType = NULL,
					LastContacted = LI.LastTouchedDate
			  FROM @Contacts LI
				  INNER JOIN dbo.Contacts C WITH (NOLOCK) ON C.ContactID = LI.ContactID
				WHERE RowNum BETWEEN ((@iternationNumber * @BatchSize)+1) AND @NextBatchSize

			IF @@ROWCOUNT < @batchSize BREAK
			set @iternationNumber = @iternationNumber + 1;  
			SET @NextBatchSize = @NextBatchSize+1000  
		END
	SET NOCOUNT OFF
END