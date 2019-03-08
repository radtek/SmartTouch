CREATE view [dbo].[vTagsLeadScore]
as
select T.TagID, T.AccountID, Count(LS.LeadScoreRuleID) as Count  from vtags T (nolock)
left join [dbo].[LeadScoreRules] LS on CAST(T.TagId as varchar(10)) =  LS.ConditionValue and LS.[ConditionID] IN (6,7) and T.AccountID = LS.AccountID
group by T.TagID, T.AccountID