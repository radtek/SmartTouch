




CREATE VIEW [dbo].[vTags] with schemabinding
as
select T.TagID
,TagName
,Description
,AccountID
,CreatedBy
,SUM(ISNULL(TC.Counts, 0)) as  Count
,IsDeleted from [dbo].Tags  T 
left JOIN dbo.vTagCounts TC ON T.TagID = TC.TagID
group by T.TagID
,TagName
,Description
,AccountID
,CreatedBy, IsDeleted





