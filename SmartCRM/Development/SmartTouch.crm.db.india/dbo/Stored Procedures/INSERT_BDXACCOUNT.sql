 CREATE PROCEDURE [dbo].[INSERT_BDXACCOUNT]
	(
		@AccountInfo				AS dbo.AccountInfo	READONLY,
		@AddressInfo				AS dbo.AddressInfo	READONLY,
		@TwilioFriendlyName         Varchar(75)
	)
AS
BEGIN 
	
	DECLARE @AccountID				INT
	DECLARE @UserID					INT
	DECLARE @CommunicationID		INT 
	DECLARE @CustomFieldTabID		INT
	DECLARE @CustomFieldSectionID	INT
	DECLARE @AccountName			VARCHAR(75)
	DECLARE @HelpURL				VARCHAR(500)
	DECLARE @Recordcount			INT = 0
	DECLARE @Loopcounter			INT = 0
	DECLARE @SearchDefinitionID		SMALLINT,
			@SubscriptionID         INT	,
			@NewServiceProviderID   INT,
			@OldServiceProviderID   INT,
			@ProviderEmail			VARCHAR(256),
		    @DropdownValueid	VARCHAR(75),
			@HomePhoneId  INT= 0 ,@MobilePhoneId INT = 0,@WorkPhoneId INT = 0
	DECLARE @UniqueID				UNIQUEIDENTIFIER	
	DECLARE @CommunicationTypeID	TINYINT
	DECLARE @AddressIDs				TABLE (AddressId INT)
	DECLARE @ServiceProvidersInfo	TABLE (ServiceProviderID INT NOT NULL, CommunicationTypeID TINYINT NOT NULL,LoginToken UNIQUEIDENTIFIER NOT NULL,
														ProviderName NVARCHAR(50),EmailType TINYINT NOT NULL,IsDefault BIT NOT NULL, SenderPhoneNumber VARCHAR(20), ROWNUMBER INT, ImageDomainID tinyint )

	DECLARE @SearchDefinitionInfo	TABLE (SearchDefinitionID SMALLINT NOT NULL,SearchDefinitionName VARCHAR(100) NOT NULL,ElasticQuery VARCHAR(500) NULL,
														SearchPredicateTypeID SMALLINT NOT NULL, CustomPredicateScript VARCHAR(100) NULL, IsFavoriteSearch BIT NOT NULL,
														IsPreConfiguredSearch BIT NOT NULL, ROWNUMBER INT NOT NULL,MapID INT NOT NULL)


	SET NOCOUNT ON

		BEGIN TRY			
			    
				SELECT @SubscriptionID = SubscriptionID, @AccountName = AccountName, @UserID = CreatedBy,@HelpURL = HelpURL  FROM @AccountInfo			

				BEGIN 
				    /* Communication details insertion */
					INSERT INTO dbo.Communications (FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl)
					SELECT FacebookUrl,TwitterUrl,GooglePlusUrl,LinkedInUrl,BlogUrl,WebSiteUrl FROM @AccountInfo

					SET @CommunicationID = SCOPE_IDENTITY()
				END
								
				BEGIN 
					/* Account insertion */
					INSERT INTO dbo.Accounts (AccountName,FirstName,LastName,Company,PrimaryEmail,HomePhone,WorkPhone,MobilePhone,PrivacyPolicy,CommunicationID,
											  SubscriptionID,DateFormatID,CurrencyID,CountryID,TimeZone,[Status],IsDeleted,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,
											  DomainURL,GoogleDriveAPIKey,GoogleDriveClientID,OpportunityCustomers,DropboxAppKey,FacebookAPPID,FacebookAPPSecret,
											  TwitterAPIKey,TwitterAPISecret,StatusMessage,HelpURL, ShowTC, TC)
					SELECT AccountName, FirstName, LastName, Company, PrimaryEmail, HomePhone, WorkPhone, MobilePhone, PrivacyPolicy, @CommunicationID,
									   SubscriptionID, DateFormatID, CurrencyID, CountryID, TimeZone, [Status], 0, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn,
									   DomainURL, GoogleDriveAPIKey, GoogleDriveClientID, 1, DropboxAppKey, FacebookAPPID, FacebookAPPSecret,
									   TwitterAPIKey, TwitterAPISecret, StatusMessage, HelpURL, ShowTC, TC FROM @AccountInfo
							   									  
				SET @AccountID = SCOPE_IDENTITY()

				END

				BEGIN 
				    /* Address insertion Code */					
					INSERT  INTO dbo.Addresses(AddressLine1, AddressLine2, AddressTypeID, City, StateID, CountryID, ZipCode, IsDefault)					
						OUTPUT INSERTED.AddressId
						INTO @AddressIDs					
					SELECT AddressLine1, AddressLine2, AddressTypeID, City, StateID, CountryID, ZipCode, IsDefault FROM @AddressInfo

					INSERT INTO dbo.AccountAddressMap(AddressID,AccountID)
					SELECT AddressId, @AccountID
						FROM @AddressIDs

				END

				BEGIN 
				    /* Default Roles insertion */
					INSERT INTO dbo.Roles(RoleName, AccountID,SubscriptionID,IsDeleted)
					SELECT RoleName, @AccountID, @SubscriptionID, 0
						FROM dbo.Roles 
						  WHERE AccountID IS NULL AND SubscriptionID = @SubscriptionID

					/* MAP all default modules to newly created roles */
					INSERT INTO dbo.RoleModuleMap(RoleID,ModuleID)
					SELECT R1.RoleID, M.ModuleID
						FROM Roles R 
							INNER JOIN dbo.RoleModuleMap RMM ON RMM.RoleID = R.RoleID
							INNER JOIN dbo.Modules M ON M.ModuleID = RMM.ModuleID
							INNER JOIN dbo.Roles R1 ON R.RoleName = R1.RoleName AND R1.AccountID = @AccountID
						WHERE ISNULL(R.AccountID, 0) = 0 AND R.SubscriptionID = @SubscriptionID

				END 

				BEGIN 
				    /* Default Dropdownvalues insertion */					
					INSERT INTO dbo.DropdownValues (DropdownID,AccountID,DropdownValue,IsDefault,SortID,DropdownValueTypeID,IsActive, IsDeleted)
					SELECT dv.DropdownID, @AccountID,dv.DropdownValue, dv.IsDefault, dv.SortID, dv.DropdownValueTypeID, dv.IsActive, 0
						FROM dbo.DropdownValues dv
						INNER JOIN dbo.SubscriptionDefaultDropdownValueMap sdv on dv.DropdownValueID = sdv.DropdownValueID and sdv.SubscriptionID = @SubscriptionID							
						WHERE dv.AccountID IS NULL 

				END

				BEGIN
				  /* Defaults subscription modules insertion */
                  INSERT INTO dbo.SubscriptionModuleMap(SubscriptionID, ModuleID, AccountID,Limit)
				  SELECT SubscriptionID, ModuleID,@AccountID,Limit
                       FROM dbo.SubscriptionModuleMap
                       WHERE SubscriptionID = @SubscriptionID AND AccountID IS NULL
 
				END

				BEGIN 
				    /* Default Opportunity Stage group insertion */								
					INSERT INTO dbo.OpportunityStageGroups (AccountID,DropdownValueID,OpportunityGroupID)
					SELECT @AccountID,DropdownValueID,
						CASE WHEN DropdownValue = 'Interested' OR DropdownValue = 'Offer' THEN 1
							 WHEN DropdownValue = 'Contract' OR DropdownValue = 'Closed' THEN 2
							 WHEN DropdownValue = 'Lost' THEN 3
						END
					FROM dbo.DropdownValues WHERE AccountID = @AccountID AND DropdownID = 6

				END

				BEGIN 
				    /* Defaults reports insertion */									
					INSERT INTO dbo.Reports (AccountID,ReportName,ReportType)
                    SELECT @AccountID,ReportName,ReportType
						FROM dbo.Reports WHERE AccountID IS NULL
				END

				BEGIN 
				    /* Default Communication providers insertion */										
					INSERT INTO @ServiceProvidersInfo (ServiceProviderID,CommunicationTypeID,LoginToken,ProviderName,EmailType,IsDefault,SenderPhoneNumber,ROWNUMBER, ImageDomainID)				
					SELECT ServiceProviderID, CommunicationTypeID, LoginToken, ProviderName, EmailType, IsDefault, SenderPhoneNumber, ROW_NUMBER() OVER(ORDER BY ServiceProviderID) AS ROWNUMBER,ImageDomainID
						  FROM dbo.ServiceProviders WHERE AccountID = 1 AND CommunicationTypeID IN (1,2)
					
					SET @Recordcount = (SELECT COUNT(1) FROM @ServiceProvidersInfo)
					SET @Loopcounter = 1

					WHILE (@Loopcounter <= @Recordcount)
						BEGIN	
							
							SET @UniqueID = NEWID()	
																	
							INSERT INTO dbo.ServiceProviders(CommunicationTypeID,LoginToken,CreatedBy,CreatedDate,AccountID,ProviderName,EmailType,IsDefault, SenderPhoneNumber,ImageDomainID)						
							SELECT CommunicationTypeID, @UniqueID, @UserID, GETUTCDATE(), @AccountID, ProviderName, EmailType, IsDefault, SenderPhoneNumber,ImageDomainID FROM @ServiceProvidersInfo
									WHERE ROWNUMBER = @Loopcounter
							SET @NewServiceProviderID = SCOPE_IDENTITY()

							SELECT @OldServiceProviderID = ServiceProviderID from @ServiceProvidersInfo WHERE ROWNUMBER = @Loopcounter
							SELECT @ProviderEmail = Email from AccountEmails where ServiceProviderID = @OldServiceProviderID 
							IF (LEN(ISNULL(@ProviderEmail,'')) > 0)
								BEGIN
									INSERT INTO AccountEmails(Email, AccountID, IsPrimary, ServiceProviderID)
									SELECT @ProviderEmail, @AccountID, 0, @NewServiceProviderID
								END
							SET @ProviderEmail = ''

							SET @CommunicationTypeID = (SELECT CommunicationTypeID FROM @ServiceProvidersInfo WHERE ROWNUMBER = @Loopcounter)
							
							IF (@CommunicationTypeID = 1)
								BEGIN
									INSERT INTO EnterpriseCommunication.dbo.MailRegistration (Guid,Name,Host,APIKey,UserName,[Password],Port,IsSSLEnabled,MailProviderID,VMTA,SenderDomain)
									SELECT @UniqueID, Name, Host, APIKey, UserName, [Password], Port, IsSSLEnabled, MailProviderID, VMTA, SenderDomain
											FROM  EnterpriseCommunication.dbo.MailRegistration WHERE [Guid] = (SELECT LoginToken FROM @ServiceProvidersInfo WHERE ROWNUMBER = @Loopcounter)
								END
							ELSE 
								BEGIN
									INSERT INTO EnterpriseCommunication.dbo.TextRegistration([Guid],Name,[Address],APIKey,Token,UserName,[Password],TextProviderID)
									SELECT @UniqueID, @TwilioFriendlyName, [Address], APIKey, Token, UserName, [Password], TextProviderID
											FROM  EnterpriseCommunication.dbo.TextRegistration WHERE [Guid] = (SELECT LoginToken FROM @ServiceProvidersInfo WHERE ROWNUMBER = @Loopcounter)
								END
							
							SET @Loopcounter = @Loopcounter + 1
						END

				END

				BEGIN 
				    /* Default pre defined saved searches insertion */						
					INSERT INTO @SearchDefinitionInfo (SearchDefinitionID,SearchDefinitionName,ElasticQuery,SearchPredicateTypeID,CustomPredicateScript,IsFavoriteSearch,IsPreConfiguredSearch,MapId,ROWNUMBER)
			    	SELECT s.SearchDefinitionID, s.SearchDefinitionName, s.ElasticQuery, s.SearchPredicateTypeID, s.CustomPredicateScript, s.IsFavoriteSearch, s.IsPreConfiguredSearch,sds.SearchDefinitionSubscriptionMapID MapID,
							    ROW_NUMBER() OVER(ORDER BY s.SearchDefinitionID) AS ROWNUMBER
							    FROM dbo.SearchDefinitions s
								INNER JOIN dbo.SearchDefinitionSubscriptionMap sds on s.SearchDefinitionID = sds.SearchDefinitionID and sds.SubscriptionID = @SubscriptionID									
								WHERE s.AccountID IS NULL

					SET @Recordcount = (SELECT COUNT(1) FROM @SearchDefinitionInfo)
					SET @Loopcounter = 1

					WHILE (@Loopcounter <= @Recordcount)
						BEGIN
													
							INSERT INTO dbo.SearchDefinitions (SearchDefinitionName,ElasticQuery,SearchPredicateTypeID,CustomPredicateScript,CreatedBy,CreatedOn,
														AccountID,IsFavoriteSearch,IsPreConfiguredSearch,SelectAllSearch)						
							SELECT SearchDefinitionName, ElasticQuery, SearchPredicateTypeID, CustomPredicateScript, @UserID, GETUTCDATE(),
											@AccountID, IsFavoriteSearch, IsPreConfiguredSearch,0 FROM @SearchDefinitionInfo
											WHERE ROWNUMBER = @Loopcounter

							SET @SearchDefinitionID = SCOPE_IDENTITY()

							 SET @DropdownValueid = (CASE WHEN ((select count(*) from @SearchDefinitionInfo where MapID = 4) >= 1)  or ((select count(*) from @SearchDefinitionInfo where MapID = 6) >= 1) THEN
							  (select DropdownValueID from DropdownValues where AccountId = @AccountID and DropdownID = 3 and DropdownValueTypeID = 15) ELSE '' END) 

							 SET @HomePhoneId = (CASE WHEN (select count(*) from @SearchDefinitionInfo where MapID = 5) >= 1  THEN
							  (select DropdownValueID from DropdownValues where AccountId = @AccountID and DropdownID = 1 and DropdownValueTypeID = 11) ELSE NULL END) 

							 SET @MobilePhoneId = (CASE WHEN (select count(*) from @SearchDefinitionInfo where MapID = 5) >= 1  THEN
							  (select DropdownValueID from DropdownValues where AccountId = @AccountID and DropdownID = 1 and DropdownValueTypeID = 9) ELSE NULL END) 

							 SET @WorkPhoneId = (CASE WHEN (select count(*) from @SearchDefinitionInfo where MapID = 5) >= 1  THEN
							  (select DropdownValueID from DropdownValues where AccountId = @AccountID and DropdownID = 1 and DropdownValueTypeID = 10) ELSE NULL END) 

							INSERT INTO dbo.SearchFilters (FieldID,SearchQualifierTypeID,SearchText,SearchDefinitionID,DropdownValueID)
							SELECT (CASE WHEN (SF.FieldID = 5 or SF.FieldID = 4 or SF.FieldID = 6)  THEN  NULL ELSE SF.FieldID END) ,
							        SF.SearchQualifierTypeID,(CASE WHEN (SF.FieldID = 22) THEN @DropdownValueid ELSE SF.SearchText END), @SearchDefinitionID, 
							        (CASE WHEN (SF.FieldID = 5) THEN @HomePhoneId ELSE (CASE WHEN (SF.FieldID = 4) THEN @MobilePhoneId ELSE (CASE WHEN (SF.FieldID = 6) THEN @WorkPhoneId ELSE SF.DropdownValueID END) END) END)
									FROM dbo.SearchFilters SF
									WHERE SF.SearchDefinitionID = (SELECT SearchDefinitionID FROM @SearchDefinitionInfo WHERE ROWNUMBER = @Loopcounter)

							SET @Loopcounter = @Loopcounter + 1
						END

				END

				BEGIN 
				    /* Default custom field tab */									
					INSERT INTO dbo.CustomFieldTabs (Name, StatusID, SortID, AccountID, IsLeadAdapterTab)
						VALUES ('Default', 0, 1, @AccountID, 0)
					
					SET @CustomFieldTabID = SCOPE_IDENTITY()

					INSERT INTO dbo.CustomFieldSections (Name, StatusID, SortID, TabID)
						VALUES ('Default', 0, 1, @CustomFieldTabID)

					SET @CustomFieldSectionID = SCOPE_IDENTITY()

					INSERT INTO dbo.Fields (Title,FieldInputTypeID,AccountID,FieldCode,CustomFieldSectionID,SortID,StatusID,IsLeadAdapterField)
						VALUES ('Comments', 14, @AccountID, '', @CustomFieldSectionID, 1, 201, 0)											
				END

				BEGIN 
				    /* Notifications for STadmins */					
					INSERT INTO Notifications (EntityID,[Subject],Details,NotificationTime,[Status],UserID,ModuleID)
					SELECT @AccountID, @AccountName + ' account was created', @AccountName + ' account was successfully created',
								GETUTCDATE(), 1, UserID, 1
						FROM dbo.Users WHERE AccountID = 1 AND IsDeleted = 0 AND [Status] = 1 

				END

				SELECT @AccountID
			
		END TRY

		BEGIN CATCH 
			PRINT 'ERROR OCCURED IN CATCH BLOCK AND ERROR MESSAGE IS BELOW: '
    		PRINT ERROR_MESSAGE()
			PRINT 'ERROR LINE NUMBER IS: '
			PRINT ERROR_LINE()
			-- ROLL BACK THE COMPLETE TRANSACTION IF ANY EXCEPTION
			--IF @@TRANCOUNT > 0
			--	ROLLBACK

			INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, ReferenceID, ContactID)
				VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE(), NULL, @AccountID)

		END CATCH

	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[IMPORT_Contacts_Data]

*/












