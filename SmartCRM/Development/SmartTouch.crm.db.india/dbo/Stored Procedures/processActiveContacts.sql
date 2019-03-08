CREATE PROC [dbo].[processActiveContacts]
as
begin
declare @accountIds table(AccountID INT)
insert into @accountIds SELECT AccountID from Accounts where IsDeleted=0

declare @currentAccountId INT

select top 1 @currentAccountId = AccountID from @accountIds

DECLARE @TagsActive DATETIME;

SET @TagsActive = DATEADD(YEAR,-1,GETUTCDATE());

while @currentAccountId is not null
begin
	;with myData as
	(
		SELECT CS.ContactID FROM vCampaignStatistics CS (NOLOCK)
		INNER JOIN vCampaignRecipients CR (NOLOCK) ON CS.CampaignRecipientID = CS.CampaignRecipientID
		INNER JOIN Campaigns C (NOLOCK) ON C.CampaignID = CR.CampaignID and c.AccountID = @currentAccountId
		WHERE CS.ActivityDate > @TagsActive
	)
	, ContactsCTM AS
	(	
		SELECT CA.ContactID FROM Contacts_Audit CA (NOLOCK) WHERE CA.AccountID = @currentAccountId AND CA.AuditAction = 'I' AND CA.LastUpdatedOn > @TagsActive	
	),contactsList as
	(
	select * from myData
	UNION
	select * from ContactsCTM
	),updateData as(
	select CE.AccountID, cl.ContactID, CE.EmailStatus, C.IsDeleted from ContactEmails CE (nolock)
	join contactsList cl on cl.ContactID = CE.ContactID
	join Contacts c (nolock) on c.ContactID = cl.ContactID
	where ce.IsPrimary = 1 and ce.IsDeleted = 0
	)
	MERGE ActiveContacts ac
	USING updateData ud
	ON ac.AccountID = ud.AccountID and ac.ContactID = ud.ContactID
	WHEN MATCHED THEN UPDATE SET ac.EmailStatus = ud.EmailStatus, ac.IsDeleted = ud.IsDeleted
	WHEN NOT MATCHED BY TARGET THEN INSERT (AccountID, ContactID, EmailStatus, IsDeleted) VALUES (ud.AccountID, ud.ContactID, ud.EmailStatus, ud.IsDeleted);

	delete from @accountIds where AccountID = @currentAccountId;
	set @currentAccountId = null;
	select top 1 @currentAccountId = AccountID from @accountIds;
end

end

