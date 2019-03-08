-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Get_Email_Stats_Summary]
    @AccountID INT ,
    @ContactID INT ,
	@Date DATETIME 
AS
BEGIN
		DECLARE @deliveredCount INT
		DECLARE @opensCount INT
		DECLARE @clicksCount INT

		SELECT * INTO #TempSendEmailDetails
			FROM
			(
				SELECT SMD.SentMailDetailID,c.ContactID  FROM dbo.ContactEmailAudit(NOLOCK) CEA
				JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID AND CE.AccountID = @accountId
				JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = @AccountID
				JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
				JOIN EnterpriseCommunication.dbo.SentMails(NOLOCK) SM ON SM.RequestGuid = SMD.RequestGuid
				WHERE CEA.[Status] = 1 AND C.ContactID = @ContactID  AND CE.ContactID = @ContactID AND CEA.RequestGuid <> '00000000-0000-0000-0000-000000000000' AND CEA.SentOn > @Date
			) TEMP

		DECLARE @EmailStastics TABLE(Delivered INT,Opened INT,Clicked INT)

		SELECT @deliveredCount = COUNT(1) FROM #TempSendEmailDetails

		SELECT @opensCount = COUNT(distinct EMS.SentMailDetailID) FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
		JOIN #TempSendEmailDetails TM ON TM.SentMailDetailID = EMS.SentMailDetailID AND TM.ContactID = EMS.ContactID
		WHERE EMS.ActivityType=1 AND EMS.ActivityDate > @Date

		SELECT @clicksCount = COUNT(distinct EMS.SentMailDetailID) FROM EnterpriseCommunication.dbo.EmailStatistics (NOLOCK) EMS
		JOIN #TempSendEmailDetails TM ON TM.SentMailDetailID = EMS.SentMailDetailID AND TM.ContactID = EMS.ContactID
		WHERE EMS.ActivityType=2 AND EMS.ActivityDate > @Date

		INSERT INTO @EmailStastics VALUES(isnull(@deliveredCount,0),isnull(@opensCount,0),isnull(@clicksCount,0))

		SELECT * FROM @EmailStastics
			
END

/*
--select * from EnterpriseCommunication.dbo.EmailStatistics where contactid=6363346
	EXEC [dbo].[Get_Email_Stats_Summary] 
	  @AccountID =339,
	  @ContactID = 6363346,
	  @Date = '2017-03-11 13:22:58.523'

*/


