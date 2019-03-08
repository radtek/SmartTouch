create view [dbo].[vTagCounts] with schemabinding
as
SELECT TagId, COUNT(TagID) Counts FROM [dbo].[ActionTagMap]  Group By TagId
	UNION ALL
SELECT CTM.TagId, COUNT(CTM.TagID) Counts FROM [dbo].[ContactTagMap] CTM  
	INNER JOIN [dbo].[Contacts] C   ON CTM.ContactID = C.ContactID AND C.IsDeleted = 0
		 Group By CTM.TagId
	UNION ALL
SELECT TagId, COUNT(TagID) Counts FROM [dbo].[NoteTagMap]  Group by TagID
	UNION ALL
SELECT TagId, COUNT(CMTM.TagID) Counts FROM [dbo].[CampaignTagMap] CMTM 
	INNER JOIN [dbo].[Campaigns] C  ON CMTM.CampaignID = C.CampaignID AND C.IsDeleted = 0
		group by TagID
	UNION ALL
SELECT FT.TagID, COUNT(FT.TagID) Counts FROM [dbo].[FormTags] FT  
	INNER JOIN [dbo].[Forms] F  ON FT.[FormID] = F.[FormID] AND F.IsDeleted = 0
		group by FT.TagID 
	UNION ALL
SELECT OTM.TagID, COUNT(OTM.TagID) Counts FROM [dbo].[OpportunityTagMap] OTM 
	INNER JOIN [dbo].[Opportunities] O  ON OTM.OpportunityID = O.OpportunityID AND O.IsDeleted = 0
		group by OTM.TagID
	UNION ALL
SELECT SearchDefinitionTagID as TagID, COUNT(SearchDefinitionID) as Counts FROM [dbo].[SearchDefinitionTagMap]  Group by SearchDefinitionTagID


