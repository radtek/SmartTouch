
CREATE  PROCEDURE [dbo].[Update_Tags_Count_OLD]
AS
BEGIN


	CREATE TABLE #TagS (TagID int ,Counts int )

	INSERT INTO #TagS (TagID , Counts)
	SELECT DISTINCT TagID ,sum(Counts)
	FROM
	(
		SELECT AC.TagID,COUNT(AC.TagID) Counts FROM [dbo].[ActionTagMap] AC
		INNER JOIN Tags T on t.TagID = AC.TagID
		GROUP BY AC.TagID

		UNION ALL

		SELECT CTM.TagID,COUNT(CTM.TagID) Counts FROM [dbo].[ContactTagMap] CTM  (NOLOCK)
		INNER JOIN [dbo].[Contacts] C  (NOLOCK) ON CTM.ContactID = C.ContactID AND C.IsDeleted = 0
		INNER JOIN Tags T on t.TagID = CTM.TagID
		GROUP BY CTM.TagID

		UNION ALL

		SELECT NT.TagID,COUNT(NT.TagID) Counts FROM [dbo].[NoteTagMap]  NT
		INNER JOIN Tags T on t.TagID = NT.TagID
		GROUP BY NT.TagID


		UNION ALL

		SELECT CMTM.TagID,COUNT(CMTM.TagID) Counts FROM [dbo].[CampaignTagMap] CMTM (NOLOCK)
		INNER JOIN [dbo].[Campaigns] C (NOLOCK) ON CMTM.CampaignID = C.CampaignID AND C.IsDeleted = 0
		INNER JOIN Tags T on t.TagID = CMTM.TagID
		GROUP BY CMTM.TagID
			
				
		UNION ALL

		SELECT FT.TagID,COUNT(FT.TagID) Counts FROM [dbo].[FormTags] FT (NOLOCK) 
		INNER JOIN [dbo].[Forms] F (NOLOCK) ON FT.[FormID] = F.[FormID] AND F.IsDeleted = 0
		INNER JOIN Tags T on t.TagID = FT.TagID
		GROUP BY FT.TagID

		UNION ALL

		SELECT OTM.TagID,COUNT(OTM.TagID) Counts FROM [dbo].[OpportunityTagMap] OTM (NOLOCK)
		INNER JOIN [dbo].[Opportunities] O (NOLOCK) ON OTM.OpportunityID = O.OpportunityID AND O.IsDeleted = 0
		INNER JOIN Tags T on t.TagID = OTM.TagID
		GROUP BY OTM.TagID

		UNION ALL

		SELECT SD.SearchDefinitionTagID,COUNT(SearchDefinitionID) FROM [dbo].[SearchDefinitionTagMap]  SD (NOLOCK) 
		INNER JOIN Tags T on t.TagID = SD.SearchDefinitionTagID
		GROUP BY SD.SearchDefinitionTagID
	
	)T 
	group by TagID
	--ORDER BY TagID 

	--SELECT DISTINCT TAGID,COUNTS FROM #TagS ORDER BY TagID 

	--SELECT T.TagID,T.[COUNT],TT.TagID,TT.COUNTS FROM #t TT
	--INNER JOIN Tags T on T.TagID = TT.TagID
	

	/*

	UPDATET
	SET T.[COUNT] = TT.COUNTS
	FROM Tags T 
	INNER JOIN #T TT ON TT.TagID = T.TagID

	*/

END