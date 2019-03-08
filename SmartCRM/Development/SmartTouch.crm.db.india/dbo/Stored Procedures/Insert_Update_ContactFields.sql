
CREATE PROC [dbo].[Insert_Update_ContactFields]
	 @contactCustomFieldMaps dbo.ContactCustomFieldMapType READONLY
	,@contactPhoneNumbers dbo.ContactPhoneNumberType READONLY
	,@contactEmails dbo.ContactEmailType READONLY
	,@contactCommunityMap dbo.ContactCommunityMapType READONLY
	,@contactId int
	,@companyId int
	,@accountId int
	,@contactType tinyint

AS
BEGIN
	BEGIN TRY
				--Custom Fields insertion and updation started

					 
					UPDATE CC
						   set
							ContactID = B.ContactID
						   ,CustomFieldID = B.CustomFieldID
						   ,Value = B.Value
					FROM ContactCustomFieldMap CC
					JOIN @contactCustomFieldMaps B ON CC.ContactID = B.ContactID AND CC.ContactCustomFieldMapId = B.ContactCustomFieldMapId;

					DELETE FROM ContactCustomFieldMap WHERE CustomFieldID NOT IN(SELECT CustomFieldID FROM @contactCustomFieldMaps) AND ContactID=CASE WHEN @contactType = 1 THEN @contactId  
																																					    WHEN @contactType = 2 THEN @companyId
																																						END 

					INSERT INTO ContactCustomFieldMap (ContactID,CustomFieldID,Value)
					SELECT CASE WHEN @contactType = 1 THEN @contactId  
								WHEN @contactType = 2 THEN @companyId END 
								,CustomFieldID,Value from @contactCustomFieldMaps where ContactCustomFieldMapId is null or ContactCustomFieldMapId = 0;

					--;MERGE ContactCustomFieldMap CC
					--USING (SELECT DISTINCT ContactId,CustomFieldId,Value FROM @contactCustomFieldMaps) AS S (ContactId,CustomFieldId,Value) ON (CC.ContactID = S.ContactID AND CC.CustomFieldID = S.CustomFieldID)
					--WHEN MATCHED THEN UPDATE SET CC.[Value] = S.[Value]
					--WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (CASE WHEN @contactType = 1 THEN @contactId  
					--													WHEN @contactType = 2 THEN @companyId
					--													END , S.CustomFieldID, S.[Value]);
 
			--ContactPhoneNumbers insertion and updation started

					UPDATE A
						   set
							PhoneNumber = B.Number
						   ,IsPrimary = B.IsPrimary
						   ,AccountID = @accountId
						   ,IsDeleted = B.IsDeleted
						   ,CountryCode = B.CountryCode
						   ,Extension = B.Extension
					FROM ContactPhoneNumbers A
					JOIN @contactPhoneNumbers B ON A.ContactID = B.ContactID AND A.ContactPhoneNumberID = B.ContactPhoneNumberID;

					UPDATE ContactPhoneNumbers SET IsDeleted=1   WHERE ContactPhoneNumberID NOT IN (select ContactPhoneNumberID from @contactPhoneNumbers) AND ContactID=CASE WHEN @contactType = 1 THEN @contactId  
																																											  WHEN @contactType = 2 THEN @companyId
																																											  END 
					insert into ContactPhoneNumbers (ContactID,PhoneNumber,PhoneType,IsPrimary,AccountID,IsDeleted, CountryCode, Extension)
					select CASE WHEN @contactType = 1 THEN @contactId  
								WHEN @contactType = 2 THEN @companyId END 
								,Number,PhoneType,IsPrimary,@accountId,IsDeleted,CountryCode, Extension from @contactPhoneNumbers where ContactPhoneNumberID is null or ContactPhoneNumberID = 0;

			--ContactEmails insertion and updation started

					UPDATE A
						   set
						   Email = B.EmailId
						   ,EmailStatus = B.EmailStatus
						   ,IsPrimary = B.IsPrimary
						   ,AccountID =@accountId
						   ,SnoozeUntil=B.SnoozeUntil
						   ,IsDeleted = B.IsDeleted
					FROM ContactEmails A
					JOIN @contactEmails B ON A.ContactID = B.ContactID AND A.ContactEmailID = B.ContactEmailID;
 
					UPDATE ContactEmails SET IsDeleted=1 WHERE ContactEmailID NOT IN (select ContactEmailID from @contactEmails) AND ContactID=CASE WHEN @contactType = 1 THEN @contactId  
																																					WHEN @contactType = 2 THEN @companyId
																																					END 
					insert into ContactEmails(ContactID,Email,EmailStatus,IsPrimary,AccountID,SnoozeUntil,IsDeleted)
					select CASE WHEN @contactType = 1 THEN @contactId  
								WHEN @contactType = 2 THEN @companyId END 
								,EmailId,EmailStatus,IsPrimary,@accountId,SnoozeUntil,IsDeleted from @contactEmails where ContactEmailID is null or ContactEmailID = 0;

			--ContactCommunityMap insertion and updation started.

					UPDATE A
					       set 
					       CommunityID = B.CommunityID
					       ,CreatedOn = GETUTCDATE()
						   ,CreatedBy = B.CreatedBy
						   ,LastModifiedOn = B.LastModifiedOn
						   ,LastModifiedBy = B.LastModifiedBy
						   ,IsDeleted = B.IsDeleted
					FROM ContactCommunityMap A
					JOIN @contactCommunityMap B ON A.ContactID = B.ContactID AND A.ContactCommunityMapID = B.ContactCommunityMapID;

					DELETE FROM ContactCommunityMap WHERE ContactID =@contactId AND CommunityID IN (SELECT CommunityID FROM @contactCommunityMap)
 
					insert into ContactCommunityMap (ContactID,CommunityID,CreatedOn,CreatedBy,LastModifiedOn,LastModifiedBy,IsDeleted)
					select @contactId,CommunityID,GETUTCDATE(),CreatedBy,LastModifiedOn,LastModifiedBy,IsDeleted from @contactCommunityMap where ContactCommunityMapID is null or ContactCommunityMapID = 0;

	END TRY
	BEGIN CATCH
				INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
							VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), NULL, @accountId)
    END CATCH;
END