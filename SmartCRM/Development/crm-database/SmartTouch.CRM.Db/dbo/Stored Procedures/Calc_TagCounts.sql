CREATE PROCEDURE [dbo].[Calc_TagCounts]
AS
BEGIN

	TRUNCATE TABLE  Tag_Counts



	CREATE TABLE #tagCounts (tagid INT, tagCount INT)
		
		INSERT INTO #tagCounts (tagid, tagCount)
		SELECT AC.TagID,COUNT(AC.TagID) Counts FROM [dbo].[ActionTagMap] AC (NOLOCK)
		INNER JOIN Tags T (NOLOCK) ON t.TagID = AC.TagID
		GROUP BY AC.TagID

		UNION ALL

		SELECT CTM.TagID,COUNT(CTM.TagID) Counts FROM [dbo].[ContactTagMap] CTM  (NOLOCK)
		INNER JOIN [dbo].[Contacts] C  (NOLOCK) ON CTM.ContactID = C.ContactID  AND CTM.AccountID = C.AccountID
		INNER JOIN Tags T (NOLOCK) ON t.TagID = CTM.TagID AND C.AccountID = T.AccountID
		WHERE C.IsDeleted = 0 
		GROUP BY CTM.TagID

		UNION ALL

		SELECT NT.TagID,COUNT(NT.TagID) Counts FROM [dbo].[NoteTagMap]  NT (NOLOCK)
		INNER JOIN Tags T  (NOLOCK) ON t.TagID = NT.TagID
		GROUP BY NT.TagID


		UNION ALL

		SELECT CMTM.TagID,COUNT(CMTM.TagID) Counts FROM [dbo].[CampaignTagMap] CMTM (NOLOCK)
		INNER JOIN [dbo].[Campaigns] C (NOLOCK) ON CMTM.CampaignID = C.CampaignID AND C.IsDeleted = 0
		INNER JOIN Tags T (NOLOCK) ON t.TagID = CMTM.TagID
		GROUP BY CMTM.TagID
			
				
		UNION ALL

		SELECT FT.TagID,COUNT(FT.TagID) Counts FROM [dbo].[FormTags] FT (NOLOCK) 
		INNER JOIN [dbo].[Forms] F (NOLOCK) ON FT.[FormID] = F.[FormID] AND F.IsDeleted = 0
		INNER JOIN Tags T (NOLOCK) ON t.TagID = FT.TagID
		GROUP BY FT.TagID

		UNION ALL

		SELECT OTM.TagID,COUNT(OTM.TagID) Counts FROM [dbo].[OpportunityTagMap] OTM (NOLOCK)
		INNER JOIN [dbo].[Opportunities] O (NOLOCK) ON OTM.OpportunityID = O.OpportunityID AND O.IsDeleted = 0
		INNER JOIN Tags T (NOLOCK)  ON t.TagID = OTM.TagID
		GROUP BY OTM.TagID

		UNION ALL

		SELECT SD.SearchDefinitionTagID,COUNT(SearchDefinitionID) FROM [dbo].[SearchDefinitionTagMap]  SD (NOLOCK) 
		INNER JOIN Tags T (NOLOCK)  ON t.TagID = SD.SearchDefinitionTagID
		GROUP BY SD.SearchDefinitionTagID


		INSERT INTO Tag_Counts ([TagId],[TagName],[Count],[AccountID])
		SELECT TT.tagid,T.TagName,SUM(TT.tagCount),T.AccountID  FROM #tagCounts tt 
		INNER JOIN Tags T (NOLOCK) ON T.TagID = TT.TagID 
		GROUP BY TT.tagid,T.TagName,T.AccountID
END



