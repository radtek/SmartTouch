CREATE    PROCEDURE [dbo].[Deleting_Accounts_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY
   

DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

	BEGIN
				DELETE	AS1 
				FROM	dbo.AccountSettings  AS AS1  (NOLOCK)
				WHERE	AS1.AccountID = @Accountid 
	
			END
			select @@rowcount  AccountSettings1count
			PRINT  ' records deleted from  AccountSettings1'



				BEGIN 
				DELETE	AE1
				FROM	dbo.AccountEmails  AS AE1   (NOLOCK)
				WHERE	AE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT AccountEmailsCOUNT
			END
			PRINT  ' records deleted from  AccountEmails1'
				BEGIN
				DELETE	TPC1
				FROM	dbo.ThirdPartyClients  AS TPC1  (NOLOCK)
				WHERE	TPC1.AccountID = @Accountid 
	         select @@rowcount   ThirdPartyClients1COUNT
			END
		PRINT ' records deleted from  ThirdPartyClients1'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[ClientRefreshTokens] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) s.ID
							FROM dbo.ClientRefreshTokens AS S (NOLOCK)
							  INNER JOIN [dbo].[ThirdPartyClients] AS U ON S.ThirdPartyClientID=U.ID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.ID = D.ID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ClientRefreshTokens
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ClientRefreshTokens'
	
			BEGIN
				DELETE	ADAP1
				FROM	dbo.AccountDataAccessPermissions  AS ADAP1   (NOLOCK)
				WHERE	ADAP1.AccountID = @Accountid
	          SELECT @@ROWCOUNT AccountDataAccessPermissions1COUNT
			END
			PRINT ' records deleted from  AccountDataAccessPermissions1'
	
			BEGIN
				DELETE	CFT1
				FROM	dbo.CustomFieldTabs  AS CFT1  (NOLOCK)
				WHERE	CFT1.AccountID = @Accountid
	           SELECT @@ROWCOUNT   CustomFieldTabs1
			END
			PRINT  ' records deleted from  CustomFieldTabs1'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ImportDataSettings AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportDataSettingID
							FROM dbo.ImportDataSettings AS CEA (NOLOCK)
							  INNER JOIN dbo.Accounts AS CE ON CEA.AccountID=CE.AccountID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.ImportDataSettingID = CEA.ImportDataSettingID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ImportDataSettings
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportDataSettings'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.MarketingMessageAccountMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) MarketingMessageAccountMapID
							FROM dbo.MarketingMessageAccountMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Accounts AS CE ON CEA.AccountID=CE.AccountID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.MarketingMessageAccountMapID = CEA.MarketingMessageAccountMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT MarketingMessageAccountMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from MarketingMessageAccountMap_accounts'

              SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	A
				FROM	[dbo].[Accounts] AS A INNER JOIN(
							SELECT TOP (@RecordPerBatch) cea.AccountID
							FROM dbo.Accounts AS CEA (NOLOCK)
							  INNER JOIN [dbo].[Images] AS CE ON CEA.[LogoImageID]=CE.[ImageID] 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.AccountID = A.AccountID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactEmailAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Accounts_Images'




			BEGIN
				DELETE	AAM1
				FROM	dbo.AccountAddressMap  AS AAM1   (NOLOCK)
				WHERE	AAM1.AccountID = @Accountid 
	
			END
			select @@rowcount  AccountSettings1count
			PRINT ' records deleted from  AccountAddressMap1'



			BEGIN
				DELETE	ADAP1
				FROM	dbo.AccountDataAccessPermissions  AS ADAP1   (NOLOCK)
				WHERE	ADAP1.AccountID = @Accountid
	          SELECT @@ROWCOUNT AccountDataAccessPermissions1COUNT
			END
			PRINT ' records deleted from  AccountDataAccessPermissions1'

		BEGIN
				DELETE	ct
				FROM	dbo.CampaignTemplates  AS ct   (NOLOCK)
				WHERE	ct.AccountID = @Accountid
	          SELECT @@ROWCOUNT CampaignTemplates
			END
			PRINT ' records deleted from  CampaignTemplates'




			BEGIN
				DELETE	A1
				FROM	dbo.Actions  AS A1  (NOLOCK)
				WHERE	A1.AccountID = @Accountid 
	         SELECT @@ROWCOUNT Actions1COUNT
			END
			PRINT  ' records deleted from  Actions1'


			BEGIN
				DELETE	ACR1
				FROM	dbo.AccountCustomReports  AS ACR1   (NOLOCK)
				WHERE	ACR1.AccountID = @Accountid 
	          SELECT @@ROWCOUNT  AccountCustomReports1
			END
			PRINT  ' records deleted from  AccountCustomReports1'


			BEGIN
				DELETE	AA1
				FROM	dbo.Actions_Audit  AS AA1   (NOLOCK)
				WHERE	AA1.AccountID = @Accountid 
	          SELECT @@ROWCOUNT Actions_Audit1
			END
			PRINT  ' records deleted from  Actions_Audit1'


			BEGIN
				DELETE	AC1
				FROM	dbo.ActiveContacts  AS AC1   (NOLOCK)
				WHERE	AC1.AccountID = @Accountid 
	         SELECT @@ROWCOUNT ActiveContacts1
			END
			PRINT  ' records deleted from  ActiveContacts1'


			BEGIN
				DELETE	BO1
				FROM	dbo.BulkOperations  AS BO1   (NOLOCK)
				WHERE	BO1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT  BulkOperations1
			END
			PRINT ' records deleted from  BulkOperations1'


			BEGIN
				DELETE	CC1
				FROM	dbo.ClassicCampaigns  AS CC1 (NOLOCK) 
				WHERE	CC1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT  ClassicCampaigns1
			END
			PRINT  ' records deleted from  ClassicCampaigns1'


			BEGIN
				DELETE	C1
				FROM	dbo.Communities  AS C1  (NOLOCK)
				WHERE	C1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT  Communities1
			END
			PRINT  ' records deleted from  Communities1'


			BEGIN
				DELETE	CE1
				FROM	dbo.ContactEmails AS CE1  (NOLOCK)
				WHERE	CE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT ContactEmails1
			END
			PRINT  ' records deleted from  ContactEmails1'


			BEGIN
				DELETE	CEA1
				FROM	dbo.ContactEmails_Audit  AS CEA1   (NOLOCK)
				WHERE	CEA1.AccountID = @Accountid 
	
	         SELECT @@ROWCOUNT ContactEmails_Audit1COUNT
			END
			PRINT ' records deleted from  ContactEmails_Audit1'


			BEGIN
				DELETE	CPN1
				FROM	dbo.ContactPhoneNumbers  AS CPN1  (NOLOCK)
				WHERE	CPN1.AccountID = @Accountid 
	          SELECT @@ROWCOUNT ContactPhoneNumbers1COUNT
			END
			PRINT ' records deleted from  ContactPhoneNumbers1'


			BEGIN
				DELETE	C1
				FROM	dbo.Contacts  AS C1  (NOLOCK)
				WHERE	C1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT   Contacts1COUNT
			END
			PRINT  ' records deleted from  Contacts1'


			BEGIN
				DELETE	CA1
				FROM	dbo.Contacts_Audit  AS CA1 (NOLOCK) 
				WHERE	CA1.AccountID = @Accountid 
	           SELECT @@ROWCOUNT Contacts_Audit1
			END
			PRINT  ' records deleted from  Contacts_Audit1'


			BEGIN
				DELETE	CFT1
				FROM	dbo.CustomFieldTabs  AS CFT1  (NOLOCK)
				WHERE	CFT1.AccountID = @Accountid
	           SELECT @@ROWCOUNT   CustomFieldTabs1
			END
			PRINT  ' records deleted from  CustomFieldTabs1'

			BEGIN
				DELETE	DV1
				FROM	dbo.DropdownValues  AS DV1  (NOLOCK)
				WHERE	DV1.AccountID = @Accountid
				 SELECT @@ROWCOUNT DropdownValues1COUNT
			END
			PRINT ' records deleted from  DropdownValues1'


			BEGIN
				DELETE	E1
				FROM	dbo.Emails  AS E1  (NOLOCK)
				WHERE	E1.AccountID = @Accountid 
	         SELECT @@ROWCOUNT  Emails1COUNT
			END
			PRINT ' records deleted from  Emails1'



			BEGIN
				DELETE	F1
				FROM	dbo.Fields  AS F1  (NOLOCK)
				WHERE	F1.AccountID = @Accountid 
	           SELECT @@ROWCOUNT   Fields1COUNT
			END
			PRINT  ' records deleted from  Fields1'


			BEGIN
				DELETE	F1
				FROM	dbo.Forms  AS F1   (NOLOCK)
				WHERE	F1.AccountID = @Accountid 
	         SELECT @@ROWCOUNT   Forms1COUNT
			END
			PRINT ' records deleted from  Forms1'


			BEGIN
				DELETE	IS1
				FROM	dbo.ImportDataSettings  AS IS1 (NOLOCK) 
				WHERE	IS1.AccountID = @Accountid 
	           SELECT @@ROWCOUNT  ImportDataSettings1COUNT
			END
			PRINT ' records deleted from  ImportDataSettings1'


			BEGIN
				DELETE	LAAAM1
				FROM	dbo.LeadAdapterAndAccountMap  AS LAAAM1  (NOLOCK)
				WHERE	LAAAM1.AccountID = @Accountid 
	          SELECT @@ROWCOUNT LeadAdapterAndAccountMap1COUNT

			END
			PRINT  ' records deleted from  LeadAdapterAndAccountMap1'


			BEGIN
				DELETE	LSM1
				FROM	dbo.LeadScoreMessages  AS LSM1 (NOLOCK) 
				WHERE	LSM1.AccountID = @Accountid 
	           SELECT @@ROWCOUNT  LeadScoreMessages1COUNT
			END
			PRINT  ' records deleted from  LeadScoreMessages1'


			BEGIN
				DELETE	LSR1
				FROM	dbo.LeadScoreRules  AS LSR1  (NOLOCK)
				WHERE	LSR1.AccountID = @Accountid 
             select @@rowcount  LeadScoreRules1COUNT
			END
			PRINT  ' records deleted from  LeadScoreRules1'


			BEGIN
				DELETE	OSG1
				FROM	dbo.OpportunityStageGroups  AS OSG1  (NOLOCK)
				WHERE	OSG1.AccountID = @Accountid 
	          select @@rowcount  OpportunityStageGroups1COUNT
			END
			PRINT  ' records deleted from  OpportunityStageGroups1'


			BEGIN
				DELETE	R1
				FROM	dbo.Reports  AS R1  (NOLOCK)
				WHERE	R1.AccountID = @Accountid 
	           select @@rowcount  Reports1COUNT
			END 
			PRINT  ' records deleted from  Reports1'


			BEGIN
				DELETE	R1
				FROM	dbo.Roles  AS R1  (NOLOCK)
				WHERE	R1.AccountID = @Accountid 
	          select @@rowcount  Roles1COUNT
			END
			PRINT ' records deleted from  Roles1'


			BEGIN
				DELETE	SD1
				FROM	dbo.SearchDefinitions  AS SD1  (NOLOCK)
				WHERE	SD1.AccountID = @Accountid 
	           select @@rowcount SearchDefinitions1COUNT
			END
			PRINT  ' records deleted from  SearchDefinitions1'


			BEGIN
				DELETE	SP1
				FROM	dbo.ServiceProviders  AS SP1  (NOLOCK)
				WHERE	SP1.AccountID = @Accountid 
	          select @@rowcount  ServiceProviders1COUNT
			END
			PRINT  ' records deleted from  ServiceProviders1'


			BEGIN
				DELETE	SPER1
				FROM	dbo.StoreProcExecutionResults  AS SPER1  (NOLOCK)
				WHERE	SPER1.AccountID = @Accountid 
	         select @@rowcount  StoreProcExecutionResults1COUNT
			END
			PRINT  ' records deleted from  StoreProcExecutionResults1'


			BEGIN
				DELETE	SPPL1
				FROM	dbo.StoreProcParamsList  AS SPPL1 (NOLOCK) 
				WHERE	SPPL1.AccountID = @Accountid
	         select @@rowcount  StoreProcParamsList1COUNT
			END
			PRINT  ' records deleted from  StoreProcParamsList1'


			BEGIN
				DELETE	SMM1
				FROM	dbo.SubscriptionModuleMap  AS SMM1  (NOLOCK)
				WHERE	SMM1.AccountID = @Accountid 
	        select @@rowcount  SubscriptionModuleMap1COUNT
			END
	PRINT  ' records deleted from  SubscriptionModuleMap1'


		


			BEGIN
				DELETE	T1
				FROM	dbo.Tours  AS T1  (NOLOCK)
				WHERE	T1.AccountID = @Accountid 
	         select @@rowcount Tours1COUNT
			END
		PRINT  ' records deleted from  Tours1'


			BEGIN
				DELETE	U1
				FROM	dbo.Users  AS U1  (NOLOCK)
				WHERE	U1.AccountID = @Accountid 
	         select @@rowcount Users1COUNT
			END
			PRINT  ' records deleted from  Users1'


			BEGIN
				DELETE	WAP1
				FROM	dbo.WebAnalyticsProviders  AS WAP1 (NOLOCK) 
				WHERE	WAP1.AccountID = @Accountid 
	          select @@rowcount WebAnalyticsProvidersCOUNT
			END
		PRINT ' records deleted from  WebAnalyticsProviders1'


			BEGIN
				DELETE	WVUN1
				FROM	dbo.WebVisitUserNotificationMap  AS WVUN1 (NOLOCK) 
				WHERE	WVUN1.AccountID = @Accountid  
	        select @@rowcount WebVisitUserNotificationMap1COUNT
			END
			PRINT ' records deleted from WebVisitUserNotificationMap '

			BEGIN
				DELETE	s
				FROM	dbo.SpamIPAddresses  AS s (NOLOCK) 
				WHERE	s.AccountID = @Accountid  
	        select @@rowcount SpamIPAddressesCOUNT
			END
			PRINT ' records deleted from SpamIPAddresses ' 

			BEGIN
				DELETE	s
				FROM	dbo.SpamValidators  AS s (NOLOCK) 
				WHERE	s.AccountID = @Accountid  
	        select @@rowcount SpamValidatorsCOUNT
			END
			PRINT ' records deleted from SpamValidators '

			BEGIN
				DELETE	W1
				FROM	dbo.Workflows  AS W1  (NOLOCK)
				WHERE	W1.AccountID = @Accountid  
	         select @@rowcount Workflows1COUNT
			END
		PRINT  ' records deleted from  Workflows1'


			BEGIN
				DELETE	I1
				FROM	dbo.Images  AS I1  (NOLOCK)
				WHERE	I1.AccountID = @Accountid  
	         select @@rowcount Images1COUNT
			END
		PRINT  ' records deleted from  Images1'


			BEGIN
				DELETE	ICD1
				FROM	dbo.ImportContactData  AS ICD1  (NOLOCK)
				WHERE	ICD1.AccountID = @Accountid  
	           select @@rowcount  ImportContactData1COUNT
			END
		PRINT  ' records deleted from  ImportContactData1'


			BEGIN
				DELETE	O1
				FROM	dbo.Opportunities  AS O1  (NOLOCK)
				WHERE	O1.AccountID = @Accountid  
	         select @@rowcount Opportunities1COUNT
			END
			PRINT  ' records deleted from  Opportunities1'


			BEGIN
				DELETE	OA1
				FROM	dbo.Opportunities_Audit  AS OA1  (NOLOCK)
				WHERE	OA1.AccountID = @Accountid  
	         select @@rowcount Opportunities_Audit1COUNT
			END
		PRINT  ' records deleted from  Opportunities_Audit1'


			BEGIN
				DELETE	N1
				FROM	dbo.Notes  AS N1  (NOLOCK)
				WHERE	N1.AccountID = @Accountid  
	        select @@rowcount  Notes1COUNT
			END
			PRINT ' records deleted from  Notes1'


			BEGIN
				DELETE	NA1
				FROM	dbo.Notes_Audit  AS NA1  (NOLOCK)
				WHERE	NA1.AccountID = @Accountid  
	          select @@rowcount   Notes_Audit1COUNT
			END
			PRINT ' records deleted from  Notes_Audit1'


			BEGIN
				DELETE	T1
				FROM	dbo.Tags  AS T1  (NOLOCK)
				WHERE	T1.AccountID = @Accountid  
	        select @@rowcount  Tags1
			END
		PRINT  ' records deleted from  Tags1'


			BEGIN
				DELETE	TA1
				FROM	dbo.Tags_Audit  AS TA1  (NOLOCK)
				WHERE	TA1.AccountID = @Accountid  
	          select @@rowcount  Tags_Audit1
			END
			PRINT ' records deleted from  Tags_Audit1'

				BEGIN
				DELETE	m
				FROM	dbo.MarketingMessageAccountMap  AS m  (NOLOCK)
				WHERE	m.AccountID = @Accountid  
	          select @@rowcount  MarketingMessageAccountMap
			END
			PRINT ' records deleted from  MarketingMessageAccountMap'

					BEGIN
				DELETE	u
				FROM	dbo.UserSettings  AS u  (NOLOCK)
				WHERE	u.AccountID = @Accountid  
	          select @@rowcount  UserSettings
			END
			PRINT ' records deleted from  UserSettings'



				BEGIN
				DELETE	s
				FROM	dbo.SubmittedFormData  AS s  (NOLOCK)
				WHERE	s.AccountID = @Accountid  
	          select @@rowcount  SubmittedFormData
			END
			PRINT ' records deleted from  SubmittedFormData'


			BEGIN
				DELETE	TA1
				FROM	dbo.Tours_Audit  AS TA1  (NOLOCK)
				WHERE	TA1.AccountID = @Accountid  
	         select @@rowcount  Tours_Audit1COUNT
			END
			PRINT  ' records deleted from  Tours_Audit1'


			BEGIN
				DELETE	UAL1
				FROM	dbo.UserActivityLogs  AS UAL1  (NOLOCK)
				WHERE	UAL1.AccountID = @Accountid 
	          select @@rowcount UserActivityLogsCOUNT
			END
			PRINT  ' records deleted from  UserActivityLogs1'

			BEGIN
				DELETE	c
				FROM	dbo.Campaigns  AS c  (NOLOCK)
				WHERE	c.AccountID = @Accountid 
	          select @@rowcount CampaignsCOUNT
			END
			PRINT  ' records deleted from  Campaigns'

			BEGIN
				DELETE	w
				FROM	Workflow.TrackMessages  AS w  (NOLOCK)
				WHERE	w.AccountID = @Accountid 
	          select @@rowcount WorkflowTrackMessagesCOUNT
			END
			PRINT  ' records deleted from  Workflow.TrackMessages'

			BEGIN
				DELETE	w
				FROM	Roles  AS w  (NOLOCK)
				WHERE	w.AccountID = @Accountid 
	          select @@rowcount Roles
			END
			PRINT  ' records deleted from  Roles'

		BEGIN
				DELETE	w
				FROM	Notes  AS w  (NOLOCK)
				WHERE	w.AccountID = @Accountid 
	          select @@rowcount Notes
			END
			PRINT  ' records deleted from  Notes'
					BEGIN
				DELETE	w
				FROM	Campaigns  AS w  (NOLOCK)
				WHERE	w.AccountID = @Accountid 
	          select @@rowcount Campaigns
			END
			PRINT  ' records deleted from  Campaigns'

			--DELETING RECORD FROM ACCOUNT TABLE--	
           	

		BEGIN
			DELETE	A1
				FROM	dbo.Accounts  AS A1  (NOLOCK)
     			WHERE	A1.AccountID = @Accountid 
				select @@rowcount AccountsCOUNT
		END
	PRINT  ' records deleted from  Accounts1'

--successfull execution query-- 
SELECT 'DEL-001' ResultCode 
SELECT @@ROWCOUNT TotalCount

	END TRY

BEGIN CATCH

		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_Accounts_sp]
		@AccountID	= 94

*/

/*
	SELECT COUNT(*) FROM FormTags WITH (NOLOCK) WHERE FormID IN (SELECT FormID FROM Forms WHERE Accountid = 22)
	SELECT COUNT(*) FROM UserActivityLogs WITH (NOLOCK) where Accountid = 19
	*/

