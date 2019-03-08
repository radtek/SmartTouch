
CREATE PROCEDURE [dbo].[DoBulkContactOperations] 
(
	@Contacts dbo.Contact_List READONLY,
	@BulkOperationId INT
)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @OperationType SMALLINT,
			@OperationID INT,
			@CreatedBy	INT ,
			@RelationType SMALLINT,
			@AccountID INT,
			@TourLifeCycleStage SMALLINT

	SELECT @OperationType = OperationType, @OperationID = OperationID, @CreatedBy = UserID, @RelationType = RelationType, @AccountID = AccountID FROM dbo.BulkOperations (NOLOCK) 
	where  BulkOperationId = @BulkOperationId
	
	print @operationtype
	IF @OperationType = 1 -- Action
		BEGIN
			declare @guid uniqueidentifier
			set @guid =  newid()
			
			INSERT INTO dbo.ContactActionMap 
			SELECT @OperationID, ContactID, 0,@CreatedBy, GETUTCDATE(),@guid  FROM @Contacts

			INSERT INTO IndexData
			SELECT newid(),ContactID,1,GETUTCDATE(),NULL,1,1 FROM @Contacts
		END
	ELSE IF @OperationType = 2 -- Note
		BEGIN
			INSERT INTO dbo.ContactNoteMap 
			SELECT @OperationID, ContactID  FROM @Contacts
		END
	ELSE IF @OperationType = 3 -- Tag
		BEGIN
		INSERT INTO dbo.ContactTagMap (ContactID, TagID, TaggedBy, TaggedOn, AccountID)
          SELECT Exc.ContactID, @OperationID,  @CreatedBy, GETUTCDATE(), @AccountID FROM
         (SELECT C.ContactID   FROM @Contacts C   EXCEPT
           SELECT CTM.ContactID FROM dbo.ContactTagMap (NOLOCK) CTM WHERE CTM.TagID = @OperationID AND CTM.AccountID = @AccountID) Exc
		  END
	ELSE IF @OperationType = 8 -- Change Owner
		BEGIN
		   
		  
			UPDATE dbo.Contacts 
			SET OwnerID = @OperationID,
			LastUpdatedBy = @CreatedBy,
			LastUpdatedOn = GETUTCDATE()
			FROM dbo.Contacts C (NOLOCK)
			INNER JOIN @Contacts TC ON TC.ContactID = C.ContactID

			--UPDATE dbo.Contacts 
			--SET OwnerID = @OperationID
			--FROM dbo.Contacts C (NOLOCK)
			--INNER JOIN @Contacts TC ON TC.ContactID = C.ContactID
			--INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus) 
			--	SELECT C.ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, 'U', 1
			--		FROM dbo.Contacts C INNER JOIN @Contacts CT ON CT.ContactID = C.ContactID

			--	INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName)
			--	SELECT C.ContactID, LastUpdatedBy, 3, 3, GETUTCDATE(), AccountID, CASE WHEN ContactType = 1 THEN FirstName+' '+LastName
			--																		 WHEN ContactType = 2 THEN Company END 
			--							FROM dbo.Contacts C INNER JOIN @Contacts CT ON CT.ContactID = C.ContactID
		END
	ELSE IF @OperationType = 9 -- Delete
		BEGIN
			UPDATE dbo.Contacts 
			SET IsDeleted = 1,
			LastUpdatedBy = @CreatedBy,
			LastUpdatedOn = GETUTCDATE()
			FROM dbo.Contacts C (NOLOCK)
			INNER JOIN @Contacts TC ON TC.ContactID = C.ContactID

			INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus) 
				SELECT C.ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, 'U', 1
					FROM dbo.Contacts(NOLOCK) C INNER JOIN @Contacts CT ON CT.ContactID = C.ContactID

				UPDATE dbo.Contacts_Audit
					SET [AuditStatus] = 0
					FROM dbo.Contacts_Audit CA
						INNER JOIN dbo.Contacts(NOLOCK) CD ON CA.ContactID = CD.ContactID AND CA.AccountID = CD.AccountID
						INNER JOIN @Contacts CT ON CT.ContactID = CD.ContactID
					

				INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName)
				SELECT C.ContactID, LastUpdatedBy, 3, 4, GETUTCDATE(), AccountID, ISNULL(FirstName, '')+' '+ISNULL(LastName, '')
					FROM dbo.Contacts(NOLOCK) C INNER JOIN @Contacts CT ON CT.ContactID = C.ContactID

		END
	ELSE IF @OperationType = 4 -- Tour
		BEGIN
						SELECT @CreatedBy = CreatedBy From Actions (NOLOCK) WHERE ActionID = @OperationID
						INSERT INTO dbo.ContactTourMap 
						SELECT @OperationID, ContactID, 0,@CreatedBy, GETUTCDATE()  FROM @Contacts
						SELECT @TourLifeCycleStage = LifeCycleStage FROM Tours (NOLOCK) WHERE TourID = @OperationID
						IF (@TourLifeCycleStage != 0 and @TourLifeCycleStage IS NOT NULL)
						BEGIN
							UPDATE C
							SET LifecycleStage = @TourLifeCycleStage FROM Contacts C
							JOIN @Contacts CO ON Co.ContactID = C.ContactID

							INSERT INTO IndexData
							SELECT NEWID(),ContactID,1,GETUTCDATE(),NULL,1,1 FROM @Contacts
						END
		END
	ELSE IF @OperationType = 6 -- Send Text
		BEGIN
			SELECT * FROM dbo.Contacts (NOLOCK)
		END
	ELSE IF @OperationType = 5 -- Send Email
		BEGIN
			SELECT * FROM dbo.Contacts (NOLOCK)
		END
	--ELSE IF @OperationType = 9 --Opportunities
	--	BEGIN
	--		INSERT INTO dbo.OpportunityContactMap 
	--		SELECT @OperationID, ContactID  FROM @Contacts
	--	END
	ELSE IF @OperationType = 7 --Relationship
		BEGIN
			INSERT INTO ContactRelationshipMap
			SELECT ContactID,@RelationType,NULL, @OperationID,@CreatedBy, GETUTCDATE() FROM @Contacts
		END
	ELSE IF @OperationType = 11 --Saved Search Workflow
		BEGIN
			INSERT INTO Workflow.TrackMessages
			SELECT NEWID(), 14, @OperationID, 0, ContactID, @CreatedBy,@AccountID,NULL, GETUTCDATE(), 701  FROM @Contacts
		END
	
END