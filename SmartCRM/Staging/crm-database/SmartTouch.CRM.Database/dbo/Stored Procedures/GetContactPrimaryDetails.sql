
    CREATE PROCEDURE [dbo].[GetContactPrimaryDetails] 
    	@ContactID INT
    AS
BEGIN
	DECLARE @ContactInfo  TABLE (
		FirstName NVARCHAR(500),
		LastName NVARCHAR(500),
		EmailId NVARCHAR(500),
		PhoneNumber NVARCHAR(500),
		City NVARCHAR(500),
		StateName NVARCHAR(500),
		ZipCode NVARCHAR(500),
		CountryName NVARCHAR(500),
		Company NVARCHAR(500),
		OwnerName NVARCHAR(500),
		CreatedBy NVARCHAR(500)
	)
	DECLARE @ContactFields TABLE (
		FieldName NVARCHAR(500),
		FieldValue NVARCHAR(500)
	)

		INSERT INTO	@ContactInfo 
		SELECT TOP 1
		ISNULL(c.FirstName,'') FirstName, 
		ISNULL(c.LastName,'') LastName, 
		ISNULL(ce.Email,'') EmailId,
		ISNULL(cp.PhoneNumber,'') PhoneNumber,
		ISNULL(ca.City,'') City,
		ISNULL(ca.StateName,'') StateName, 
		ISNULL(ca.ZipCode,'') ZipCode, 
		ISNULL(ca.CountryName,'') CountryName,
		ISNULL(co.company,'') Company,
		ISNULL(u.FirstName,'') +' '+ ISNULL(u.LastName,'') OwnerName,
		ISNULL(U1.FirstName,'') +' '+ ISNULL(U1.LastName,'') CreatedBy

		FROM Contacts (NOLOCK) c
			LEFT JOIN 
				Contacts(NOLOCK) co ON co.ContactID = c.CompanyID
			LEFT JOIN (
				SELECT  contactid, email FROM contactemails (NOLOCK) WHERE IsPrimary = 1) ce ON c.contactid = ce.ContactID 
			LEFT JOIN (
				SELECT  contactid, PhoneNumber FROM ContactPhoneNumbers (NOLOCK) WHERE IsPrimary = 1) cp ON c.ContactID = cp.ContactID
			LEFT JOIN (
				SELECT   ZipCode,a.City, s.StateName,cou.CountryName,a.AddressID,cam.contactid FROM Addresses (NOLOCK) a 
					LEFT JOIN ContactAddressMap(NOLOCK) cam ON a.AddressID = cam.AddressID
					LEFT JOIN States(NOLOCK) S ON a.StateID = s.StateID
					LEFT JOIN Countries(NOLOCK) Cou ON Cou.CountryID = a.CountryID
					WHERE a.IsDefault = 1) ca ON c.ContactID = ca.ContactID
			LEFT JOIN Users (NOLOCK) u ON u.Userid = c.Ownerid
			LEFT JOIN (
				SELECT * FROM Contacts_Audit (NOLOCK) WHERE AuditAction = 'I') cau ON cau.ContactID = c.ContactID
			LEFT JOIN Users (NOLOCK) u1 ON u1.UserID = cau.LastUpdatedBy
		WHERE c.ContactID = @ContactID	
	
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'FIRSTNAME',FirstName FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'LASTNAME',LastName FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'EMAILID',EmailId FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'PHONE',PhoneNumber FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'CITY',City FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'STATE',StateName FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'ZIPCODE',ZipCode FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'COUNTRY',CountryName FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'COMPANY',Company FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'OWNERNAME',OwnerName FROM @ContactInfo
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT 'CREATEDBY',CreatedBy FROM @ContactInfo

		--Include the below query if Custom fields are also included in campaign url
		--INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT CONCAT(CustomFieldID,'CF') FieldName, FieldValue FROM GET_ContactCustomField_Values WHERE contactid = @ContactID
		
		--Include the below query if Dropdown fields are also included in campaign url
		INSERT INTO @ContactFields  (FieldName, FieldValue) SELECT CONCAT(CustomFieldID,'DF') FieldName, FieldValue FROM GET_DropdownFieldValues WHERE contactid = @ContactID
		
		
		SELECT * FROM @ContactFields
		
		END
		
		--EXEC GetContactPrimaryDetails 1739592