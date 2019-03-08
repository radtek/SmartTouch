-- =============================================
-- Author:		<Author,,SurendraBabu>
-- Create date: <Create Date,,11/14/17>
-- Description:	<Description,,Gettting ContactIds for My Communication Drildown Activity >
-- =============================================
CREATE PROCEDURE [dbo].[GetMyCommunicationContacts]
    @UserId INT,
	@AccountId INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@Activity VARCHAR(10),
	@ActivityType VARCHAR(10),
	@Category SMALLINT
AS
BEGIN
	
		IF(@Activity = 'A')
		BEGIN
				SELECT CAM.ContactID FROM Actions (NOLOCK) A 
				JOIN ContactActionMap (NOLOCK) CAM ON CAM.ActionID=A.ActionID
				JOIN UserActionMap (NOLOCK) UAM ON UAM.ActionID=A.ActionID
				WHERE A.AccountID=@AccountId AND UAM.UserID=@UserId AND CAM.IsCompleted=1 AND a.ActionType=@Category
				AND A.LastUpdatedOn BETWEEN @StartDate AND @EndDate
		END
		ELSE IF(@Activity = 'T')
		BEGIN
				SELECT CTM.ContactID FROM Tours (NOLOCK) T
				JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourID = T.TourID
				JOIN UserTourMap (NOLOCK) UTM ON UTM.TourID = T.TourID
				WHERE T.AccountID=@AccountId AND UTM.UserID=@UserId AND CTM.IsCompleted=1 AND T.LastUpdatedOn BETWEEN @StartDate AND @EndDate
		END
		ELSE IF(@Activity = 'N')
		BEGIN
				SELECT CNM.ContactID FROM Notes (NOLOCK) N
				JOIN ContactNoteMap (NOLOCK) CNM ON CNM.NoteID=N.NoteID
				WHERE N.AccountID=@AccountId AND N.CreatedBy=@UserId AND n.NoteCategory=@Category
				AND N.CreatedOn BETWEEN  @StartDate AND @EndDate
		END
		ELSE IF(@Activity = 'E')
		BEGIN
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

				IF(@ActivityType = 'D')
				BEGIN
						SELECT ContactID FROM #TempSendEmailDetails
				END
				ELSE
				BEGIN
						SELECT EMS.ContactID FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
						JOIN #TempSendEmailDetails TM ON TM.SentMailDetailID = EMS.SentMailDetailID AND TM.ContactID = EMS.ContactID
						WHERE EMS.ActivityType=1 AND EMS.ActivityDate BETWEEN @StartDate AND @EndDate
				END
			
		END
		ELSE IF(@Activity = 'C')
		BEGIN
				SELECT * INTO #TEM
				FROM
				(
					SELECT CR.CampaignRecipientID,CR.CampaignID,CR.[SentOn],CR.DeliveredOn,CR.DeliveryStatus,CR.ContactID FROM  CampaignRecipients CR WITH (NOLOCK)
					JOIN Campaigns (NOLOCK) C ON C.CampaignID=CR.CampaignID AND C.AccountID=CR.AccountId
					WHERE  CR.AccountID = @AccountID AND C.LastUpdatedBy=@UserId 
				)T

				IF(@ActivityType = 'D')
				BEGIN
						SELECT ContactID FROM #TEM WHERE DeliveryStatus IN (111) AND DeliveredOn BETWEEN @StartDate AND @EndDate
				END
				ELSE
				BEGIN
						SELECT CR.ContactID FROM CampaignStatistics (NOLOCK) CS
						JOIN CampaignRecipients (NOLOCK) CR ON CR.CampaignRecipientID=CS.CampaignRecipientID
						WHERE CS.ActivityType = 1 AND CS.AccountID = @AccountID AND CS.[ActivityDate] BETWEEN @StartDate AND @EndDate
					    AND CS.CampaignRecipientID IN (SELECT CampaignRecipientID FROM #TEM )
				END
			
		END
END

/*
 EXEC [dbo].[GetMyCommunicationContacts] 6889,4218,'2017-10-09 00:00:00.000','2017-11-14 00:00:00.000','T','D'
 */
