-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetMyCommunicationChartDetails]
	@UserId INT,
	@AccountId INT,
	@StartDate DATETIME,
	@EndDate DATETIME
	WITH RECOMPILE 
AS
BEGIN
		DECLARE @ActionsCount INT
		DECLARE @ToursCount INT
		DECLARE @NotesCount INT
		DECLARE @EmailDeliveredCount INT
		DECLARE @EmailOpensCount INT
		DECLARE @CampaignDeliveredCount INT
	    DECLARE @CampaignOpensCount INT

		--DECLARE @EmailStastics TABLE(ID int,Name varchar(100),TotalCount int,ActivityType varchar(10))

		CREATE TABLE #EmailStastics (ID int,Name varchar(100),TotalCount int,ActivityType varchar(10))

		--FOR ACTIONS COMPLETED
		;WITH CTE AS(
	    SELECT A.ActionType,COUNT(1) AS TotalCount FROM Actions (NOLOCK) A 
		JOIN ContactActionMap (NOLOCK) CAM ON CAM.ActionID=A.ActionID
		JOIN Contacts (nolock) c on c.contactid=cam.contactid
		JOIN UserActionMap (NOLOCK) UAM ON UAM.ActionID=A.ActionID
		WHERE A.AccountID=@AccountId AND UAM.UserID=@UserId and c.isdeleted=0 AND CAM.IsCompleted=1 AND A.LastUpdatedOn BETWEEN @StartDate AND @EndDate
		GROUP BY A.ActionType) 

		INSERT INTO #EmailStastics
		SELECT DV.DropdownValueID,DV.DropdownValue,T.TotalCount,'A' FROM CTE T
		JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID=T.ActionType
		WHERE T.TotalCount >0

		--FOR TOURS COMPLETED
		INSERT INTO #EmailStastics
		SELECT 1,'Tours Completed',COUNT(1),'T' FROM Tours (NOLOCK) T
		JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourID = T.TourID
		JOIN UserTourMap (NOLOCK) UTM ON UTM.TourID = T.TourID
		WHERE T.AccountID=@AccountId AND UTM.UserID=@UserId AND CTM.IsCompleted=1 AND T.LastUpdatedOn BETWEEN @StartDate AND @EndDate

		--FOR NOTES CREATED
		;WITH NOTECTE 
		AS
		(
			SELECT N.NoteCategory,COUNT(1) AS COUNT FROM Notes (NOLOCK) N
			JOIN contactnotemap (nolock) CNM ON CNM.NOTEID=N.NOTEID
			JOIN CONTACTS(NOLOCK) C ON C.CONTACTID=CNM.CONTACTID
			WHERE N.AccountID=@AccountId AND N.CreatedBy=@UserId AND C.ISDELETED=0 AND N.CreatedOn BETWEEN  @StartDate AND @EndDate
			GROUP BY N.NoteCategory
		)
		
		INSERT INTO #EmailStastics
		SELECT DV.DropdownValueID,DV.DropdownValue,NT.COUNT,'N' FROM NOTECTE NT
		JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID=NT.NoteCategory
		WHERE COUNT > 0

		--FOR EMAILS DELIVERED AND OPENED
		SELECT * INTO #TempSendEmailDetails
		FROM
		(
			SELECT SMD.SentMailDetailID,c.ContactID  FROM dbo.ContactEmailAudit(NOLOCK) CEA
			JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID AND CE.AccountID = @accountId
			JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = @AccountID
			JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
			JOIN EnterpriseCommunication.dbo.SentMails(NOLOCK) SM ON SM.RequestGuid = SMD.RequestGuid
			WHERE CEA.[Status] = 1 AND CEA.SentBy=@UserId AND CEA.RequestGuid <> '00000000-0000-0000-0000-000000000000' AND CEA.SentOn BETWEEN @StartDate AND @EndDate
		) TEMP

		INSERT INTO #EmailStastics
		SELECT 1,'Emails Delivered', COUNT(1),'E' FROM #TempSendEmailDetails

		;with temCte
		as
		(
		SELECT Distinct EMS.ContactID,ems.SentMailDetailID FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
		JOIN #TempSendEmailDetails TM ON TM.SentMailDetailID = EMS.SentMailDetailID AND TM.ContactID = EMS.ContactID
		join contacts(nolock) c on c.contactid=ems.contactid and c.accountid=@AccountID
		WHERE EMS.ActivityType=1 AND c.isdeleted=0 and EMS.ActivityDate BETWEEN @StartDate AND @EndDate
		)

		INSERT INTO #EmailStastics
		select  1,'Emails Opened',count(1),'E' from temCte

		--FOR CAMPAIGNS SUMMARY
		SELECT * INTO #TEM
		FROM
		(
			SELECT CR.CampaignRecipientID,CR.CampaignID,CR.[SentOn],CR.DeliveredOn,CR.DeliveryStatus FROM  CampaignRecipients CR WITH (NOLOCK)
			JOIN Campaigns (NOLOCK) C ON C.CampaignID=CR.CampaignID AND C.AccountID=CR.AccountId
			WHERE  CR.AccountID = @AccountID AND C.LastUpdatedBy=@UserId  AND DeliveredOn BETWEEN @StartDate AND @EndDate
		)T

		INSERT INTO #EmailStastics
		SELECT 1,'Campaigns Delivered',COUNT(1),'C' FROM #TEM WHERE DeliveryStatus IN (111) --AND DeliveredOn BETWEEN @StartDate AND @EndDate

		INSERT INTO #EmailStastics
		SELECT 1,'Campaigns Opened',COUNT(Distinct cs.CampaignRecipientID),'C' FROM CampaignStatistics (NOLOCK) CS
		JOIN CampaignRecipients  CR WITH (NOLOCK) ON CR.CampaignRecipientID=CS.CampaignRecipientID
		JOIN Contacts (nolock) c on c.contactid=cr.contactid and c.accountid=@AccountId
		WHERE CS.ActivityType = 1 AND CS.AccountID = @AccountID AND CS.[ActivityDate] BETWEEN @StartDate AND @EndDate
	    AND c.isdeleted=0 and CS.CampaignRecipientID IN (SELECT CampaignRecipientID FROM #TEM )

		--INSERT INTO @EmailStastics(ActionsCompleted,ToursCompleted,NotesAdded,EmailsDelivered,EmailsOpened,CampaignDelivered,CampaignOpened)
		--SELECT @ActionsCount,@ToursCount,@NotesCount,@EmailDeliveredCount,@EmailOpensCount, @CampaignDeliveredCount ,@CampaignOpensCount 

		SELECT * FROM #EmailStastics
END

/*
 EXEC [dbo].[GetMyCommunicationChartDetails] 6889,4218,'2017-10-09 00:00:00.000','2017-11-17 00:00:00.000'
 */

