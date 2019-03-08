


CREATE PROCEDURE [dbo].[SaveOpportunity](
	 @opportunity dbo.OpportunityTableType READONLY,
	 @image dbo.ImageType READONLY
)
AS
BEGIN 
	BEGIN TRY
		DECLARE @opportunityId INT
		DECLARE @imageId INT
		DECLARE @accountId INT
		DECLARE @IsOpportunityExits bit = 0

		SET @accountId = (SELECT AccountID FROM @opportunity)
		SET @opportunityId = (SELECT OpportunityID FROM @opportunity)

		--insertion of opportunity

		IF(@opportunityId IS NULL OR @opportunityId = 0) -- checking if opportunity is already exists or not
		BEGIN
			INSERT INTO [dbo].[Opportunities]([OpportunityName],[Potential],[StageID],[ExpectedClosingDate],[Description],[Owner],[AccountID],[CreatedBy],[CreatedOn],[LastModifiedBy],[LastModifiedOn],[IsDeleted],[OpportunityType],[ProductType],[Address],[ImageID])
			SELECT [OpportunityName],[Potential],[StageID],[ExpectedClosingDate],[Description],[Owner],[AccountID],[CreatedBy],[CreatedOn],[LastModifiedBy],[LastModifiedOn],[IsDeleted],[OpportunityType],[ProductType],[Address],[ImageID] FROM @opportunity

			SET @opportunityId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			UPDATE O
			SET 
			[OpportunityName] = OT.OpportunityName
			,[Potential] = OT.Potential 
			,[StageID] = OT.StageID
			,[ExpectedClosingDate] = OT.ExpectedClosingDate
			,[Description] = OT.Description
			,[Owner] = OT.Owner
			,[AccountID] = OT.AccountID
			,[CreatedBy] = OT.CreatedBy
			,[CreatedOn] = OT.CreatedOn
			,[LastModifiedBy] = OT.LastModifiedBy
			,[LastModifiedOn] = OT.LastModifiedOn
			,[IsDeleted] = OT.IsDeleted
			,[OpportunityType] = OT.OpportunityType
			,[ProductType] = OT.ProductType
			,[Address] = OT.Address
			,[ImageID] = OT.ImageID
			FROM Opportunities O JOIN @opportunity OT ON O.OpportunityID = OT.OpportunityID AND O.AccountID=OT.AccountID 

			SELECT @opportunityId= O.OpportunityID FROM Opportunities O JOIN @opportunity OT ON O.OpportunityID = OT.OpportunityID AND O.AccountID=OT.AccountID 

			SET @IsOpportunityExits = 1
		END

		-- insertion of image
		IF (@imageId IS NULL OR @imageId = 0 )
					BEGIN
						   IF EXISTS (SELECT 1 FROM @image)
						   BEGIN
							   INSERT INTO Images(FriendlyName,StorageName,OriginalName,CreatedBy,CreatedDate,ImageCategoryID,AccountID)
							   SELECT FriendlyName,StorageName,OriginalName,1,GETUTCDATE(),CategoryId,@accountId from @image
							   SET @imageId = SCOPE_IDENTITY();
							   UPDATE Opportunities SET ImageID = @imageId WHERE  OpportunityID=@opportunityId
						   END
					END	

		SELECT isnull(@opportunityId,0) as OpportunityID

	END TRY
	BEGIN CATCH
		
		IF(@opportunityId > 0 AND @IsOpportunityExits = 0)
		BEGIN
			UPDATE Opportunities SET IsDeleted = 1 WHERE  OpportunityID=@opportunityId
		END


		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
				VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), NULL, @accountId)
	END CATCH
END

/*

DECLARE  @opportunity dbo.OpportunityTableType
DECLARE  @image dbo.ImageType

INSERT INTO @opportunity VALUES(0,'NEW-OPPORTUNITY 2.0.1.1',10000,4669,GETUTCDATE(),'New Opportunities 2.0.1 Enhancement',6889,4218,6889,GETUTCDATE(),6889,GETUTCDATE(),0,'Physical Home Address','Building 2','Madhpur near image hospatel',null)
INSERT INTO @image VALUES(0,'AddOpportunityScreen.png','8F9345E0-5A02-4BAE-8F9B-92695AD72681.png','AddOpportunityScreen.png',6889,GETUTCDATE(),1,4218)

EXECUTE [dbo].[SaveOpportunity] @opportunity,@image

*/