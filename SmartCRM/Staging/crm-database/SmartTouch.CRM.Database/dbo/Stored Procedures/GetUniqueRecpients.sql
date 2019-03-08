CREATE PROCEDURE [dbo].[GetUniqueRecpients]
       @Tags dbo.Contact_List READONLY, 
       @SDefinitions dbo.SavedSearchContacts READONLY,
       @AccountId INT,
       @OwnerId INT,
       @RoleId INT,
	   @IsDataSharingOn BIT
AS
BEGIN
		SET DEADLOCK_PRIORITY HIGH

		DECLARE @OwnerIds int = @OwnerId,@IsDataSharingOns bit = @IsDataSharingOn
		DECLARE  @SDefinition TABLE (	[GroupID] [uniqueidentifier] NULL DEFAULT (newid()),
		[SearchDefinitionID] [int] NOT NULL,
		[ContactID] [int] NOT NULL )

		INSERT INTO  @SDefinition ([SearchDefinitionID],[ContactID])
		SELECT  [SearchDefinitionID],[ContactID] FROM  @SDefinitions

		SELECT * INTO #Tags   from  @Tags




	
		DECLARE @tagLogs varchar(max) =''
		DECLARE @sslogs varchar(Max) =''
		DECLARE @Owner Nvarchar(Max) =''
		DECLARE @Role varchar(Max) =''
		DECLARE @IsDataSharing varchar(Max) =''
		Declare @AccountIDS INT 
		declare @ssIds table (id int)
		insert into @ssIds
		select distinct SearchDefinitionid from @SDefinition

		select @tagLogs = coalesce(@tagLogs +',','') + convert(varchar(max),contactid) from #Tags
		select  @sslogs = coalesce(@sslogs +',','') + convert(varchar(max),Id) from @ssIds
		select  @Owner = CAST(@OwnerIds AS Nvarchar(MAX))
		select  @Role = CAST(@RoleId AS Nvarchar(MAX))
		select  @IsDataSharing = CAST(@IsDataSharingOns AS Nvarchar(MAX))
		SELECT  @AccountIDS = @AccountId

		--DECLARE @ResultID INT
		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'Tags:' + @tagLogs +',SS:' +@sslogs+',OwnerId:' +@Owner+',RoleId:' +@Role+',IsDataSharing:' +@IsDataSharing)
		--SET @ResultID = scope_identity()



       /*
       Entity
       Tag - 0
       SearchDefinition - 1
       TagAll - 2
       SearchDefinitionAll - 3
       TagsAllActive - 4
       SerchDefinitionsAllActive - 5
	   TagAllSDActive - 6
	   TagActiveSDAll - 7
	   TotalActiveRecipientsCount - 8
       TotalUnique - 9
       */

       create TABLE #Counts 
       (             
              EntityType tinyint,
              EntityId INT,
              ContactId INT,
              Primary Key NONCLUSTERED (EntityId, ContactId) WITH (IGNORE_DUP_KEY = ON),
              UNIQUE CLUSTERED (EntityType, EntityId, ContactId)
       )

       DECLARE @Active DATETIME = DATEADD(dd,-120,GETUTCDATE())

	   CREATE  TABLE  #EntityRecipients
	   (             
              EntityType tinyint,
              EntityId INT,
              Total INT
       )

       --DECLARE @EntityRecipients TABLE
       --(             
       --       EntityType tinyint,
       --       EntityId INT,
       --       Total INT
       --)

	

		--create table #ContactEmails
		--(
		--	ContactId INT
		--	Primary Key NONCLUSTERED ( ContactId) WITH (IGNORE_DUP_KEY = ON),
		--	UNIQUE CLUSTERED (ContactId)
		--)
		
		declare @l1 int

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting email addresses, result id : ' + convert(varchar(10), @ResultID))
		--set @l1 = scope_identity()

		DECLARE @ContactEmails dbo.Contact_List
		INSERT INTO @ContactEmails (ContactId)
		SELECT  C.ContactID   FROM  ContactEmails (NOLOCK) CE
		INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CE.ContactID AND C.AccountID = CE.AccountID
		WHERE  C.AccountID=@AccountIDS AND CE.IsDeleted = 0 AND C.IsDeleted = 0 AND (C.DoNotEmail IS NULL OR C.DoNotEmail=0) AND CE.IsPrimary = 1 AND CE.EmailStatus IN (50,51,52)
		AND ((@IsDataSharingOns =1 AND COALESCE(C.OwnerID,0) = COALESCE(NULLIF(@OwnerIds,0), C.OwnerID,0)) 
				OR @IsDataSharingOns = @IsDataSharingOns) 

		--UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l1

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting tag email addresses, result id : ' + convert(varchar(10), @ResultID))
		--declare @l2 int = scope_identity()

		;WITH tagsCte AS (
			SELECT 0 EntityType, CTM.TagID, CTM.ContactId FROM ContactTagMap CTM (NOLOCK)
		   INNER JOIN #Tags T ON T.ContactID = CTM.TagID
		   INNER JOIN @ContactEmails  CE ON CE.ContactID = CTM.ContactID
		   WHERE CTM.AccountID = @AccountIDS
		)
       INSERT INTO #Counts
	   SELECT * FROM tagsCte
       

		--UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l2

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting ss email addresses, result id : ' + convert(varchar(10), @ResultID))
		--declare @l3 int = scope_identity()
       
	    INSERT INTO #Counts
		SELECT 1, SS.SearchDefinitionID, SS.ContactID
		FROM @SDefinition SS
		INNER JOIN SearchDefinitions (NOLOCK) SSC ON SSC.SearchDefinitionID=SS.SearchDefinitionID 
		INNER JOIN @ContactEmails  CE ON CE.ContactID = SS.ContactID 
		
		
		--UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l3

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting entity counts, result id : ' + convert(varchar(10), @ResultID))
		--declare @l4 int = scope_identity()

       -- Entity - 0 Tags Counts, 1 SD Counts
       INSERT INTO #EntityRecipients
       SELECT EntityType, EntityId, COUNT(1) FROM #Counts
       GROUP BY EntityId, EntityType

       -- Entity - 2 TagsAll Counts,  3 SDAll Counts
       INSERT INTO #EntityRecipients
       SELECT 2 EntityType,0, COUNT(DISTINCT ContactId) FROM #Counts WHERE EntityType = 0
       GROUP BY EntityType
	   UNION
	   SELECT 3 EntityType,0, COUNT(DISTINCT ContactId) FROM #Counts WHERE EntityType = 1
       GROUP BY EntityType

  --     UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l4

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting entity 4, 5 counts, result id : ' + convert(varchar(10), @ResultID))
		--declare @l5 int = scope_identity()

       --Entity TagsAllActive - 4 , SerchDefinitionsAllActive - 5
       INSERT INTO #EntityRecipients
       SELECT 4,0, COUNT(DISTINCT AC.ContactID) FROM ActiveContacts AC (NOLOCK)
       INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 0
       UNION
       SELECT 5,0, COUNT(DISTINCT AC.ContactID) FROM ActiveContacts AC (NOLOCK)
       INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 1

	 --   UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l5

		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting entity type 6, 7 counts, result id : ' + convert(varchar(10), @ResultID))
		--declare @l6 int = scope_identity()

	 -- Entity 6 - TagAllSDActive
	   ;with TagAllSDActive as(
	   SELECT DISTINCT ContactId FROM #Counts WHERE EntityType = 0
	   union
	   SELECT DISTINCT AC.ContactID FROM ActiveContacts AC (NOLOCK)
       INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 1
	   )
	   INSERT INTO #EntityRecipients
	   select 6,0 ,count(Distinct ContactID) from TagAllSDActive

	   --7 - TagActiveSDAll
	   ;with TagActiveSDAll as(
	   SELECT DISTINCT ContactId FROM #Counts WHERE EntityType = 1
	   union
	   SELECT DISTINCT AC.ContactID FROM ActiveContacts AC (NOLOCK)
       INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 0
	   )
	   INSERT INTO #EntityRecipients
	   select 7,0 ,count(Distinct ContactID) from TagActiveSDAll

	 --  UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l6

	 --  INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'inserting entity type 8, 9 counts, result id : ' + convert(varchar(10), @ResultID))
		--declare @l7 int = scope_identity()

	   -- Entity - TotalActiveUnique - 8
      ;with TotalActive
	   as (
		   SELECT Distinct AC.ContactID FROM ActiveContacts AC (NOLOCK)
		   INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
		   WHERE AC.IsDeleted = 0 and Cnts.EntityType = 0
		   UNION
		   SELECT Distinct AC.ContactID FROM ActiveContacts AC (NOLOCK)
		   INNER JOIN #Counts Cnts ON Cnts.ContactId = AC.ContactID AND AC.AccountID = @AccountIDS
		   WHERE AC.IsDeleted = 0 and Cnts.EntityType = 1
	   )
       INSERT INTO #EntityRecipients
	   select 8, 0,  count(Distinct ContactID) from  TotalActive


	   -- Entity - TotalUnique - 9
       INSERT INTO #EntityRecipients
       SELECT 9, 0, COUNT(Distinct ContactId) FROM #Counts

	 --  UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l7

       SELECT * FROM #EntityRecipients

	 --   INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('GetUniqueRecpients', @AccountIDS, 'deleting entity counts, email counts, result id : ' + convert(varchar(10), @ResultID))
		--declare @l8 int = scope_identity()

	   drop table #Counts
	   --drop table #ContactEmails
	   drop table #EntityRecipients

	 --  UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @l8

	 --  UPDATE	StoreProcExecutionResults
		--SET		EndTime = GETDATE(),
		--		TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
		--		Status = 'C'
		--WHERE	ResultID = @ResultID

END
/*

DECLARE @Tags dbo.Contact_List
DECLARE @SS dbo.SavedSearchContacts

insert into @tags 
select 8212

--insert into @SS
--select GroupID,SearchDefinitionID,ContactID from temp_SavedSearch_102017

EXECUTE [dbo].[GetUniqueRecpients] @Tags,@SS, 398, 233,1705,0

*/

