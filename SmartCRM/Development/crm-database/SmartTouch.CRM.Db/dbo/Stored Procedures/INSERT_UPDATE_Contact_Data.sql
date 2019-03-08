CREATE  PROCEDURE [dbo].[INSERT_UPDATE_Contact_Data]
	(
		@ContactID		int,
		--@ContactsData	AS dbo.Contacts_Data readonly, 
		@ActionFlag		tinyint,
		@ReturnFlag		tinyint = 0,
		@ContactCreatedOn datetime = null
	)
AS
BEGIN	
	SET NOCOUNT ON
	BEGIN TRY	
		/* New Contact - Insert Contact */
		DECLARE @CreatedOn datetime, @AuditDate datetime, @LastUpdatedBy INT


		SET @CreatedOn = ISNULL(@ContactCreatedOn, GETUTCDATE())
		SET @AuditDate = ISNULL(@ContactCreatedOn, GETUTCDATE())

		SELECT @LastUpdatedBy = LastUpdatedBy FROM Contacts (NOLOCK) WHERE ContactID = @ContactID
				IF (EXISTS(SELECT * FROM Users WHERE UserID = @LastUpdatedBy AND AccountID = 1))
				BEGIN
					SELECT @LastUpdatedBy = value FROM EnvironmentSettings (NOLOCK) WHERE Name = 'CRM User'
				END

		IF (@ActionFlag = 1)	
			BEGIN
				INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus,AuditDate) 
				SELECT ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, @LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, 'I', 1,@AuditDate
					FROM dbo.Contacts(NOLOCK)
					WHERE ContactID = @ContactID

				INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName)
				SELECT ContactID, LastUpdatedBy, 3, 1, @CreatedOn, AccountID, CASE WHEN ContactType = 1 THEN FirstName+' '+LastName
																					 WHEN ContactType = 2 THEN Company END
				FROM dbo.Contacts(NOLOCK)
				WHERE ContactID = @ContactID
			END
		/* Update Contact - Update Contact */
		ELSE IF (@ActionFlag = 2)
			BEGIN
				INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus) 
				SELECT ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, @LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, 'U', 1
					FROM dbo.Contacts(NOLOCK)
					WHERE ContactID = @ContactID

				INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName)
				SELECT ContactID, LastUpdatedBy, 3, 3, @CreatedOn, AccountID, CASE WHEN ContactType = 1 THEN FirstName+' '+LastName
																					 WHEN ContactType = 2 THEN Company END 
					FROM dbo.Contacts(NOLOCK)
					WHERE ContactID = @ContactID
			END
		/* Delete Contact - Delete Contact */
		ELSE IF (@ActionFlag = 3)
			BEGIN
				INSERT INTO dbo.Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, AuditAction, AuditStatus) 
				SELECT ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID, ReferenceID, @LastUpdatedBy, LastUpdatedOn, OwnerID, ContactSource, SourceType, CompanyID, LastContactedThrough, FirstContactSource, FirstSourceType, 'U', 1
					FROM dbo.Contacts(NOLOCK)
					WHERE ContactID = @ContactID

				UPDATE dbo.Contacts_Audit
					SET [AuditStatus] = 0
					FROM dbo.Contacts_Audit CA
						INNER JOIN dbo.Contacts CD ON CA.ContactID = CD.ContactID AND CA.AccountID = CD.AccountID
					WHERE CA.ContactID = @ContactID AND CD.ContactID = @ContactID

				INSERT INTO dbo.UserActivityLogs (EntityID, UserID, ModuleID, UserActivityID, LogDate, AccountID, EntityName)
				SELECT ContactID, LastUpdatedBy, 3, 4, @CreatedOn, AccountID, ISNULL(FirstName, '')+' '+ISNULL(LastName, '')
					FROM dbo.Contacts(NOLOCK)
					WHERE ContactID = @ContactID
			END
			
			IF (@ReturnFlag = 0)
				BEGIN
					SELECT 'SEL-001' ResultCode 
				END
	END TRY
	BEGIN CATCH
		IF (@ReturnFlag = 0)
			BEGIN
				SELECT 'SEL-002' ResultCode 
			END

		SELECT CONVERT(sysname,  CURRENT_USER),  ERROR_NUMBER(),  ERROR_SEVERITY(),  ERROR_STATE(),  ERROR_PROCEDURE(),  ERROR_LINE(),  ERROR_MESSAGE(), @CreatedOn
	END CATCH
	SET NOCOUNT OFF
END

/*
	EXEC dbo.INSERT_UPDATE_Contact_Data
		@ContactID		= 1055982,
		-- @ContactsData	AS dbo.Contacts_Data readonly, 
		@ActionFlag		= 2
*/