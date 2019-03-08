CREATE PROC [dbo].[OpportunityBuyersSave]
@opportunitycontactmap dbo.OpportunityContactMapTableType READONLY
AS
BEGIN
	BEGIN TRY
		DECLARE @oppContactMapIds TABLE (Id INT IDENTITY(1,1), OpportunityContactMapID INT)
		DECLARE @oppcontmapId INT
		DECLARE @opportunityId INT

		SET @oppcontmapId = (SELECT TOP 1 OpportunityContactMapID FROM @opportunitycontactmap)
		SET @opportunityId = (SELECT TOP 1 OpportunityID FROM @opportunitycontactmap)

		INSERT INTO @oppContactMapIds
		SELECT OCM.OpportunityContactMapID FROM OpportunityContactMap OCM JOIN @opportunitycontactmap OCTM ON OCM.OpportunityID = OCTM.OpportunityID AND OCM.ContactID = OCTM.ContactID

		IF(@oppcontmapId IS NULL OR @oppcontmapId = 0)
			BEGIN
				UPDATE OCM
				SET
				OCM.IsDeleted = 1
				FROM OpportunityContactMap OCM JOIN @oppContactMapIds OCMI ON OCM.OpportunityContactMapID = OCMI.OpportunityContactMapID 

				INSERT INTO OpportunityContactMap(OpportunityID,ContactID,Potential,ExpectedToClose,Comments,[Owner],StageID,IsDeleted,CreatedOn,CreatedBy)
				SELECT OpportunityID,ContactID,Potential,ExpectedToClose,Comments,[Owner],StageID,IsDeleted,CreatedOn,CreatedBy FROM @opportunitycontactmap
			END

	END TRY
	BEGIN CATCH
			INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
				VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), NULL, @opportunityId)
	END CATCH
END

GO