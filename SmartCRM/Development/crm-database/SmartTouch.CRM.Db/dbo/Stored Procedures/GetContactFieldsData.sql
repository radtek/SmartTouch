
CREATE PROCEDURE [dbo].[GetContactFieldsData]
	@ContactId INT,
	@Fields VARCHAR(MAX),
	@NotificationType INT,
	@EntityID INT,
	@AccountID INT
AS
BEGIN
--select FieldID, Title, CASE WHEN (FieldCode = '' AND AccountID IS NULL) THEN 'D' WHEN (FieldCode != '' AND AccountID IS NULL) THEN 'S' ELSE 'C' END AS [Type],
-- CASE WHEN (FieldCode = '' AND AccountID IS NULL) THEN CAST(FieldID AS VARCHAR(250)) WHEN (FieldCode != '' AND AccountID IS NULL) THEN FieldCode ELSE CAST(FieldID AS VARCHAR(250)) END AS [Type]
--from fields where (accountid is null or accountid = 94) AND StatusID = 201 AND FieldCode != '' AND AccountID IS NULL
	DECLARE @FieldsList TABLE (ID INT IDENTITY(1,1), FieldID INT, FieldName VARCHAR(100), FieldType VARCHAR(1))
	DECLARE @ContactFields TABLE (ID INT IDENTITY(1,1), FieldID INT, FieldName VARCHAR(100), VALUE Varchar(MAX))
	DECLARE @FieldsListsCopy TABLE (ID INT, FieldID INT, FieldName VARCHAR(100), FieldType VARCHAR(1))

	INSERT INTO @FieldsList 
	SELECT FieldID, Title, CASE  WHEN (AccountID IS NULL) THEN 'S' ELSE 'C' END AS [FieldType]
	FROM Fields F (NOLOCK)
	INNER JOIN dbo.Split_2(@Fields,',') S ON S.DataValue = F.FieldID AND (AccountID IS NULL OR AccountID = @AccountID)
	UNION
	SELECT DropdownValueID, DropdownValue, 'D' FROM DropdownValues DV (NOLOCK)
	INNER JOIN dbo.Split_2(@Fields,',') S ON S.DataValue = DV.DropdownValueID and DV.AccountID = @AccountID
	
	INSERT INTO @FieldsListsCopy
	SELECT * FROM @FieldsList

	DECLARE @Counter INT = 1
	DECLARE @FieldID INT
	DECLARE @FieldName VARCHAR(100)
	DECLARE @FieldValue VARCHAR(MAX)
	DECLARE @FieldType VARCHAR(1)
	
	WHILE @Counter > 0
		BEGIN
			SELECT @FieldID = FieldID , @FieldName = FieldName, @FieldType = FieldType FROM @FieldsList WHERE FieldID = @Counter
			IF @FieldType = 'S'
			BEGIN
				IF @FieldID = 1 	-- First Name	
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, C.FirstName FROM Contacts C (NOLOCK) WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 2 	-- Last Name	
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, C.LastName FROM Contacts C (NOLOCK) WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 3	-- Company	
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.Company FROM Contacts C (NOLOCK) 
						INNER JOIN Contacts CM (NOLOCK) ON CM.ContactID = C.CompanyID
						WHERE C.ContactID = @ContactID
					END
				--ELSE IF @FieldID = 4	-- Mobile Phone	
				--	BEGIN
				--		INSERT INTO @ContactFields
				--	    SELECT @FieldID, @FieldName, C.PhoneNumber FROM ContactPhoneNumbers C (NOLOCK) 
				--	    INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = C.PhoneType
				--	 	WHERE C.ContactID = @ContactId and DDV.DropdownValue= 'Mobile'
				--	END
				--ELSE IF @FieldID = 5	-- Home Phone	
				--	BEGIN
				--		INSERT INTO @ContactFields
				--		SELECT @FieldID, @FieldName, C.PhoneNumber FROM ContactPhoneNumbers C (NOLOCK) 
				--	    INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = C.PhoneType
				--	 	WHERE C.ContactID = @ContactId and DDV.DropdownValue= 'Home'
				--	END
				--ELSE IF @FieldID = 6	-- Work Phone	
				--	BEGIN
				--		INSERT INTO @ContactFields
				--		SELECT @FieldID, @FieldName, C.PhoneNumber FROM ContactPhoneNumbers C (NOLOCK) 
				--	    INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = C.PhoneType
				--	 	WHERE C.ContactID = @ContactId and DDV.DropdownValue= 'Work'
				--	END
				ELSE IF @FieldID = 7 -- Email
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CE.Email FROM ContactEmails CE (NOLOCK) WHERE ContactID = @ContactId AND IsPrimary = 1 AND IsDeleted = 0
					END
				ELSE IF @FieldID = 8 	-- Title
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, C.Title FROM Contacts C (NOLOCK) WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 9 	-- FB URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.FacebookUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 10 	-- Twitter URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.TwitterUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID =11 	-- Linked IN URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.LinkedInUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 12 	-- Google Plus URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.GooglePlusUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 13 	-- Website URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.WebSiteUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 14 	-- Blog URL
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CM.BlogUrl FROM Contacts C (NOLOCK) 
						INNER JOIN Communications CM ON CM.CommunicationID = C.CommunicationID
						WHERE ContactID = @ContactID
					END
				ELSE IF @FieldID = 15 -- Address line 1
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, A.AddressLine1 FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 16 -- Address line 2
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, A.AddressLine2 FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 17 -- City
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, A.City FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 18 -- State
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, S.StateName FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						INNER JOIN States S (NOLOCK) ON S.StateID = A.StateID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 19 -- Zip Code
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, A.ZipCode FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 20 -- Country
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, A.CountryID  FROM ContactAddressMap CAM (NOLOCK) 
						INNER JOIN Addresses A (NOLOCK)  ON A.AddressID = CAM.AddressID
						INNER JOIN Countries (NOLOCK) CN ON CN.CountryID = A.CountryID
						WHERE CAM.ContactID = @ContactID
					END
				ELSE IF @FieldID = 21 -- Partner Type
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, DDV.DropdownValue FROM Contacts C (NOLOCK) 
						INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = C.PartnerType
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 22 -- Life Cycle Stage
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, DDV.DropdownValue FROM Contacts C (NOLOCK) 
						INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = C.LifecycleStage
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 23 -- Do not email
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName,
						CASE WHEN C.DoNotEmail = 1 THEN 'Yes' 
					    WHEN C.DoNotEmail = 0 THEN 'No'
						ELSE '' END
					    FROM Contacts C (NOLOCK) 
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 24
						BEGIN
							DECLARE @leadsources VARCHAR(max)
							SELECT   @leadsources = COALESCE(@leadsources + ', ', '') + DDV.DropdownValue FROM ContactLeadSourceMap CLM (NOLOCK) 
							INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CLM.ContactID
							INNER JOIN DropdownValues (NOLOCK) DDV ON DDV.DropdownValueID = CLM.LeadSouceID
							WHERE C.ContactID = @ContactID 
							INSERT INTO @ContactFields
							SELECT @FieldID, @FieldName,@leadsources  
							--INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CLM.ContactID
							--INNER JOIN DropdownValues (NOLOCK) DDV ON DDV.DropdownValueID = CLM.LeadSouceID
							--WHERE C.ContactID = @ContactID AND CLM.IsPrimaryLeadSource = 1
							--DELETE @FieldsList WHERE FieldID = 24
						END
				ELSE IF @FieldID = 25 -- Owner
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, U.FirstName + ' ' + U.LastName from Users U (NOLOCK) 
						INNER JOIN Contacts(NOLOCK) C ON C.OwnerID = U.UserID
						WHERE C.ContactID=@ContactId
					END
				ELSE IF @FieldID = 26 -- Lead Score
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, SUM(L.Score) FROM Contacts C (NOLOCK) 
						INNER JOIN LeadScores L (NOLOCK) ON L.ContactID = C.ContactID
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 27 -- created by
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName, U.FirstName + ' ' + U.LastName FROM Contacts C (NOLOCK) 
						INNER JOIN Contacts_Audit (NOLOCK) CA ON CA.ContactID = C.ContactID
						INNER JOIN Users (NOLOCK) U ON U.UserID = CA.LastUpdatedBy
						WHERE C.ContactID = @ContactId AND CA.AuditAction ='I'
					END
				ELSE IF @FieldID = 28 -- created on
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName, C.LastUpdatedOn FROM Contacts C (NOLOCK) 
						INNER JOIN Contacts_Audit (NOLOCK) CA ON CA.ContactID = C.ContactID
						WHERE C.ContactID = @ContactId AND CA.AuditAction ='I'
					END
				ELSE IF @FieldID = 29 -- Last Touched
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName, C.LastContacted FROM Contacts C (NOLOCK) 
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 41 -- Last Touched Method
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName,
						CASE WHEN C.LastContactedThrough = 4 THEN 'Campaign' 
					    WHEN C.LastContactedThrough = 26 THEN 'Text'
					    WHEN C.LastContactedThrough = 25 THEN 'Email'
					    WHEN C.LastContactedThrough = 46 THEN 'Phone Call'
						WHEN C.LastContactedThrough = 47 THEN 'Email'
						WHEN C.LastContactedThrough = 48 THEN 'Appointment'
						WHEN C.LastContactedThrough = 3 THEN 'Action-Other'
						WHEN C.LastContactedThrough = 6 THEN 'Note'
						WHEN C.LastContactedThrough = 7 THEN 'Tour'
						ELSE '' END
						FROM Contacts C (NOLOCK) 
						WHERE C.ContactID = @ContactId
					END
				ELSE IF @FieldID = 45 -- Web Page Visited
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName,CW.PageVisited FROM ContactWebVisits CW (NOLOCK) 
						INNER JOIN Contacts C (NOLOCK)  ON C.ContactID = CW.ContactID
						WHERE CW.ContactID = @ContactID
					END
				ELSE IF @FieldID = 46 -- Web Page Duration
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName,CW.Duration FROM ContactWebVisits CW (NOLOCK) 
						INNER JOIN Contacts C (NOLOCK)  ON C.ContactID = CW.ContactID
						WHERE CW.ContactID = @ContactID
					END
				ELSE IF @FieldID = 47 -- Tag
					BEGIN
						DECLARE @Names VARCHAR(max) 
                        SELECT @Names = COALESCE(@Names + ', ', '') + T.TagName  FROM vTags T (NOLOCK) 
						INNER JOIN ContactTagMap C (NOLOCK) on C.TagID = T.TagID
						WHERE C.ContactID = @ContactID AND C.AccountID = @AccountID

						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName,@Names
					END
				ELSE IF @FieldID = 48 -- Form Name
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName, F.Name  from Forms F
                        INNER JOIN FormSubmissions(NOLOCK) FS ON FS.FormID = F.FormID
                        WHERE FS.ContactID= @ContactID ORDER BY FS.FormSubmissionID DESC
					END
				ELSE IF @FieldID = 49 -- Form Submitted on
					BEGIN
						INSERT INTO @ContactFields
						SELECT TOP 1 @FieldID, @FieldName, FS.SubmittedOn  from Forms F
                        INNER JOIN FormSubmissions(NOLOCK) FS ON FS.FormID = F.FormID
                        WHERE FS.ContactID= @ContactID ORDER BY FS.FormSubmissionID DESC
					END
				ELSE IF @FieldID = 50 -- LeadSource Date
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CLM.LastUpdatedDate FROM ContactLeadSourceMap CLM (NOLOCK) 
						INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CLM.ContactID
						INNER JOIN DropdownValues (NOLOCK) DDV ON DDV.DropdownValueID = CLM.LeadSouceID
						WHERE C.ContactID = @ContactID AND CLM.IsPrimaryLeadSource = 1
					END
				ELSE IF @FieldID = 51 -- First Lead Source
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, DDV.DropdownValue FROM ContactLeadSourceMap CLM (NOLOCK) 
						INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CLM.ContactID
						INNER JOIN DropdownValues (NOLOCK) DDV ON DDV.DropdownValueID = CLM.LeadSouceID
						WHERE C.ContactID = @ContactID AND CLM.IsPrimaryLeadSource = 1
					END
				ELSE IF @FieldID = 52 -- First Lead Source Date
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName, CLM.LastUpdatedDate FROM ContactLeadSourceMap CLM (NOLOCK) 
						INNER JOIN Contacts (NOLOCK) C ON C.ContactID = CLM.ContactID
						INNER JOIN DropdownValues (NOLOCK) DDV ON DDV.DropdownValueID = CLM.LeadSouceID
						WHERE C.ContactID = @ContactID AND CLM.IsPrimaryLeadSource = 1
					END
				ELSE IF @FieldID = 61 -- LeadAdapter Type
					BEGIN
						INSERT INTO @ContactFields
						SELECT @FieldID, @FieldName,lt.Name  from Contacts(NOLOCK) c
                         INNER JOIN LeadAdapterJobLogDetails(NOLOCK) ld ON c.ReferenceID = ld.ReferenceID
                         INNER JOIN LeadAdapterJobLogs(NOLOCK) ljl ON ld.LeadAdapterJobLogID = ljl.LeadAdapterJobLogID
                         INNER JOIN LeadAdapterAndAccountMap(NOLOCK) ldm ON ljl.LeadAdapterAndAccountMapID = ldm.LeadAdapterAndAccountMapID
                         INNER JOIN LeadAdapterTypes(NOLOCK) lt ON ldm.LeadAdapterTypeID = lt.LeadAdapterTypeID
                         where c.ContactID= @ContactID ORDER BY ld.CreatedDateTime DESC
					END
				
				DELETE @FieldsList WHERE FieldID = @Counter
			END
			ELSE IF @FieldType = 'C' OR @FieldType = 'D'
				BEGIN
					DECLARE @PhoneTable table (FieldID INT, FieldName VARCHAR(100), VALUE Varchar(MAX))
					
					DECLARE @phFieldId INT = 0
					SELECT TOP 1 @phFieldId = FieldId FROM @FieldsList WHERE FieldType = 'D'

					WHILE (@phFieldId > 0)
						BEGIN
							declare @phFieldName VARCHAR(100) =null, @phoneNumber VARCHAR(MAX)=null
							select @phFieldId = dvs.DropdownValueID, @phFieldName=dvs.DropdownValue , @phoneNumber = COALESCE(@phoneNumber+', ','') + dv.PhoneWithFormat from GET_DropdownFieldValues (nolock) dv 
							inner join DropdownValues dvs (nolock) on dvs.DropdownValueID = dv.CustomFieldID and contactid = @ContactId 
							WHERE dvs.DropdownValueID = @phFieldId
							
							INSERT INTO @PhoneTable
							SELECT @phFieldId, @phFieldName, @phoneNumber

							DELETE FROM @FieldsList WHERE FieldID = @phFieldId
							SET @phFieldId = 0
							SELECT TOP 1 @phFieldId = FieldId FROM @FieldsList WHERE FieldType = 'D'

						END
					INSERT INTO @ContactFields
					SELECT F.FieldID, F.FieldName, CFV.FieldValue FROM GET_ContactCustomField_Values CFV (NOLOCK) 
					RIGHT JOIN @FieldsList F ON F.FieldID = CFV.CustomFieldID AND CFV.ContactID = @ContactID
					WHERE F.FieldType = 'C'
					UNION
					SELECT FieldID,FieldName,VALUE FROM @PhoneTable 
					
					DELETE @FieldsList WHERE FieldType IN ('C','D')
						

						
				END
			SET @Counter = 0
			SELECT TOP 1 @Counter = FieldID FROM @FieldsList
		END -- While Close
		
		SELECT DISTINCT FL.FieldID, FL.FieldName, CF.VALUE FROM @ContactFields CF
		RIGHT JOIN @FieldsListsCopy FL ON FL.FieldID = CF.FieldID
		ORDER BY FieldID ASC
		

		IF @NotificationType = 3 -- Forms
			BEGIN 
				DECLARE @IsApiForm BIT = 0
				DECLARE @submittedData nvarchar(max) = ''

				SELECT @IsApiForm = IsAPIForm, @submittedData = Fs.SubmittedData FROM Forms (NOLOCK) F
				INNER JOIN FormSubmissions (NOLOCK) FS ON FS.FormID = F.FormID
				WHERE FormSubmissionID=@EntityID

				IF @IsApiForm = 1
					SELECT '' AS SubmittedData
				ELSE
					SELECT @submittedData AS SubmittedData					
			END
		ELSE IF @NotificationType = 22 -- LeadAdapter 
			BEGIN
			    SELECT TOP 1 ld.SubmittedData FROM Contacts(NOLOCK) c
                INNER JOIN LeadAdapterJobLogDetails(NOLOCK) ld ON c.ReferenceID = ld.ReferenceID
                INNER JOIN LeadAdapterJobLogs(NOLOCK) ljl ON ld.LeadAdapterJobLogID = ljl.LeadAdapterJobLogID
                WHERE c.ContactID=@ContactID ORDER BY ld.CreatedDateTime DESC

			END
		ELSE
			BEGIN
			    SELECT ''
			END

END


--EXEC [dbo].[GetContacttFieldsData] 1890988,'1,2,7,3,24,25,22,26,4653,4652,4654',12,0,4218
--1740634

--select * from dropdownvalues (nolock) where dropdownid = 1 and accountid = 4218
--



--select * from ContactPhoneNumbers where contactid = 1890988
--select * from dropdownvalues where dropdownid = 1 and (accountid = 4218 or accountid is null)




