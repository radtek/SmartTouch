
CREATE PROCEDURE [dbo].[GetContactsForSocialSync] 
	@AccountId int,
	@pageNumber int,
	@Limit int
AS
BEGIN
	SELECT C.ContactID,C.AccountID,C.FirstName,C.LastName, C.Company, C.ContactImageUrl, C.CompanyID, C.ContactSource, C.ContactType, C.DoNotEmail, C.FirstContactSource, C.FirstSourceType,
	C.HomePhone, C.IsDeleted, C.LastContacted, C.LastContactedThrough, C.LastUpdatedBy, C.LastUpdatedOn,C.LeadScore,C.LifecycleStage,C.MobilePhone,C.OwnerID,C.PartnerType,C.ProfileImageKey,C.ReferenceId,
	C.SourceType,C.SSN,C.Title,C.WorkPhone,C.CommunicationID,CE.Email AS PrimaryEmail,C.ImageID  
	INTO #TempContacts FROM Contacts (NOLOCK) C
	INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.IsPrimary = 1 AND CE.IsDeleted=0
	LEFT JOIN Images (NOLOCK) IMG ON IMG.ImageID = C.ImageID
	--INNER JOIN Communications (NOLOCK) COM ON COM.CommunicationID = C.CommunicationID
	WHERE C.AccountID=@AccountId AND C.IsDeleted=0
	ORDER BY c.ContactID ASC
	OFFSET (@pageNumber-1)*@Limit ROWS
	FETCH NEXT @Limit ROWS ONLY

	SELECT * FROM #TempContacts

	SELECT COM.* FROM Communications (NOLOCK) COM
	JOIN #TempContacts TM ON TM.CommunicationID= COM.CommunicationID
END




