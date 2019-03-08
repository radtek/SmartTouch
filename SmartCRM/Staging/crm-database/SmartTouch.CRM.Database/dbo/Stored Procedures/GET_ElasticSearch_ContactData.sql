

CREATE PROCEDURE [dbo].[GET_ElasticSearch_ContactData]
	(
		@ContactsList	dbo.Contact_List READONLY
	)
AS
BEGIN	
	SET NOCOUNT ON
	BEGIN TRY	
			SELECT CL.*, C.AccountID  INTO #MomentaryContactsList FROM @ContactsList CL
			INNER JOIN Contacts C (NOLOCK) on CL.ContactID = C.ContactID
			WHERE C.IsDeleted = 0

			SELECT C.* 
				FROM dbo.Contacts C (NOLOCK)
					INNER JOIN #MomentaryContactsList CL ON CL.ContactID = C.ContactID AND CL.AccountID = C.AccountID
			WHERE C.IsDeleted = 0
			
			
			--Get Addresses
			SELECT CL.ContactID, A.* 
				FROM #MomentaryContactsList CL
					INNER JOIN dbo.ContactAddressMap CAM  (NOLOCK) ON CL.ContactID = CAM.ContactID
					INNER JOIN dbo.Addresses A  (NOLOCK) ON A.AddressID = CAM.AddressID AND A.IsDefault = 1
						
			--Get Communications
			SELECT CL.ContactID, CM.* 
				FROM dbo.Contacts C  (NOLOCK)
					INNER JOIN #MomentaryContactsList CL ON CL.ContactID = C.ContactID AND CL.AccountID = C.AccountID
					INNER JOIN dbo.Communications (NOLOCK) CM ON CM.CommunicationID = C.CommunicationID

			--Get Emails
			SELECT CE.* 
				FROM #MomentaryContactsList CL 
					INNER JOIN dbo.ContactEmails CE  (NOLOCK) ON CL.ContactID = CE.ContactID WHERE CE.IsDeleted = 0 AND CE.IsPrimary = 1 AND CL.AccountID = CE.AccountID

			--Get Contact phone numbers
			SELECT CPN.* 
				FROM #MomentaryContactsList CL 
					INNER JOIN dbo.ContactPhoneNumbers CPN  (NOLOCK) ON CL.ContactID = CPN.ContactID	WHERE CPN.IsDeleted = 0	AND CPN.IsPrimary = 1
				
			--Get Images
			SELECT CL.ContactID, I.* 
				FROM dbo.Contacts C
					INNER JOIN #MomentaryContactsList CL ON CL.ContactID = C.ContactID AND CL.AccountID = C.AccountID
					INNER JOIN dbo.Images I  (NOLOCK) ON C.ImageID = I.ImageID

			--Get lead sources
			SELECT CLSM.* 
				FROM #MomentaryContactsList CL 
					INNER JOIN dbo.ContactLeadSourceMap CLSM  (NOLOCK) ON CL.ContactID = CLSM.ContactID
					ORDER BY CLSM.IsPrimaryLeadSource DESC
			--Get custom fields
			SELECT CCFM.*, F.FieldInputTypeID
				FROM #MomentaryContactsList CL
					INNER JOIN dbo.ContactCustomFieldMap CCFM  (NOLOCK) ON CL.ContactID = CCFM.ContactID
					INNER JOIN dbo.Fields F  (NOLOCK) on F.FieldID = CCFM.CustomFieldID
				WHERE LEN(CCFM.Value) > 0  AND F.StatusID != 204

		    -- Get phone types			
            SELECT DISTINCT  D.* 
			  FROM #MomentaryContactsList CL
			  INNER JOIN ContactPhoneNumbers CP  (NOLOCK) ON CL.ContactID = CP.ContactID
              INNER JOIN DropdownValues D  (NOLOCK) ON CP.PhoneType = D.DropdownValueID and D.DropdownID = 1 WHERE CP.IsDeleted = 0


			-- Get Address States
            SELECT DISTINCT  S.* 
			  FROM #MomentaryContactsList CL
			  INNER JOIN  ContactAddressMap CA  (NOLOCK) ON CL.ContactID = CA.ContactID
              INNER JOIN Addresses A  (NOLOCK) ON CA.AddressID = A.AddressID
              INNER JOIN States S  (NOLOCK) ON A.StateID = S.StateID

			-- Get Address Countries
            SELECT DISTINCT  CU.* 
			  FROM #MomentaryContactsList CL
			  INNER JOIN  ContactAddressMap CA  (NOLOCK) ON CL.ContactID = CA.ContactID
              INNER JOIN Addresses A  (NOLOCK) ON CA.AddressID = A.AddressID
              INNER JOIN Countries CU  (NOLOCK) ON A.CountryID = CU.CountryID

			--Get WebVisits
			SELECT CW.ContactID, CW.PageVisited, CW.Duration, CW.VisitedOn FROM #MomentaryContactsList CL
			  INNER JOIN ContactWebVisits CW  (NOLOCK) ON CW.ContactID = CL.ContactID

			--GET TAGS 
			SELECT CTM.ContactID, CTM.TagID FROM #MomentaryContactsList CL
			  INNER JOIN ContactTagMap CTM  (NOLOCK) ON CTM.ContactID = CL.ContactID AND CL.AccountID = CTM.AccountID

			--GET FORM-SUBMISSIONS
			SELECT FS.FormID, FS.ContactID, FS.SubmittedOn FROM #MomentaryContactsList CL
			  INNER JOIN FormSubmissions FS  (NOLOCK) ON FS.ContactID = CL.ContactID 
            ;WITH C_A
			AS
			(
				SELECT C.contactid as ContactId, 
				COALESCE(CA.lastupdatedon, CT.lastupdatedon , GETUTCDATE()) AS CreatedOn, 
				COALESCE(CA.LastUpdatedBy, CT.LastUpdatedBy,1) AS CreatedBy , ROW_NUMBER() OVER(PARTITION BY C.ContactID ORDER BY CA.LastUpdatedOn ASC) RowNumber
				FROM contacts_audit CA  (NOLOCK)
							right outer join #MomentaryContactsList C on C.contactid = CA.contactid and C.AccountID = CA.AccountID
							INNER JOIN Contacts CT (NOLOCK) ON CT.ContactID = C.ContactID AND C.AccountID = CT.AccountID
			)
			SELECT ContactId, CreatedOn, CreatedBy FROM C_A WHERE RowNumber = 1

			--Contact Communities
			SELECT DISTINCT CL.ContactID,T.CommunityID, DV.* FROM #MomentaryContactsList CL 
			INNER JOIN ContactTourMap (NOLOCK) CTM on CTM.ContactID = cl.ContactID
			INNER JOIN Tours (NOLOCK) T ON T.TourID = CTM.TourID
			INNER JOIN DropdownValues (NOLOCK) DV ON DV.DropdownValueID = T.CommunityID
			WHERE DV.IsDeleted = 0 AND DV.DropdownID = 7
			UNION ALL
			SELECT DISTINCT CL.ContactID,CCM.CommunityID, DV.* FROM #MomentaryContactsList CL 
			INNER JOIN ContactCommunityMap (NOLOCK) CCM ON CCM.ContactID = CL.ContactID
			INNER JOIN DropdownValues (NOLOCK) DV ON DV.DropdownValueID = CCM.CommunityID
			WHERE DV.IsDeleted = 0 AND DV.DropdownID = 7

			-- Contact Last Note Details
			select CNM.ContactID as ContactId, 
			(select dbo.GenerateConsolidatedNoteDetails(cnm.contactid)) as NoteDetails,
			lni.CreatedOn as LastNoteDate,lni.NoteDetails as LastNote, lni.NoteCategory as LastNoteCategory
			from #MomentaryContactsList CL
			INNER JOIN  ContactNoteMap cnm (NOLOCK) ON cnm.ContactID = CL.ContactID
			inner join notes n (NOLOCK) on cnm.NoteID = n.noteid 
			inner join GetContactLastNoteInfo lni on cnm.Contactid = lni.Contactid
			INNER JOIN Contacts (NOLOCK) C ON C.CONTACTID = CNM.ContactID
			where n.AddToContactSummary = 1 and lni.SortOrder = 1 AND C.IsDeleted = 0


			--;WITH CTE AS (
			--SELECT MC.ContactID, 
			--N.NoteDetails AS LastNote,
			--ROW_NUMBER() OVER (PARTITION BY MC.CONTACTID ORDER BY N.CreatedOn DESC) AS Ranking
			--FROM #MomentaryContactsList MC
			--JOIN ContactNoteMap(NOLOCK) CNM ON CNM.ContactID = MC.ContactID
			--JOIN Notes (NOLOCK) N ON N.NoteID = CNM.NoteID
			--WHERE N.AddToContactSummary=1 AND N.AccountID = MC.AccountID)

			--SELECT ContactID, LastNote FROM CTE WHERE Ranking = 1

			--Contact Tours
			SELECT DISTINCT T.TourID, T.CommunityID, T.TourType, T.TourDate, T.CreatedBy, CTM.ContactID,
			 STUFF((SELECT  ', '+ CAST(UserID AS VARCHAR(10)) FROM UserTourMap (NOLOCK) WHERE TourID = T.TourID FOR XML PATH('')),1,2,'') As Users FROM #MomentaryContactsList CL 
			INNER JOIN ContactTourMap (NOLOCK) CTM on CTM.ContactID = cl.ContactID
			INNER JOIN Tours (NOLOCK) T ON T.TourID = CTM.TourID
			UNION ALL
			SELECT DISTINCT 0, CCM.CommunityID, 0,NULL, 0, CL.ContactID, '' As Users FROM #MomentaryContactsList CL 
			INNER JOIN ContactCommunityMap (NOLOCK) CCM ON CCM.ContactID = CL.ContactID


			--Contact Actions
			SELECT A.ActionID, CreatedOn, ActionType, ActionDate, CAM.ContactID,
			STUFF((SELECT  ', '+ CAST(UserID AS VARCHAR(10)) FROM UserActionMap (NOLOCK) WHERE ActionID = A.ActionID FOR XML PATH('')),1,2,'') As Users FROM #MomentaryContactsList CL 
			JOIN ContactActionMap (NOLOCK) CAM ON CAM.ContactID = CL.ContactID
			JOIN Actions (NOLOCK) A ON A.ActionID = CAM.ActionID

			-- Contact Notes
			SELECT N.NoteID,N.CreatedOn,N.CreatedBy,N.NoteDetails,N.NoteCategory,CNM.ContactID FROM #MomentaryContactsList CL 
			JOIN ContactNoteMap (NOLOCK) CNM ON CNM.ContactID=CL.ContactID
			JOIN Notes (NOLOCK) N ON N.NoteID = CNM.NoteID

			SELECT CL.ContactID, CASE WHEN AC.ContactID IS NULL THEN 0 ELSE 1 END AS IsActive FROM #MomentaryContactsList CL 
			LEFT JOIN ActiveContacts(NOLOCK) AC ON AC.ContactID = CL.ContactID
			--ORDER BY AC.CreatedOn DESC

	END TRY
	BEGIN CATCH
		SELECT CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE()

	END CATCH
	SET NOCOUNT OFF
END




SET STATISTICS TIME ON 
--drop table #MomentaryContactsList
--DECLARE @ContactsList dbo.Contact_List
--	INSERT INTO @ContactsList (ContactID)
--	select 1893060
--exec [dbo].[GET_ElasticSearch_ContactData] @ContactsList

--	SELECT CL.*  INTO #MomentaryContactsList FROM @ContactsList CL
--			INNER JOIN Contacts C (NOLOCK) on CL.ContactID = C.ContactID
--			WHERE C.IsDeleted = 0
		

--	Select * from #MomentaryContactsList CL
--			INNER JOIN ContactSummary CS on CL.ContactID = CS.ContactID
--			INNER JOIN Contacts (NOLOCK) C ON C.CONTACTID = CS.ContactID
--				WHERE C.IsDeleted = 0

--SET STATISTICS TIME OFF




