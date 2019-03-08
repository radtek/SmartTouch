
CREATE   PROC [dbo].[processUniqueRecipients]
AS
BEGIN
	/*
	-- LOGIC --
	1. Contacts Created in Last 1 year (exclude contacts created through imports) 
	union
	2. Contacts Clicked/Opened any campiagn in last 1 year
	*/
	DECLARE @accountIds TABLE(AccountID INT)
	INSERT INTO @accountIds SELECT AccountID FROM Accounts WITH (NOLOCK) WHERE Isdeleted = 0  AND Status = 1 order by accountid

	DECLARE @activeDateTime datetime = DATEADD(YEAR,-1,GETUTCDATE())
	DECLARE @ImportedContacts dbo.Contact_List
	DECLARE @currentAccountId INT

	SELECT TOP 1 @currentAccountId = AccountID FROM @accountIds

	


	WHILE @currentAccountId is not null
	BEGIN
	      
		  DROP TABLE IF EXISTS #T

	      SELECT  * INTO #T  FROM ActiveContacts WHERE  AccountID = @currentAccountId


		  DELETE FROM @ImportedContacts

		   DECLARE @Count INT = 5000
		   WHILE @Count = 5000
		   BEGIN
				DELETE TOP(5000) ActiveContacts WHERE AccountID = @currentAccountId
				SET @Count = @@ROWCOUNT
		   END
		   SET @Count = (SELECT COUNT(1) FROM ACtiveContacts (NOLOCK) WHERE AccountID = @currentAccountId)
		   PRINT CONVERT(VARCHAR(10),@Count) + ' DELETED - ' + CONVERT(VARCHAR(10),@currentAccountId)

		  --get all imported contacts from this account
		  --INSERT INTO @ImportedContacts
		  --SELECT ContactId FROM Contacts (NOLOCK) WHERE AccountID = @currentAccountId AND FirstContactSource = 2 AND IsDeleted = 0

       DROP TABLE IF EXISTS #updateData


		   SELECT  * INTO #updateData from (
					SELECT DISTINCT C.AccountID, C.ContactID, CE.EmailStatus, C.IsDeleted FROM dbo.contacts c (NOLOCK)
					INNER JOIN dbo.contactemails ce (NOLOCK)  on ce.contactid = c.contactid    AND ce.isprimary = 1
					INNER JOIN Campaigns CA (NOLOCK) ON C.AccountID = CA.AccountID
					INNER JOIN vCampaignRecipients CR (NOLOCK) ON  CR.CampaignID = CA.CampaignID and C.ContactID = CR.ContactID
					INNER JOIN vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND cs.ActivityDate >  @activeDateTime    
				WHERE C.AccountID = @currentAccountId  AND C.IsDeleted = 0
				UNION
				SELECT DISTINCT C.AccountID, C.ContactID, CE.EmailStatus, C.IsDeleted FROM Contacts c (NOLOCK)
					INNER JOIN Contacts_Audit ca (NOLOCK) ON c.ContactID = ca.ContactID and ca.LastUpdatedOn >  @activeDateTime     AND CA.AuditAction = 'I'
					INNER JOIN dbo.contactemails ce (NOLOCK)  on ce.contactid = c.contactid    AND ce.isprimary = 1
				WHERE CA.AccountID = @currentAccountId AND (c.FirstContactSource != 2 OR C.FirstContactSource IS NULL)  AND C.IsDeleted = 0
				)
		      t

		 

		 ALTER TABLE #updateData ADD RowNumber INT IDENTITY(1,1)

		 declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 5000;
		select @rowCount = Count(1) from #updateData

		WHILE 1=1

		  BEGIN 

		       INSERT INTO ActiveContacts  (AccountID, ContactID, EmailStatus, IsDeleted) 
			   SELECT AccountID,ContactID, EmailStatus, IsDeleted
			   FROM  #updateData
               WHERE RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount)
			

				IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
				BEGIN
					BREAK
				END
	 
				set @counter = @counter + 1;
		  END

			

			
			

			/* INDEX */

			SELECT  * INTO #IndexData FROM (
			SELECT DISTINCT T.ContactID FROM  #updateData  T 
			LEFT JOIN  #T A  ON A.ContactID = T.ContactID
			WHERE  A.ContactID IS NULL and t.AccountID = @currentAccountId
			UNION 
			SELECT  DISTINCT A.ContactID FROM  #T A 
			LEFT JOIN  #updateData   T  ON A.ContactID = T.ContactID
			WHERE T.ContactID IS NULL and a.AccountID = @currentAccountId )T

			INSERT INTO  [dbo].[IndexData] (ReferenceID, EntityID, IndexType, CreatedOn, Status, IsPercolationNeeded)
			SELECT  NEWID(),ContactID,1,GETUTCDATE(),1,0 FROM  #IndexData

           DROP TABLE IF EXISTS #updateData,#IndexData--,#T



		   PRINT @currentAccountId

		   DELETE FROM @accountIds WHERE AccountID = @currentAccountId;
		   SET @currentAccountId = null;
		   SELECT TOP 1 @currentAccountId = AccountID FROM @accountIds;
	END

END


--EXEC  [dbo].[processUniqueRecipients]
