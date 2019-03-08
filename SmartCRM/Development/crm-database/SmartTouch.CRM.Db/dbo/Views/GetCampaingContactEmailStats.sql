CREATE view [dbo].[GetCampaingContactEmailStats] WITH SCHEMABINDING
as

select c.AccountID, c.ContactID,cs.ActivityDate from dbo.contacts c 
inner join dbo.contactemails ce  on ce.contactid = c.contactid AND CE.AccountID = C.AccountID
inner JOIN dbo.CampaignRecipients cr  on cr.ContactID = c.ContactID AND cr.AccountID = C.AccountID
inner join dbo.CampaignStatistics cs  on cs.CampaignRecipientID = cr.CampaignRecipientID AND CS.AccountID = C.AccountID --and cs.ActivityDate >  Convert(dateTime,DateAdd(dd,-120,GETUTCDATE()) ,120) 
where c.isdeleted = 0  and ce.isprimary = 1 
and ce.EmailStatus in (50,51,52)
