
CREATE PROCEDURE [dbo].[Contact_Duplicate_Check] 
	@firstName varchar(200),
	@lastName varchar(200),
	@email varchar(200),
	@company varchar(200),
	@contactID INT,
	@accountID INT,
	@contactType smallint
AS
BEGIN
	DECLARE @Contacts TABLE (ID INT IDENTITY(1,1), ContactID INT)
	DECLARE @DuplicateContactID INT

	DECLARE @duplicate bit
	IF (LEN(@email) > 0)
	BEGIN
		INSERT INTO @Contacts
		SELECT CE.ContactID 
		FROM ContactEmails(NOLOCK) CE
		INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CE.ContactID
		WHERE Email = @email AND IsPrimary = 1 AND CE.AccountID = @accountID AND CE.IsDeleted = 0 AND C.IsDeleted = 0 AND C.ContactType = @contactType
		 
		IF (@contactID > 0)
			IF(EXISTS (SELECT 1 FROM @Contacts EXCEPT SELECT @contactID))
				SELECT TOP 1 @DuplicateContactID =  ContactID FROM (SELECT ContactID FROM @Contacts EXCEPT SELECT @contactID) x
			ELSE
				SET @DuplicateContactID = 0				
		ELSE
			IF(EXISTS (SELECT 1 FROM @Contacts))
				SELECT TOP 1 @DuplicateContactID =  ContactID FROM @Contacts
			ELSE
				SET @DuplicateContactID = 0				
	END
	ELSE IF(LEN(@firstName) > 0 AND LEN(@lastName) > 0 AND LEN(@company) = 0)
	BEGIN
		INSERT INTO @Contacts
		SELECT C.ContactID
		FROM Contacts(NOLOCK) C
		WHERE FirstName = @firstName AND LastName = @lastName AND (Company IS NULL OR Company = '') AND C.AccountID = @accountID
		AND C.IsDeleted = 0 AND C.ContactType = @contactType

		IF (@contactID > 0)
			IF (EXISTS  (SELECT 1 FROM @Contacts EXCEPT SELECT @contactID))
				SELECT TOP 1 @DuplicateContactID =  ContactID FROM (SELECT ContactID FROM @Contacts EXCEPT SELECT @contactID) x
			ELSE
				SET @DuplicateContactID = 0	
		ELSE
			IF (@contactID > 0 AND EXISTS (SELECT 1 FROM @Contacts))
				SELECT TOP 1 @DuplicateContactID =  ContactID FROM @Contacts
			ELSE
				SET @DuplicateContactID = 0	
	END
	ELSE IF (LEN(@firstName) > 0 AND LEN(@lastName) > 0 AND LEN(@company) > 0)
	BEGIN
		INSERT INTO @Contacts
		SELECT C.ContactID FROM Contacts(NOLOCK) C
		WHERE FirstName = @firstName AND LastName = @lastName AND Company = @company AND C.AccountID = @accountID
		AND C.IsDeleted = 0 AND C.ContactType = @contactType

		IF (@contactID > 0)
			IF (EXISTS (SELECT 1 FROM @Contacts EXCEPT SELECT @contactID))
				 SELECT TOP 1 @DuplicateContactID =  ContactID FROM (SELECT ContactID FROM @Contacts EXCEPT SELECT @contactID) x
			ELSE
				 SET @DuplicateContactID = 0	
		ELSE
			IF (EXISTS (SELECT 1 FROM @Contacts))
				 SELECT TOP 1 @DuplicateContactID =  ContactID FROM @Contacts
			ELSE
				 SET @DuplicateContactID = 0	
	END
	
	--DECLARE @duplicateContacts dbo.Contact_List 
	--INSERT INTO @duplicateContacts
	SELECT @DuplicateContactID

	--EXEC GET_ElasticSearch_ContactData  @duplicateContacts
END

--exec Contact_Duplicate_Check 'new','dd','','',0,4218,1