
CREATE FUNCTION [dbo].[TagCounts] 
	(
		@TagID int
	)
	RETURNS		int 
AS
BEGIN
	DECLARE @TagCount int
	
	SELECT @TagCount = SUM(Counts) 
	FROM
		(	SELECT COUNT(TagID) Counts FROM [dbo].[ActionTagMap] WHERE TagID = @TagID
				UNION ALL
			SELECT COUNT(CTM.TagID) Counts FROM [dbo].[ContactTagMap] CTM  (NOLOCK)
				INNER JOIN [dbo].[Contacts] C  (NOLOCK) ON CTM.ContactID = C.ContactID AND C.IsDeleted = 0 AND C.AccountID = CTM.AccountID
					WHERE CTM.TagID = @TagID
				UNION ALL
			SELECT COUNT(TagID) Counts FROM [dbo].[NoteTagMap] (NOLOCK) WHERE TagID = @TagID
				UNION ALL
			SELECT COUNT(CMTM.TagID) Counts FROM [dbo].[CampaignTagMap] CMTM (NOLOCK)
				INNER JOIN [dbo].[Campaigns] C (NOLOCK) ON CMTM.CampaignID = C.CampaignID AND C.IsDeleted = 0
					WHERE CMTM.TagID = @TagID
				UNION ALL
			SELECT COUNT(FT.TagID) Counts FROM [dbo].[FormTags] FT (NOLOCK) 
				INNER JOIN [dbo].[Forms] F (NOLOCK) ON FT.[FormID] = F.[FormID] AND F.IsDeleted = 0
					WHERE FT.TagID = @TagID
				UNION ALL
			SELECT COUNT(OTM.TagID) Counts FROM [dbo].[OpportunityTagMap] OTM (NOLOCK)
				INNER JOIN [dbo].[Opportunities] O (NOLOCK) ON OTM.OpportunityID = O.OpportunityID AND O.IsDeleted = 0
					WHERE OTM.TagID = @TagID
				UNION ALL
			SELECT COUNT(SearchDefinitionID) FROM [dbo].[SearchDefinitionTagMap] (NOLOCK) WHERE [SearchDefinitionTagID] = @TagID
		) TempTags
	 
	RETURN @TagCount
END

