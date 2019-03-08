
CREATE PROCEDURE [dbo].[Get_Send_Email_Details_Of_Contact]
    @contactId int,
	@accountId int
AS
BEGIN
		SELECT SM.[From], SMD.[Subject],CEA.SentOn,SMD.SentMailDetailID,C.ContactID
		INTO #temp_ConctactEmails
		FROM dbo.ContactEmailAudit(NOLOCK) CEA
		JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID AND CE.AccountID = @accountId
		JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = @accountId
		JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
		JOIN EnterpriseCommunication.dbo.SentMails(NOLOCK) SM ON SM.RequestGuid = SMD.RequestGuid
		WHERE CEA.[Status] = 1 AND C.ContactID = @contactId  AND CE.ContactID = @contactId AND CEA.RequestGuid <> '00000000-0000-0000-0000-000000000000'

		SELECT CE.SentMailDetailID,EMS.ActivityType,CASE WHEN EMS.EmailLinkID IS NOT NULL THEN EMS.EmailLinkID ELSE 0 END AS LinkId
		INTO #temp_ContactEmailStats
		FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
		INNER JOIN #temp_ConctactEmails CE ON CE.SentMailDetailID = EMS.SentMailDetailID AND CE.ContactID = EMS.ContactID

	   SELECT SentMailDetailID, [1] AS Opened, [2] AS Clicked 
	   INTO #temp_EmailStatistics
	   FROM 
	   (
		 SELECT DISTINCT SentMailDetailID, ActivityType, LinkId FROM #temp_ContactEmailStats
	   ) src
	   PIVOT
	   (
			COUNT(LinkId)
			FOR ActivityType IN ([1], [2])
	   ) pv;

	   ;WITH CTE AS (
	   SELECT CE.SentMailDetailID, EMS.ActivityDate, ROW_NUMBER() OVER(PARTITION BY CE.SentMailDetailID ORDER BY EMS.ActivityDate DESC) RN
	   FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
	   INNER JOIN #temp_ConctactEmails (NOLOCK) CE ON CE.SentMailDetailID = EMS.SentMailDetailID)

	   SELECT CE.SentMailDetailID, CE.[From],CE.[Subject],CE.SentOn,COALESCE(ES.Opened,0) AS Opened,COALESCE( ES.Clicked,0) AS Clicked, T.ActivityDate 
	   FROM #temp_ConctactEmails CE
	   LEFT JOIN #temp_EmailStatistics ES ON ES.SentMailDetailID = CE.SentMailDetailID
	   LEFT JOIN CTE T ON T.SentMailDetailID = ES.SentMailDetailID AND RN = 1
	   ORDER BY CE.SentOn DESC

END


/*104764,104773,104778,104766
	EXEC [dbo].[Get_Send_Email_Details_Of_Contact] 6363309,339
 */

 --SELECT DISTINCT EMS.SentMailDetailID,EMS.ActivityType,EMS.ContactID,EMS.ActivityDate,EMS.EmailLinkID FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS