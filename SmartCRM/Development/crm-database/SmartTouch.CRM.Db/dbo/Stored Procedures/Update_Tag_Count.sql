CREATE PROCEDURE [dbo].[Update_Tag_Count](
	 @TagID INT
)
AS
BEGIN



	SELECT TagID ,sum(Counts) AS [Count]
	INTO #Tags
	FROM
	(
		SELECT CTM.TagID,COUNT(CTM.TagID) Counts FROM [dbo].[ContactTagMap] CTM  (NOLOCK)
		INNER JOIN [dbo].[Contacts] C  (NOLOCK) ON CTM.ContactID = C.ContactID AND C.IsDeleted = 0 and CTM.Accountid = C.Accountid
		WHERE CTM.TagID = @TagID 
		GROUP BY CTM.TagID
	)T 
	group by TagID

	IF EXISTS( SELECT 1 FROM #Tags)
	BEGIN
			UPDATE T
			SET
			T.[Count] = TT.[Count]
			FROM Tags T 
			JOIN #Tags TT ON TT.TagID = T.TagID
	END
	ELSE
	BEGIN
		    UPDATE T
			SET
			T.[Count] = 0
			FROM Tags T WHERE T.TagID=@TagID
	END

END
GO