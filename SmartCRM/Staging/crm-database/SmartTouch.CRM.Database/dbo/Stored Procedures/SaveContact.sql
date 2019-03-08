

CREATE proc [dbo].[SaveContact]
	 @contact dbo.ContactType READONLY
	,@addresses dbo.AddresseType READONLY
	,@communication dbo.CommunicationType READONLY
	,@image dbo.ImageType READONLY
	,@contactCustomFieldMaps dbo.ContactCustomFieldMapType READONLY
	,@contactPhoneNumbers dbo.ContactPhoneNumberType READONLY
	,@contactEmails dbo.ContactEmailType READONLY
	,@contactLeadSourceMap dbo.ContactLeadSourceMapType READONLY
	,@contactCommunityMap dbo.ContactCommunityMapType READONLY
AS
BEGIN
	BEGIN TRY
			--Variables Declearation

				DECLARE @Addressids table (AddressId INT)
				DECLARE @communicationId int
				DECLARE @accountId int
				DECLARE @imageId int
				DECLARE @company varchar(75)
				DECLARE @companyId int
				DECLARE @contactId int
				DECLARE @contactType tinyint
				DECLARE @OwnerId INT
				DECLARE @isContactExists bit = 0
				DECLARE @isCompanyExits bit = 0
				DECLARE @ContactSource tinyint 
			--Inserting and Updating  Company started

				SET  @company = (SELECT Company FROM @contact)
				SET @companyId = (SELECT CompanyID FROM @contact)
				SET @accountId =(SELECT AccountID FROM @contact)
				SET @contactType =(SELECT ContactType FROM @contact)
				SET @OwnerId =(SELECT OwnerId FROM @contact)
				set @ContactSource = (SELECT ContactSource FROM @contact)
				IF @OwnerId = 0
					SET @OwnerId = NULL
				
				IF(@company IS NOT NULL AND @company <> '') -- checking for comany name is not empty
				BEGIN
					-- Checking for Company is already exits are not.

					IF(@companyId IS NOT NULL AND @companyId > 0)
						BEGIN
						 SET @companyId = (SELECT TOP 1 C.ContactID  FROM Contacts (NOLOCK) C WHERE C.IsDeleted=0 AND C.ContactID=@companyId AND C.ContactType=2 AND C.Company= @company AND C.AccountID=@accountId)
						END
					 ELSE
						BEGIN
						  SET @companyId = (SELECT TOP 1 C.ContactID  FROM Contacts (NOLOCK) C WHERE C.IsDeleted=0 AND C.ContactType=2 AND C.Company=@company  AND C.AccountID = @accountId)
						END
				 IF(@companyId is NULL OR  @companyId = 0) -- if id is 0 or null then inserting company
				 BEGIN
					   IF(@company IS NOT NULL AND @company <> '')
					   BEGIN
							INSERT INTO Contacts(Company,AccountID,ContactType,DoNotEmail
						   ,IsDeleted,ReferenceID,LastUpdatedBy,LastUpdatedOn,OwnerID,ContactSource,
						   LeadScore)
						   SELECT Company,AccountID,2,DoNotEmail
						   ,IsDeleted,NEWID(),LastUpdatedBy,LastUpdatedOn,@OwnerId,ContactSource,
						   LeadScore FROM @contact;
 
					       SET @companyId  = SCOPE_IDENTITY();

						   IF(@companyId IS NOT NULL AND @companyId > 0 AND  @ContactSource IS NOT NULL AND @ContactSource = 3)
						   BEGIN
								INSERT INTO IndexData(ReferenceID,EntityID,IndexType,CreatedOn,IndexedOn,Status,IsPercolationNeeded)
								VALUES(NEWID(),@companyId,1,GETUTCDATE(),NULL,1,0)
						   END
					   END

					   IF(@contactType = 1)
					   BEGIN
							INSERT INTO Communications(SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl,FacebookAccessToken,TwitterOAuthToken,TwitterOAuthTokenSecret)
							VALUES(NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)

							SELECT @communicationId = SCOPE_IDENTITY();
							UPDATE Contacts SET CommunicationID = @communicationId WHERE ContactID = @companyId
					   END

					END
					ELSE -- updating company
					BEGIN
						 if(@contactType = 2)
						 begin
							  SET @isCompanyExits = 1	
								
							   UPDATE C
							   SET
								Company      = CT.Company
							   ,ContactType  = 2
							   ,DoNotEmail   = CT.DoNotEmail
							   ,IsDeleted    = CT.IsDeleted
							   ,ReferenceID  = NEWID()
							   ,LastUpdatedBy       = CT.LastUpdatedBy
							   ,LastUpdatedOn       = CT.LastUpdatedOn
							   ,OwnerID      = @OwnerId
							   ,ContactSource = CT.ContactSource
							   --,LeadScore    = CT.LeadScore
							   FROM Contacts C JOIN @contact CT ON C.ContactID = @companyId AND C.AccountID = CT.AccountID;
						 end
					END
					END

			--Inserting and Updating  Contact started

					IF NOT EXISTS (SELECT 1 FROM Contacts C JOIN @contact CT ON C.ContactID = CT.ContactID AND C.AccountID = CT.AccountID) -- checking contact already exits or not if not then inserting
					BEGIN
						   IF(@contactType = 1)
						   BEGIN
								 INSERT INTO Contacts(FirstName,LastName,Company,Title,ContactImageUrl,AccountID,LeadSource,HomePhone,WorkPhone
								,MobilePhone,PrimaryEmail,ContactType,SSN,LifecycleStage,DoNotEmail,LastContacted
								,IsDeleted,ProfileImageKey,ReferenceID,LastUpdatedBy,LastUpdatedOn,OwnerID
								,PartnerType,ContactSource,SourceType,CompanyID,LastContactedThrough,FirstContactSource
								,FirstSourceType,LeadScore)
								SELECT FirstName,LastName,Company,Title,ContactImageUrl,AccountID,LeadSource,HomePhone,WorkPhone
								,MobilePhone,PrimaryEmail,ContactType,SSN,LifecycleStage,DoNotEmail,LastContacted
								,IsDeleted,ProfileImageKey,NEWID(),LastUpdatedBy,LastUpdatedOn,@OwnerId
								,PartnerType,ContactSource,SourceType, @companyId as CompanyID,LastContactedThrough,FirstContactSource
								,FirstSourceType,LeadScore FROM @contact;
 
							   SET @contactId = SCOPE_IDENTITY();
						   END
      
 
					END
					ELSE -- updating contact
					BEGIN
						   SET @isContactExists=1	

						   UPDATE C
						   SET
						   FirstName     = CT.FirstName
						   ,LastName     = CT.LastName
						   ,Company      = CT.Company
						   ,CommunicationID     = (SELECT C.CommunicationID  FROM Communications(NOLOCK) CM join Contacts (NOLOCK) C 
												  ON C.CommunicationID=CM.CommunicationID  WHERE C.ContactID =(SELECT CONTACTID FROM @contact))
						   ,Title = CT.Title
						   ,ContactImageUrl     = CT.ContactImageUrl
						   ,LeadSource   = CT.LeadSource
						   ,HomePhone    = CT.HomePhone
						   ,WorkPhone    = CT.WorkPhone
						   ,MobilePhone  = CT.MobilePhone
						   ,PrimaryEmail = CT.PrimaryEmail
						   ,ContactType  = CT.ContactType
						   ,SSN   = CT.SSN
						   ,LifecycleStage      = CT.LifecycleStage
						   ,DoNotEmail   = CT.DoNotEmail
						   ,LastContacted       = CT.LastContacted
						   ,IsDeleted    = CT.IsDeleted
						   ,ProfileImageKey     = CT.ProfileImageKey
						   ,ImageID      = CT.ImageID
						   ,ReferenceID  = CT.ReferenceID
						   ,LastUpdatedBy       = CT.LastUpdatedBy
						   ,LastUpdatedOn       = CT.LastUpdatedOn
						   ,OwnerID      = @OwnerId
						   ,PartnerType  = CT.PartnerType
						   ,ContactSource       = CT.ContactSource
						   ,SourceType   = CT.SourceType
						   ,CompanyID    = @companyId
						   ,IncludeInReports = CT.IncludeInReports
						   ,LastContactedThrough      = CT.LastContactedThrough
						   --,FirstContactSource  = CT.FirstContactSource
						   --,FirstSourceType     = CT.FirstSourceType
						   --,LeadScore    = CT.LeadScore
                            FROM Contacts C JOIN @contact CT ON C.ContactID = CT.ContactID AND C.AccountID = CT.AccountID;
 
						   SELECT @contactId = C.ContactID FROM Contacts C JOIN @contact CT ON C.ContactID = CT.ContactID AND C.AccountID = CT.AccountID;
					END

			--Addresses intering and Updating Started

					UPDATE A
						   SET
						   AddressTypeID = B.AddressTypeID
						   ,AddressLine1 = B.AddressLine1
						   ,AddressLine2 = B.AddressLine2
						   ,City = B.City
						   ,StateID = B.StateID
						   ,CountryID = B.CountryID
						   ,ZipCode = B.ZipCode
						   ,IsDefault = B.IsDefault
						   FROM Addresses A
						   JOIN @addresses B ON A.AddressID = B.AddressID AND B.AddressID > 0;
 
					INSERT INTO Addresses (AddressTypeID,AddressLine1,AddressLine2,City,StateID,CountryID,ZipCode,IsDefault) output INSERTED.AddressId INTO @Addressids(AddressId)
					SELECT AddressTypeID,AddressLine1,AddressLine2,City,StateID,CountryID,ZipCode,IsDefault FROM @addresses where AddressID = 0 OR AddressID is null;
 
					DELETE FROM  ContactAddressMap WHERE AddressID NOT IN (select AddressID from @addresses) AND ContactID = CASE WHEN @contactType = 1 THEN @contactId  
																																  WHEN @contactType = 2 THEN @companyId
																																  END 
					INSERT INTO ContactAddressMap (AddressID, ContactID)
					SELECT AddressId,
					CASE WHEN @contactType = 1 THEN @contactId  
						 WHEN @contactType = 2 THEN @companyId
						 END FROM @Addressids;
 

			--Communications inserting and Updating started

					SELECT @communicationId = CommunicationID, @accountId = AccountID, @imageId = ImageID from Contacts where ContactID = CASE WHEN @contactType = 1 THEN @contactId  
																																			   WHEN @contactType = 2 THEN @companyId
																																			   END 
 
					IF @communicationId IS NULL
					BEGIN
						   INSERT INTO Communications(SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl,FacebookAccessToken,TwitterOAuthToken,TwitterOAuthTokenSecret)
						   SELECT SecondaryEmails,FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl,FacebookAccessToken,TwitterOAuthToken,TwitterOAuthTokenSecret from @communication
						   select @communicationId = SCOPE_IDENTITY();
						   UPDATE Contacts SET CommunicationID = @communicationId WHERE ContactID = CASE WHEN @contactType = 1 THEN @contactId  
																										 WHEN @contactType = 2 THEN @companyId
																										 END 
					END
					ELSE
					BEGIN
						   UPDATE A
						   SET
						   SecondaryEmails = B.SecondaryEmails
						   ,FacebookUrl = B.FacebookUrl
						   ,TwitterUrl = B.TwitterUrl
						   ,GooglePlusUrl = B.GooglePlusUrl
						   ,LinkedInUrl = B.LinkedInUrl
						   ,BlogUrl = B.BlogUrl
						   ,WebSiteUrl = B.WebSiteUrl
						   ,FacebookAccessToken = B.FacebookAccessToken
						   ,TwitterOAuthToken = B.TwitterOAuthToken
						   ,TwitterOAuthTokenSecret = B.TwitterOAuthTokenSecret
						   FROM Communications A
						   JOIN @communication B ON A.CommunicationID = (SELECT C.CommunicationID  FROM Communications(NOLOCK) CM join Contacts (NOLOCK) C 
												  ON C.CommunicationID=CM.CommunicationID  WHERE C.ContactID =(SELECT CASE WHEN @contactType = 1 THEN ContactID  
																														   WHEN @contactType = 2 THEN CompanyID END FROM @contact))

						   WHERE A.CommunicationID = (SELECT C.CommunicationID  FROM Communications(NOLOCK) CM join Contacts (NOLOCK) C 
												  ON C.CommunicationID=CM.CommunicationID  WHERE C.ContactID =(SELECT CASE WHEN @contactType = 1 THEN ContactID  
																														   WHEN @contactType = 2 THEN CompanyID END FROM @contact))
					END
 
			--Images insertion and updation started

					IF (@imageId IS NULL OR @imageId = 0 )
					BEGIN
						   IF EXISTS (SELECT 1 FROM @image) -- checking contact already exits or not if not then inserting
						   BEGIN
							   INSERT INTO Images(FriendlyName,StorageName,OriginalName,CreatedBy,CreatedDate,ImageCategoryID,AccountID)
							   SELECT CASE WHEN FriendlyName IS NULL THEN ' ' ELSE FriendlyName END,StorageName,OriginalName,CreatedBy,GETUTCDATE(),CategoryId,@accountId from @image
							   select @imageId = SCOPE_IDENTITY();
							   UPDATE Contacts SET ImageID = @imageId WHERE ContactID = CASE WHEN @contactType = 1 THEN @contactId  
																							 WHEN @contactType = 2 THEN @companyId
																							 END 
						   END
					END
 
			--ContactleadsourceMap insertion and updation started

					DECLARE @ExistPrimaryId SMALLINT
					DECLARE @NewPrimaryId SMALLINT
					DECLARE @ExistingDateTime DATETIME

					IF(@contactType = 1)
					BEGIN
						  SELECT @ExistPrimaryId = clm.LeadSouceID,@ExistingDateTime =clm.LastUpdatedDate FROM ContactLeadSourceMap (NOLOCK) clm WHERE clm.ContactID = @contactId and IsPrimaryLeadSource=1
					      SELECT @NewPrimaryId = tlsm.LeadSouceID  FROM @contactLeadSourceMap tlsm where tlsm.IsPrimary = 1
						  IF(@ExistPrimaryId = @NewPrimaryId)
						  BEGIN
								UPDATE A
									   set
									   LeadSouceID = B.LeadSouceID
									   ,IsPrimaryLeadSource = B.IsPrimary
									   ,LastUpdatedDate = @ExistingDateTime
								FROM ContactLeadSourceMap A
								JOIN @contactLeadSourceMap B ON A.ContactID = B.ContactID AND A.ContactLeadSourceMapID = B.ContactLeadSourceMapID;
 
								DELETE FROM  ContactLeadSourceMap WHERE  ContactID = @contactId
								insert into ContactLeadSourceMap (ContactID,LeadSouceID,IsPrimaryLeadSource,LastUpdatedDate)
								select @contactId,LeadSouceID,IsPrimary,@ExistingDateTime from @contactLeadSourceMap where ContactLeadSourceMapID is null or ContactLeadSourceMapID = 0 and LeadSouceID > 0;
						  END
						  ELSE
						  BEGIN
								DELETE FROM  ContactLeadSourceMap WHERE  ContactID = @contactId
								insert into ContactLeadSourceMap (ContactID,LeadSouceID,IsPrimaryLeadSource,LastUpdatedDate)
								select @contactId,LeadSouceID,IsPrimary,GETUTCDATE() from @contactLeadSourceMap where ContactLeadSourceMapID is null or ContactLeadSourceMapID = 0 and LeadSouceID > 0;
						  END

					END

			-- Inserting and Updating Contact Emails, Phones, CustomFields
					EXEC [dbo].[Insert_Update_ContactFields] @contactCustomFieldMaps,@contactPhoneNumbers,@contactEmails,@contactCommunityMap,@contactId,@companyId,@accountId,@contactType;


		   SELECT isnull(@contactId,0) as ContactID
		   SELECT isnull(@companyId,0) as CompanyID

END TRY
BEGIN CATCH
	IF(@contactId IS NOT NULL AND @contactId > 0 AND @isContactExists = 0)
		BEGIN
			update Contacts set IsDeleted=1 where ContactID=@contactId 
		END
	ELSE IF(@companyId IS NOT NULL AND @companyId > 0 AND @isCompanyExits = 0 AND @contactType = 2)
		BEGIN
			update Contacts set IsDeleted=1 where ContactID=@companyId 
		END
		
	INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
				VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), NULL, @AccountID)
END CATCH;
END

/*

DECLARE  @contact dbo.ContactType 
DECLARE  @communication dbo.CommunicationType
DECLARE  @addresses dbo.AddresseType
DECLARE  @contactLeadSourceMap dbo.ContactLeadSourceMapType
insert into @contact values(1888933,'Avinash','Reddy',NULL,1058469,null,NULL,4218,NULL,NULL,NULL,NULL,NULL,1,NULL,5911,0,'2017-01-20 14:18:39.100',0,NULL,0,'A34D1014-D0E3-4BEC-97BE-D16B963543E8',6889,'2016-09-06 13:22:43.840',6889,null,NULL,NULL,NULL,25,3,0,1376)
--INSERT INTO @communication VALUES(null,NULL,'https://facebook.com/siva',NULL,NULL,null,NULL,NULL,NULL,NULL,NULL)
--insert into @addresses values(0,4662,'sd','sdfwe','Texas','US-TX','US',090709,1)
--insert into @addresses values(0,550,'sd','sdfwe','Texas','US-TX','US',090709,1)
INSERT INTO @contactLeadSourceMap VALUES(0,1888933,4658,1,'2017-01-19 07:42:30.227')



EXECUTE [dbo].[SaveContact] @contact,@contactLeadSourceMap

*/

