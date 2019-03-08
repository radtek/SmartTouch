
CREATE PROC [dbo].[Bounced_Mail_Data_Processor]
AS

BEGIN

DECLARE @LastRunDate DATETIME

SET @LastRunDate = DateADD(mi, -1,  GETUTCDATE());

	;WITH Recipientdata
	As(
		 SELECT DISTINCT  ContactID,[To] FROM vCampaignRecipients (nolock) WHERE LastModifiedOn >= @LastRunDate
	), ContactDeliveryStatusesCTE
	AS (
		SELECT rd.ContactID,rd.[To],cr.DeliveryStatus, DENSE_RANK() OVER (PARTITION BY cr.ContactID ORDER BY  cr.CampaignRecipientID  DESC) ranking 
		FROM Recipientdata rd
		INNER JOIN vCampaignRecipients (nolock) cr ON rd.ContactID = cr.ContactID AND rd.[To] = cr.[To]
	), HardBounceContactsCTE
	AS(
		SELECT ContactId, DeliveryStatus, [To], COUNT(1) BreachCount FROM ContactDeliveryStatusesCTE WHERE Ranking < 6 AND DeliveryStatus = 112
		GROUP BY ContactID,DeliveryStatus, [To]
		HAVING COUNT(1) = 5
	)

	UPDATE  CE
	SET EmailStatus = 53
	FROM ContactEmails CE 
	INNER JOIN HardBounceContactsCTE cte ON cte.ContactID = CE.ContactID AND CE.Email = cte.[To]





	--DECLARE @LASTRUNDATE DATETIME
	--DECLARE @RECIPIENTDATA TABLE(ContactId INT,Email VARCHAR(max),RowId INT)

	--SET @LASTRUNDATE = DateADD(mi, -1,  GETUTCDATE());

	--INSERT INTO @RECIPIENTDATA(ContactId,Email,RowId)
	--SELECT ContactID,[To],ROW_NUMBER() OVER(ORDER BY CampaignRecipientId) AS RowId FROM vCampaignRecipients WHERE LastModifiedOn >= @LASTRUNDATE


	--DECLARE @LoopCounter INT
	--DECLARE @noOfRecords INT
	--DECLARE @EMAIL VARCHAR(MAX);
 --   DECLARE @ISHARDBOUNCE BIT;
	--DECLARE @CONTACTID INT;

	--SET @LoopCounter = 1
	--SET @noOfRecords = (SELECT COUNT(1) FROM @RECIPIENTDATA)		

	--WHILE (@LoopCounter <= @noOfRecords)
	--BEGIN	 
	--	SELECT @EMAIL = Email, @CONTACTID = ContactId FROM @RECIPIENTDATA WHERE RowId = @LoopCounter		
		
	--	SET @ISHARDBOUNCE = CASE WHEN( (SELECT COUNT(1) FROM (SELECT top 5 DeliveryStatus FROM vCampaignRecipients WHERE [To] = @EMAIL ORDER BY LastModifiedOn DESC) D WHERE D.DeliveryStatus = 111) = 5) THEN 1 ELSE 0 END

	--	IF(@ISHARDBOUNCE = 1)
	--	 BEGIN
	--		UPDATE ContactEmails SET EmailStatus = 53 WHERE ContactID = @CONTACTID AND Email = @EMAIL --AND IsDeleted = 0
	--	 END 
	--	SET @LoopCounter = @LoopCounter + 1
	--END
END


    

GO


