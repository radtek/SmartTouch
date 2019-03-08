
CREATE PROCEDURE [dbo].[GetUniqueRecpients]
       @Tags dbo.Contact_List READONLY, 
       @SDefinitions dbo.SavedSearchContacts READONLY,
       @AccountId INT,
       @OwnerId INT,
       @RoleId INT,
	   @IsDataSharingOn BIT
AS
BEGIN
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

       DECLARE @Counts TABLE
       (             
              EntityType tinyint,
              EntityId INT,
              ContactId INT,
              Primary Key NONCLUSTERED (EntityId, ContactId) WITH (IGNORE_DUP_KEY = ON),
              UNIQUE CLUSTERED (EntityType, EntityId, ContactId)
       )

       DECLARE @Active DATETIME = DATEADD(dd,-120,GETUTCDATE())


       DECLARE @EntityRecipients TABLE
       (             
              EntityType tinyint,
              EntityId INT,
              Total INT
       )

       INSERT INTO @Counts
       SELECT 0, CTM.TagID, CTM.ContactId FROM ContactTagMap CTM (NOLOCK)
       INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CTM.ContactID AND C.AccountID = @AccountId
       INNER JOIN @Tags T ON T.ContactID = CTM.TagID
       INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID=@AccountId
       WHERE C.IsDeleted = 0 AND CE.EmailStatus IN (50,51,52) AND CE.IsPrimary = 1 AND CE.IsDeleted=0 AND CTM.AccountID = @AccountId
	   AND ((@IsDataSharingOn =1 AND COALESCE(C.OwnerID,0) = COALESCE(NULLIF(@OwnerId,0), C.OwnerID,0)) 
			OR @IsDataSharingOn = @IsDataSharingOn)

       
	    INSERT INTO @Counts
		SELECT 1, SS.SearchDefinitionID, SS.ContactID
		FROM @SDefinitions SS
		INNER JOIN SearchDefinitions (NOLOCK) SSC ON SSC.SearchDefinitionID=SS.SearchDefinitionID 
		INNER JOIN Contacts (NOLOCK) C ON C.ContactID = SS.ContactID AND C.AccountID= @AccountId
		INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = @AccountId
		WHERE CE.EmailStatus IN (50,51,52) AND CE.IsPrimary = 1 AND CE.IsDeleted=0;

       -- Entity - 0 Tags Counts, 1 SD Counts
       INSERT INTO @EntityRecipients
       SELECT EntityType, EntityId, COUNT(1) FROM @Counts
       GROUP BY EntityId, EntityType

       -- Entity - 2 TagsAll Counts,  3 SDAll Counts
       INSERT INTO @EntityRecipients
       SELECT 2 EntityType,0, COUNT(DISTINCT ContactId) FROM @Counts WHERE EntityType = 0
       GROUP BY EntityType
	   UNION
	   SELECT 3 EntityType,0, COUNT(DISTINCT ContactId) FROM @Counts WHERE EntityType = 1
       GROUP BY EntityType

       

       --Entity TagsAllActive - 4 , SerchDefinitionsAllActive - 5
       INSERT INTO @EntityRecipients
       SELECT 4,0, COUNT(DISTINCT AC.ContactID) FROM ActiveContacts AC (NOLOCK)
       INNER JOIN @Counts Cnts ON Cnts.ContactId = AC.ContactID
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 0
       UNION
       SELECT 5,0, COUNT(DISTINCT AC.ContactID) FROM ActiveContacts AC (NOLOCK)
       INNER JOIN @Counts Cnts ON Cnts.ContactId = AC.ContactID
       WHERE AC.IsDeleted = 0 and Cnts.EntityType = 1

	   -- Entity 6 - TagAllSDActive, 7 - TagActiveSDAll
	   INSERT INTO @EntityRecipients
	   SELECT 6, 0,  SUM(Distinct Total)  FROM @EntityRecipients
	   WHERE EntityType IN (2,5)
	   UNION
	   SELECT 7, 0,  SUM(Distinct Total)  FROM @EntityRecipients
	   WHERE EntityType IN (3,4)

	   -- Entity - TotalActiveUnique - 8
       INSERT INTO @EntityRecipients
       SELECT 8, 0,  SUM(Distinct Total)  FROM @EntityRecipients 
	   WHERE EntityType IN (4,5)


	   -- Entity - TotalUnique - 9
       INSERT INTO @EntityRecipients
       SELECT 9, 0, COUNT(Distinct ContactId) FROM @Counts

       SELECT * FROM @EntityRecipients

END
/*

DECLARE @Tags dbo.Contact_List
DECLARE @SS dbo.SavedSearchContacts

insert into @tags 
select tagid from campaigncontacttagmap (nolock) where campaignid = 9629

EXECUTE [dbo].[GetUniqueRecpients] @Tags,@SS, 4218, 6889,0,1

*/
GO


