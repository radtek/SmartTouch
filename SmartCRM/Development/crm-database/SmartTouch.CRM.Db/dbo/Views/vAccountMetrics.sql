create view [dbo].[vAccountMetrics]
as
with u
as
(
	select AccountID, Count(1) UsersCount from Users U where U.Status = 1 and U.IsDeleted = 0 group by AccountID
), c
as
(
	select AccountID, Count(1) ContactsCount from Contacts C (nolock) where C.IsDeleted = 0 group by AccountID
), ca
as
(
	select AccountID, Count(1) SentCampaingns from Campaigns C (nolock) where C.CampaignStatusID = 105 and C.IsLinkedToWorkflows = 0 and C.IsDeleted =0 group by AccountID
), tc
as
(
	select AccountID, Count(1) TotalCampaingns from Campaigns C (nolock) where C.IsLinkedToWorkflows = 0 and C.IsDeleted =0 group by AccountID
)
, auc
as
(
	select AccountID, Count(1) AutomationCampaingns from Campaigns C (nolock) where C.IsLinkedToWorkflows = 1 and C.IsDeleted =0 group by AccountID
)
, wf
as
(
	select AccountID
	, sum(case when Status = 401 then 1 else 0 end) ActiveWorkflows 
	, sum(case when Status = 402 then 1 else 0 end) DraftWorkflows 
	, sum(case when Status = 403 then 1 else 0 end) PausedWorkflows 
	, sum(case when Status = 404 then 1 else 0 end) InActiveWorkflows 
	, Count(1) TotalWorkflows
	from Workflows C (nolock) where C.IsDeleted =0 group by AccountID
),fo
as
(
	select C.AccountID, count(distinct C.FormId) FormsCount, SUM(case when FS.FormSubmissionID is not null then 1 else 0 end ) as SubmissionsCount from Forms C (nolock) 
	left join FormSubmissions FS on C.FormID = FS.FormID
	where C.IsDeleted =0 group by AccountID
), fsub
as
(
	select F.FormID, F.AccountID, SUM(case when FS.FormSubmissionID is not null then 1 else 0 end ) as SubmissionsCount from Forms F (nolock)
	left join FormSubmissions FS (nolock) on F.FormID = FS.FormID
	group by F.FormID, F.AccountID
), fSubCount
as
(
	select AccountID, Count(1) ZeroSubmissions from  fsub where SubmissionsCount < 1 group by AccountID
)
select A.AccountID, AccountName, DomainURL, case when A.Status = 10 then 'Active' when A.Status = 3 then 'Paused' else 'InActive' end as AccountStatus, UsersCount
,c.ContactsCount, ca.SentCampaingns, tc.TotalCampaingns, auc.AutomationCampaingns
, wf.ActiveWorkflows,wf.DraftWorkflows,wf.PausedWorkflows,wf.InActiveWorkflows, wf.TotalWorkflows
, fo.FormsCount
, isnull(fSubCount.ZeroSubmissions,0) as ZeroSubmissions
, case when wap.AccountId is null then 'No' else 'Yes' end as IsWebAnalyticsExists
, case when sm.AccountId is null then 'No' else 'Yes' end as FullContact
from Accounts A (nolock)
join u ON A.AccountID = U.AccountID
join c ON A.AccountID = C.AccountID
join ca ON A.AccountID = Ca.AccountID
join tc ON A.AccountID = tc.AccountID
join auc ON A.AccountID = auc.AccountID
join wf ON A.AccountID = wf.AccountID
join fo ON A.AccountID = fo.AccountID
left join fSubCount ON A.AccountID = fSubCount.AccountID
left join WebAnalyticsProviders wap on A.AccountID = wap.AccountID and wap.StatusID = 1
left join SubscriptionModuleMap sm on A.AccountID = sm.AccountID and sm.moduleid = 36
where AccountName not like '%test%' and A.[Status] in (1,3,4)
--group by  A.AccountID, AccountName, DomainURL, A.Status
--order by A.Status, AccountName


