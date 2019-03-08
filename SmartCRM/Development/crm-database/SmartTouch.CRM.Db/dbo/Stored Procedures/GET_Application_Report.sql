
CREATE PROCEDURE  [dbo].[GET_Application_Report]
AS
BEGIN
	SET NOCOUNT ON

		select a.AccountID,a.AccountName, COUNT(CASE WHEN CampaignStatusID = 110 THEN CampaignStatusID END) Failed,
				COUNT(CASE WHEN CampaignStatusID = 104 THEN CampaignStatusID END) Sending
				from Campaigns(NOLOCK) c inner join Accounts(NOLOCK) a on c.AccountID=a.AccountID and c.IsDeleted=0 and DATEDIFF(MINUTE, c.LastUpdatedOn, GETUTCDATE()) > 60
				Group By a.AccountID,a.AccountName 
		

		select CONVERT(VARCHAR(20),MAX(fs.SubmittedOn)) Data, 'Form Submission' Module, a.AccountName AccountName, f.AccountID AccountID from forms(NOLOCK) f
		inner join FormSubmissions(NOLOCK) fs on f.FormID = fs.FormID
		inner join Accounts(NOLOCK) a on f.AccountID = a.AccountID
		group by f.AccountID,a.AccountName
		

		SELECT CONVERT(VARCHAR(20),MAX(SentOn)) Date, 'Email' Module, c.AccountID, acc.AccountName
		FROM ContactEmailAudit(NOLOCK) cea inner join
		ContactEmails(NOLOCK) c on cea.ContactEmailID = c.ContactEmailID
		inner join Accounts(NOLOCK) acc on c.AccountID = acc.AccountID
		Group By c.AccountID,acc.AccountName
		

		select CONVERT(VARCHAR(20),MAX(cr.SentOn)) Data, 'Campaign Sent' Module, a.AccountName AccountName, camp.AccountID AccountID from CampaignRecipients cr (NOLOCK)
		inner join Campaigns(NOLOCK) camp on cr.CampaignID = camp.CampaignID AND cr.AccountID = camp.AccountID
		inner join Accounts(NOLOCK) a on camp.AccountID = a.AccountID
		group by camp.AccountID,a.AccountName
		

		SELECT CONVERT(VARCHAR(20),MAX(la.EndDate)) Data,'Failed Import' Module,  acc.AccountName, lam.AccountID
		FROM LeadAdapterJobLogs(NOLOCK) la 
						inner join LeadAdapterAndAccountMap(NOLOCK) lam 
						on la.LeadAdapterAndAccountMapID = lam.LeadAdapterAndAccountMapID 
						inner join Accounts(NOLOCK) acc on lam.AccountID = acc.AccountID
						where lam.LeadAdapterTypeID = 11 AND la.LeadAdapterJobStatusID = 2
						Group By lam.AccountID, acc.AccountName
		

		SELECT CONVERT(VARCHAR(20),MAX(la.EndDate)) Data,'Succeeded Import' Module, acc.AccountName, lam.AccountID
		FROM LeadAdapterJobLogs(NOLOCK) la 
						inner join LeadAdapterAndAccountMap(NOLOCK) lam 
						on la.LeadAdapterAndAccountMapID = lam.LeadAdapterAndAccountMapID 
						inner join Accounts(NOLOCK) acc on lam.AccountID = acc.AccountID
						where lam.LeadAdapterTypeID = 11 AND la.LeadAdapterJobStatusID = 3
						Group By lam.AccountID, acc.AccountName
		
		select CONVERT(VARCHAR(20), MAX(la.StartDate)) StartDate, 'InProgress Import' Module,lam.AccountID, acc.AccountName
						 from LeadAdapterJobLogs(NOLOCK) la
						 inner join LeadAdapterAndAccountMap(NOLOCK) lam
						 on la.LeadAdapterAndAccountMapID = lam.LeadAdapterAndAccountMapID
						 inner join Accounts(NOLOCK) acc on lam.AccountID = acc.AccountID
						 where lam.LeadAdapterTypeID = 11 AND la.LeadAdapterJobStatusID = 4
						 Group By lam.AccountID, acc.AccountName

		SELECT CONVERT(VARCHAR(20),MAX(la.EndDate)) Data,'Failed Lead Adapter' Module, acc.AccountName, lam.AccountID
		FROM LeadAdapterJobLogs(NOLOCK) la 
						inner join LeadAdapterAndAccountMap(NOLOCK) lam 
						on la.LeadAdapterAndAccountMapID = lam.LeadAdapterAndAccountMapID 
						inner join Accounts(NOLOCK) acc on lam.AccountID = acc.AccountID
						where lam.LeadAdapterTypeID != 11 AND la.LeadAdapterJobStatusID = 2
						Group By lam.AccountID, acc.AccountName

		SELECT CONVERT(VARCHAR(20),MAX(la.EndDate)) Data,'succeeded Lead Adapter' Module, acc.AccountName, lam.AccountID
		FROM LeadAdapterJobLogs(NOLOCK) la 
						inner join LeadAdapterAndAccountMap(NOLOCK) lam 
						on la.LeadAdapterAndAccountMapID = lam.LeadAdapterAndAccountMapID 
						inner join Accounts(NOLOCK) acc on lam.AccountID = acc.AccountID
						where lam.LeadAdapterTypeID != 11 AND la.LeadAdapterJobStatusID = 3
						Group By lam.AccountID, acc.AccountName

		
		SELECT CONVERT(VARCHAR(20),MAX(AddedOn)) Data, 'Lead Score' Module, acc.AccountName, lr.AccountID
		FROM dbo.LeadScores(NOLOCK) ls
						inner join LeadScoreRules(NOLOCK) lr on ls.LeadScoreRuleID = lr.LeadScoreRuleID
						inner join Accounts(NOLOCK) acc on lr.AccountID = acc.AccountID
						Group By lr.AccountID, acc.AccountName

;with cte
as
(
       select distinct contactid, workflowid from contactworkflowaudit cwa group by contactid, workflowid
),
wat as(
       select wat.WorkflowTriggerID,
              coalesce(wat.campaignid,wat.formid, wat.lifecycledropdownvalueid,wat.tagid,wat.searchdefinitionid,wat.opportunitystageid,wat.leadadapterid) as entityid,
              case
                     when CampaignId > 0 then 18
                     when FormId > 0 then 3
                     when LifecycleDropdownValueID > 0 then 12
                     when tagid > 0 then 10
                     when SearchDefinitionID > 0 then 14
                     when opportunitystageid > 0 then 22
                     when leadadapterid > 0 then 9
              end as lct, wat.WorkflowID
       from workflowtriggers(NOLOCK) wat
 
),
wat1 as(
       select wat.WorkflowTriggerID,
              coalesce(wat.campaignid,wat.formid, wat.lifecycledropdownvalueid,wat.tagid,wat.searchdefinitionid,wat.opportunitystageid,wat.leadadapterid) as entityid,
              case
                     when CampaignId > 0 then 18
                     when FormId > 0 then 3
                     when LifecycleDropdownValueID > 0 then 12
                     when tagid > 0 then 10
                     when SearchDefinitionID > 0 then 14
                     when opportunitystageid > 0 then 22
                     when leadadapterid > 0 then 9
              end as lct, wat.WorkflowID
       from workflowtriggers(NOLOCK) wat )
 
select x.AccountID ,count(contactid) ContactsCount,  x.AccountName from (select distinct tm.*, a.AccountName from trackmessages(NOLOCK) tm
inner join wat wat on tm.entityid = wat.entityid and tm.LeadScoreConditionType = wat.lct
inner join workflows(NOLOCK) w on w.workflowid = wat.workflowid
inner join contacts(NOLOCK) c on c.contactid = tm.contactid
inner join Accounts(NOLOCK) a on w.AccountID = a.AccountID
where w.[Status] = 401
and tm.LeadScoreConditionType in (2,3,10,12,14,15,16,18,19,20,22)
and c.isdeleted = 0
--and tm.accountid not in (2,5)
and tm.createdon > GETUTCDATE()-1 and w.createdon > tm.createdon --and tm.createdon < '2015-09-02 00:00:00.000'
except
select distinct tm.*, a.AccountName from trackmessages(NOLOCK) tm
inner join wat1 wat on tm.entityid = wat.entityid and tm.LeadScoreConditionType = wat.lct
inner join workflows(NOLOCK) w on w.workflowid = wat.workflowid
inner join cte cwa on cwa.workflowid = w.workflowid and tm.contactid = cwa.contactid
inner join Accounts(NOLOCK) a on w.AccountID = a.AccountID
where w.[Status] = 401
and tm.LeadScoreConditionType in (2,3,10,12,14,15,16,18,19,20,22)
--and tm.accountid not in (2,5)
and tm.createdon > GETUTCDATE()-1 and w.createdon > tm.createdon --and tm.createdon < '2015-09-02 00:00:00.000'
) as x
group by x.accountid, x.AccountName 

	SET NOCOUNT OFF
END

/*
	
	EXEC GET_Application_Report

*/


GO


