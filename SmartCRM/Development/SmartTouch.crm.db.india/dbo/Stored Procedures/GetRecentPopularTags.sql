create  PROCEDURE [dbo].[GetRecentPopularTags] 
(
	@AccountID INT
)
AS
BEGIN
	select Top 10 T.TagId Id
, case when Count(LeadScoreRuleID) >0 then T.TagName + ' *' else T.TagName end as TagName
, T.Count
, case when Count(LeadScoreRuleID) > 0 then 1 else 0 end as LeadScoreTag
, 'Popular' as TagType from Tag_Counts T(nolock)
left join [dbo].[LeadScoreRules] (NOLOCK) LS on
CAST(T.TagId as varchar(10)) =  LS.ConditionValue and LS.[ConditionID] IN (6,7) and T.AccountID = LS.AccountID
where T.AccountId = @AccountID
group by T.TagId, T.TagName, T.Count
order by t.Count desc

 
select   Top 10 T.TagId Id
, case when Count(LeadScoreRuleID) >0 then T.TagName + ' *' else T.TagName end as TagName
, 1 as Count --T.Count
, case when Count(LeadScoreRuleID) > 0 then 1 else 0 end as LeadScoreTag
, 'Recent' as TagType from vtags T(nolock)
left join [dbo].[LeadScoreRules] (NOLOCK) LS on
CAST(T.TagId as varchar(10)) =  LS.ConditionValue and LS.[ConditionID] IN (6,7) and T.AccountID = LS.AccountID
where T.AccountId = @AccountID
group by T.TagId, T.TagName, T.Count
order by T.TagId desc

END