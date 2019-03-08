CREATE  PROC [dbo].[processUniqueRecipients]
AS
BEGIN
	/*
	-- LOGIC --
	1. Contacts Created in Last 1 year (exclude contacts created through imports) 
	union
	2. Contacts Clicked/Opened any campiagn in last 1 year
	*/
	DECLARE @accountIds TABLE(AccountID INT)
	INSERT INTO @accountIds SELECT AccountID FROM Accounts

	DECLARE @activeDateTime datetime = DATEADD(YEAR,-1,GETUTCDATE())
	DECLARE @ImportedContacts dbo.Contact_List
	DECLARE @currentAccountId INT

	SELECT TOP 1 @currentAccountId = AccountID FROM @accountIds

	WHILE @currentAccountId is not null
	BEGIN
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
       
		   ;WITH updateData AS (
				SELECT DISTINCT C.AccountID, C.ContactID, CE.EmailStatus, C.IsDeleted FROM dbo.contacts c (NOLOCK)
					INNER JOIN dbo.contactemails ce (NOLOCK)  on ce.contactid = c.contactid AND CE.AccountID = C.AccountID  AND ce.isprimary = 1
					INNER JOIN Campaigns CA (NOLOCK) ON C.AccountID = CA.AccountID
					INNER JOIN vCampaignRecipients CR (NOLOCK) ON  CR.CampaignID = CA.CampaignID and C.ContactID = CR.ContactID AND CR.AccountID = CA.AccountID
					INNER JOIN vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CR.AccountID = CS.AccountID AND cs.ActivityDate >  @activeDateTime 
				WHERE C.AccountID = @currentAccountId
				UNION
				SELECT DISTINCT C.AccountID, C.ContactID, CE.EmailStatus, C.IsDeleted FROM Contacts c (NOLOCK)
					INNER JOIN Contacts_Audit ca (NOLOCK) ON c.ContactID = ca.ContactID and ca.LastUpdatedOn >  @activeDateTime     AND CA.AuditAction = 'I' AND C.AccountId = CA.AccountId
					INNER JOIN dbo.contactemails ce (NOLOCK)  on ce.contactid = c.contactid    AND ce.isprimary = 1 AND C.AccountId = CE.AccountId
				WHERE CA.AccountID = @currentAccountId AND C.FirstContactSource != 2
		   )

		   MERGE ActiveContacts ac
		   USING (SELECT UDI.* FROM updateData UDI
		   LEFT JOIN @ImportedContacts IC ON UDI.ContactID = IC.ContactID 
		   WHERE IC.ContactID IS NULL) ud ON ac.AccountID = ud.AccountID and ac.ContactID = ud.ContactID
		   WHEN MATCHED THEN UPDATE SET ac.EmailStatus = ud.EmailStatus, ac.IsDeleted = ud.IsDeleted
		   WHEN NOT MATCHED BY TARGET THEN INSERT (AccountID, ContactID, EmailStatus, IsDeleted) VALUES (ud.AccountID, ud.ContactID, ud.EmailStatus, ud.IsDeleted);
		   	   
		   PRINT @currentAccountId

		   DELETE FROM @accountIds WHERE AccountID = @currentAccountId;
		   SET @currentAccountId = null;
		   SELECT TOP 1 @currentAccountId = AccountID FROM @accountIds;
	END

END


--EXEC  [dbo].[processUniqueRecipients]
GO

