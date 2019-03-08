CREATE   PROCEDURE [dbo].[GetRecentPopularTags] 
(
	@AccountID INT
)
AS
BEGIN


		SELECT TOP 10 T.TagId Id
		, CASE WHEN COUNT(LeadScoreRuleID) >0 THEN T.TagName + ' *' ELSE T.TagName END AS TagName
		, T.COUNT
		, CASE WHEN COUNT(LeadScoreRuleID) > 0 THEN 1 ELSE 0 END AS LeadScoreTag
		, 'Popular' AS TagType FROM Tag_Counts T(NOLOCK)
		LEFT JOIN [dbo].[LeadScoreRules] (NOLOCK) LS ON
		CAST(T.TagId AS VARCHAR(10)) =  LS.ConditionValue AND LS.[ConditionID] IN (6,7) AND T.AccountID = LS.AccountID
		INNER JOIN Tags TT ON TT.TagID = T.TagId
		WHERE T.AccountId = @AccountID AND TT.IsDeleted ! =1
		GROUP BY T.TagId, T.TagName, T.COUNT
		ORDER BY T.COUNT DESC

 
		SELECT   TOP 10 T.TagId Id
		, CASE WHEN COUNT(LeadScoreRuleID) >0 THEN T.TagName + ' *' ELSE T.TagName END AS TagName
		, 1 AS COUNT --T.Count
		, CASE WHEN COUNT(LeadScoreRuleID) > 0 THEN 1 ELSE 0 END AS LeadScoreTag
		, 'Recent' AS TagType FROM vtags T(NOLOCK)
		LEFT JOIN [dbo].[LeadScoreRules] (NOLOCK) LS ON
		CAST(T.TagId AS VARCHAR(10)) =  LS.ConditionValue AND LS.[ConditionID] IN (6,7) AND T.AccountID = LS.AccountID
		WHERE T.AccountId = @AccountID AND  T.IsDeleted != 1
		GROUP BY T.TagId, T.TagName--, T.Count
		ORDER BY T.TagId DESC

END