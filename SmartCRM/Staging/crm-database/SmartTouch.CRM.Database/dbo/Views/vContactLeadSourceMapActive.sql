CREATE view [dbo].[vContactLeadSourceMapActive]
as
select Max(ContactLeadSourceMapID) as ContactLeadSourceMapID, max(LeadSouceID) as LeadSouceID, ContactID, IsPrimaryLeadSource from ContactLeadSourceMap 
where IsPrimaryLeadSource = 1 
group by ContactID, IsPrimaryLeadSource