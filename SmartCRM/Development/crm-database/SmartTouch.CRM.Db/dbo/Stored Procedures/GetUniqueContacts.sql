
CREATE  PROCEDURE [dbo].[GetUniqueContacts] 
	 @SDefinitions dbo.SavedSearchContacts READONLY,
	 @Tags dbo.Contact_List READONLY,
     @AccountId INT,
	 @EntityType TINYINT
AS
BEGIN
	 IF (@EntityType = 1)
		SELECT COUNT(DISTINCT CTM.ContactId) FROM @Tags T
		INNER JOIN ContactTagMap CTM (NOLOCK) ON T.ContactID = CTM.TagID
		INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CTM.ContactID AND C.AccountID = @AccountId
		INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = @AccountId
		WHERE C.IsDeleted = 0 AND CE.IsPrimary = 1 AND CE.IsDeleted=0 AND CTM.AccountID = @AccountId
	    
	 ELSE
	    SELECT COUNT(DISTINCT SS.ContactID)
		FROM @SDefinitions SS
		INNER JOIN Contacts (NOLOCK) C ON C.ContactID = SS.ContactID AND C.AccountID = @AccountId
		INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = @AccountId
		WHERE C.IsDeleted = 0 AND CE.IsPrimary = 1 AND CE.IsDeleted = 0
END
GO

