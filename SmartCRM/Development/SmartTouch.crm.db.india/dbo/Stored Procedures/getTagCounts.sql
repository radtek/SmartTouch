

CREATE proc [dbo].[getTagCounts]
@tagIds dbo.Contact_List readonly --for Tag Ids
,@SSContacts dbo.Contact_List readonly--for Contact Ids
,@ownerId INT = NULL
as
begin

DECLARE @tagsActive datetime = DATEADD(dd,-120,GETUTCDATE())
declare @campaignStat Table
(
	TagId INT,
	ContactId INT
);

declare @result Table
(
	TagId INT,
	Total INT,
	Active INT
);

INSERT INTO @result (TagId, Total, Active)
SELECT T.TagID, SUM(CASE WHEN C.ContactID IS NOT NULL THEN 1 ELSE 0 END) as TotalCount
, 0 as ActiveCount  FROM Tags T (NOLOCK)
JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
LEFT JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0
GROUP BY T.TagID

insert into @result values(-1,0,0) --Tags Active
insert into @result values(-2,0,0) --SS Active
insert into @result values(-3,0,0) --Total

--Tags wise
;with campaignStat (TagId, ContactId)
as
(
SELECT DISTINCT CR.ContactID, 'Active' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0 and (@ownerId IS NULL OR C.OwnerId = @ownerId)
	join vCampaignRecipients CR (NOLOCK) ON CR.ContactId = C.ContactID AND CR.AccountID = C.AccountID
	join vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CS.AccountID = CR.AccountID and CS.ActivityDate < @tagsActive
	WHERE CS.CampaignRecipientID IS NOT NULL
)
update @result set Active = (select count(1) from campaignStat) where TagId = -1;

;with Normal as
(
	SELECT DISTINCT C.ContactID, 'Normal' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	LEFT JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0 and (@ownerId IS NULL OR C.OwnerId = @ownerId)
	WHERE C.ContactID IS NOT NULL
)
update @result set Total = (select count(1) from Normal) where TagId = -1;

--Saved Searches
;with campaignStat (TagId, ContactId)
as
(
SELECT DISTINCT CR.ContactID, 'Active' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0
	join vCampaignRecipients CR (NOLOCK) ON CR.ContactId = C.ContactID AND CR.AccountID = C.AccountID
	join vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CS.AccountID = CR.AccountID and CS.ActivityDate < @tagsActive
	--join @SSContacts SS ON SS.ContactID = C.ContactID
	WHERE CS.CampaignRecipientID IS NOT NULL
)
update @result set Active = (select count(1) from campaignStat) where TagId = -2;

;with Normal as
(
	SELECT DISTINCT C.ContactID, 'Normal' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	LEFT JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0	
	--join @SSContacts SS ON SS.ContactID = C.ContactID
	WHERE C.ContactID IS NOT NULL
)
update @result set Total = (select count(1) from Normal) where TagId = -2;

--Combined Total
;with campaignStat (TagId, ContactId)
as
(
SELECT DISTINCT CR.ContactID, 'Active' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0 and (@ownerId IS NULL OR C.OwnerId = @ownerId)
	join vCampaignRecipients CR (NOLOCK) ON CR.ContactId = C.ContactID AND CR.AccountID = C.AccountID
	join vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CS.AccountID = CR.AccountID and CS.ActivityDate < @tagsActive
	WHERE CS.CampaignRecipientID IS NOT NULL
	UNION
SELECT DISTINCT CR.ContactID, 'Active' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0
	join vCampaignRecipients CR (NOLOCK) ON CR.ContactId = C.ContactID AND CR.AccountID = C.AccountID
	join vCampaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CS.AccountID = CR.AccountID and CS.ActivityDate < @tagsActive
	join @SSContacts SS ON SS.ContactID = C.ContactID
	WHERE CS.CampaignRecipientID IS NOT NULL
)
update @result set Active = (select count(1) from campaignStat) where TagId = -3;

;with Normal as
(
	SELECT DISTINCT C.ContactID, 'Normal' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	LEFT JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0 and (@ownerId IS NULL OR C.OwnerId = @ownerId)
	WHERE C.ContactID IS NOT NULL
	UNION
	SELECT DISTINCT C.ContactID, 'Normal' as IsActive FROM Tags T (NOLOCK)
	JOIN ContactTagMap CTM (NOLOCK) ON T.TagID = CTM.TagID AND T.TagID IN (SELECT ContactID FROM @tagIds)
	LEFT JOIN Contacts C (NOLOCK) ON CTM.ContactID = C.ContactID and c.IsDeleted = 0	
	join @SSContacts SS ON SS.ContactID = C.ContactID
	WHERE C.ContactID IS NOT NULL
)
update @result set Total = (select count(1) from Normal) where TagId = -3;


SELECT * FROM @result

end

/*
declare @tags as dbo.Contact_List 

declare @ss as dbo.Contact_List 

insert into @tags
select 9971
union select 9986

exec [getTagCounts] @tags,@ss,0
*/


