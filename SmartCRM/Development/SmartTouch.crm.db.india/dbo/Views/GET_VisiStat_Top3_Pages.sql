



CREATE VIEW [dbo].[GET_VisiStat_Top3_Pages]
AS
SELECT	*
FROM	(
			SELECT ContactID, PageVisited, VisitReference,
					PageNumber = 'Page' + CAST( RANK() OVER(PARTITION BY VisitReference ORDER BY Duration desc, ContactWebVisitID desc) AS VARCHAR)
			FROM	ContactWebVisits
			WHERE	isnull(VisitReference, '000') <> '000'
		) tmp
PIVOT(
	MAX(PageVisited)
	FOR PageNumber IN ( [Page1], [Page2], [Page3] )
)p







