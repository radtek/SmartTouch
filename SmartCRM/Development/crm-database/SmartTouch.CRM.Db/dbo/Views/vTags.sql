




CREATE VIEW [dbo].[vTags] with schemabinding
as
select T.TagID
,TagName
,Description
,AccountID
,CreatedBy
,Count
,IsDeleted from [dbo].Tags  T  WITH (NOLOCK)
--left JOIN dbo.vTagCounts TC ON T.TagID = TC.TagID and TC.AccountID = T.AccountID
--group by T.TagID
--,TagName
--,Description
--,AccountID
--,CreatedBy, IsDeleted

GO






