
CREATE PROCEDURE  [dbo].[Update_Contact_LifecycleStage]
	(
		@ContactID		int, 
		@LifeCycleStage	smallint
	)
AS
BEGIN
 
	SET NOCOUNT ON

	INSERT INTO Contacts_Audit (ContactID, FirstName, LastName, Company, CommunicationID, Title,
		ContactImageUrl, AccountID, LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, 
		PrimaryEmail, ContactType, SSN, LifecycleStage, PartnerType, DoNotEmail, LastContacted, 
		IsDeleted, ProfileImageKey, ImageID, ReferenceID, LastUpdatedBy, LastUpdatedOn, ContactSource,
		SourceType, CompanyID, OwnerID, LastContactedThrough,  AuditAction, AuditStatus, 
		IsLifecycleStageChanged, NewLifecycleStage) 
	SELECT ContactID, FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, 
		LeadScore, LeadSource, HomePhone, WorkPhone, MobilePhone, PrimaryEmail, ContactType, SSN, 
		LifecycleStage, PartnerType, DoNotEmail, LastContacted, IsDeleted,  ProfileImageKey, ImageID, 
		ReferenceID, LastUpdatedBy, LastUpdatedOn, ContactSource, SourceType, CompanyID, OwnerID, 
		LastContactedThrough, 'U', 1, 1, @LifeCycleStage
	FROM dbo.Contacts(NOLOCK)
	WHERE ContactID = @ContactID


	 SET NOCOUNT OFF

END

/*
	EXEC dbo.Update_Contact_LifeCycleStage
		@ContactID		= 1000, 
		@LifeCycleStage	= 250
*/