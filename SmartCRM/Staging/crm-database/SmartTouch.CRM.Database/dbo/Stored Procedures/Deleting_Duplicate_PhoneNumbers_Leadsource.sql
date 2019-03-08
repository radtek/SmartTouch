

CREATE PROCEDURE [dbo].[Deleting_Duplicate_PhoneNumbers_Leadsource]
AS
BEGIN


		---------------------------*     DuplicatePhoneNumbers   *---------------------------------

		DROP TABLE IF EXISTS #DuplicatePhoneNumbers

		; WITH DupContactPhoneNumbers (Accountid,AccountName,ContactPhoneNumberID,ContactID,PhoneNumber,IsPrimary, DuplicateCount)
		AS 
		(
			SELECT
				CP.Accountid,
				AccountName,
				ContactPhoneNumberID,
				cp.ContactID,
				PhoneNumber,
				IsPrimary,
				ROW_NUMBER() OVER (PARTITION BY CP.Accountid,AccountName,cp.ContactID, PhoneNumber,IsPrimary ORDER BY ContactPhoneNumberID) AS DuplicateCount
			FROM SmartCRM.dbo.ContactPhoneNumbers cp WITH (NOLOCK) 
			INNER JOIN Accounts a ON A.Accountid = cp.Accountid
			INNER JOIN Contacts c on c.ContactID = Cp.ContactID
			WHERE CP.IsDeleted = 0  and CP.IsPrimary = 1 and c.IsDeleted = 0
		)


		SELECT * INTO #DuplicatePhoneNumbers
		FROM
		(
		SELECT * FROM DupContactPhoneNumbers  WHERE DuplicateCount > 1
		)t



		SELECT * from ContactPhoneNumbers WITH (Nolock) Where  ContactPhoneNumberID in (
		select ContactPhoneNumberID FROM #DuplicatePhoneNumbers)

		

		UPDATE C
		SET C.IsPrimary = 0
		FROM  ContactPhoneNumbers C Where  C.ContactPhoneNumberID IN (
		SELECT ContactPhoneNumberID FROM #DuplicatePhoneNumbers)


		
		INSERT INTO IndexData ([ReferenceID],[EntityID],[IndexType],[CreatedOn],[Status],[IsPercolationNeeded])
		select NEWID(),ContactID,1,GETUTCDATE(),1,0
		FROM #DuplicatePhoneNumbers
		

		---------------------------*     DuplicateContactLeadSource   *---------------------------------
		DROP TABLE IF EXISTS #DuplicateContactLeadSource

		;
		WITH DupLeadSource (Accountid,AccountName,ContactID,LeadSouceID,IsPrimaryLeadSource,ContactLeadSourceMapID, DuplicateCount)
		AS (
				SELECT 
					C.Accountid,
					AccountName,
					CL.ContactID,
					LeadSouceID,
					IsPrimaryLeadSource,
					ContactLeadSourceMapID,
					ROW_NUMBER() OVER (PARTITION BY C.Accountid,AccountName,CL.ContactID,LeadSouceID,IsPrimaryLeadSource ORDER BY ContactLeadSourceMapID) AS DuplicateCount
				FROM SmartCRM.dbo.ContactLeadSourceMap cl
				INNER JOIN Contacts c on c.ContactID = CL.ContactID
				INNER JOIN Accounts A ON A.AccountID = C.AccountID
				WHERE IsPrimaryLeadSource = 1 and c.IsDeleted = 0 
			)
	


		SELECT * INTO #DuplicateContactLeadSource
		FROM
		(
		SELECT  * FROM DupLeadSource  WHERE DuplicateCount >1
		)T



		SELECT * FROM ContactLeadSourceMap  WHERE ContactLeadSourceMapID IN (SELECT ContactLeadSourceMapID FROM  #DuplicateContactLeadSource)


		UPDATE C
		SET C.IsPrimaryLeadSource = 0
		FROM ContactLeadSourceMap  C
		WHERE C.ContactLeadSourceMapID IN (SELECT ContactLeadSourceMapID FROM  #DuplicateContactLeadSource)

		
		INSERT INTO IndexData ([ReferenceID],[EntityID],[IndexType],[CreatedOn],[Status],[IsPercolationNeeded])
		select NEWID(),ContactID,1,GETUTCDATE(),1,0
		FROM #DuplicateContactLeadSource
		

   

END 






































