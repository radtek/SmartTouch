CREATE PROCEDURE [dbo].[Get_ReEngamentClickSummaryReport]
	  @AccountID INT	
	, @FromDate DATETIME
	, @ToDate DATETIME
	, @Entities VARCHAR(MAX) 
	, @IsDefaultDateRange bit
AS
BEGIN
	
	DECLARE @EntitiesTable TABLE (ID INT IDENTITY(1,1),CampaignID INT, LinkId INT)
	DECLARE @Stats TABLE (ID INT IDENTITY(1,1), CampaignID INT, CampaignName VARCHAR(250),CurrentMonth INT, LastMonth INT, Last12Months INT)
	
	IF ((SELECT sum(cast(DataValue as int)) FROM dbo.Split_2(@Entities,',')) > 0)
	BEGIN
		INSERT INTO @EntitiesTable
		SELECT CL.CampaignID, L.DataValue FROM dbo.Split_2(@Entities,',') L
		INNER JOIN CampaignLinks (NOLOCK) CL ON CL.CampaignLinkID = L.DataValue
	END
	ELSE
	BEGIN
		INSERT INTO @EntitiesTable
		SELECT DISTINCT CM.CampaignID, WCAL.LinkID FROM WorkflowActions WA (NOLOCK)--CM.CampaignID, WCAL.LinkID, 
		INNER JOIN Workflows W (NOLOCK) ON W.WorkflowID = WA.WorkflowID
		INNER JOIN WorkflowCampaignActions WCA (NOLOCK) ON WA.WorkflowActionID = WCA.WorkflowActionID
		INNER JOIN Campaigns (NOLOCK) CM ON CM.CampaignID = WCA.CampaignID
		INNER JOIN WorkflowCampaignActionLinks (NOLOCK) WCAL ON  WCAL.ParentWorkflowActionID = WCA.WorkflowCampaignActionID
		WHERE CM.IsDeleted = 0 --AND CM.CampaignStatusID = 107 
		AND CM.AccountID = @AccountID AND WA.IsDeleted = 0 AND W.IsDeleted = 0
	END
	--select * from @EntitiesTable
	IF @IsDefaultDateRange > 0
		BEGIN
			--Current Month
			DECLARE @CurrentMonthTo DATETIME = DATEADD(dd, DATEDIFF(dd, 0, getutcdate()) + 1, 0)
			DECLARE @CurrentMonthFrom DATETIME = DATEADD(dd,-1*DATEPART(dd,@CurrentMonthTo) + 1 , @CurrentMonthTo)
			DECLARE @LastMonthTo DATETIME = DATEADD(SS, -1, DATEADD(MONTH, DATEDIFF(MONTH, -1, getutcdate())-1, 0))
			DECLARE @LastMonthFrom DATETIME = DATEADD(MM, DATEDIFF(MM, 0, getutcdate())-1, 0)
			DECLARE @Last12MonthsTo DATETIME = @CurrentMonthTo
			DECLARE @Last12MonthsFrom DATETIME = DATEADD(YY, -1, @Last12MonthsTo)
			
			;WITH CurrrentMonthClicks AS (
				SELECT E.CampaignID, COUNT(1) CurrentMonth FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @CurrentMonthFrom AND @CurrentMonthTo AND CS.AccountID = @AccountID
				GROUP BY E.CampaignID)
			,CurrrentMonthContacts AS (
			select CampaignID,count(1) CurrentMonthContacts from (
				SELECT DISTINCT E.CampaignID, VCR.ContactID FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN CampaignRecipients	VCR ON VCR.CampaignRecipientID = CS.CampaignRecipientID AND VCR.AccountID = @AccountID
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @CurrentMonthFrom AND @CurrentMonthTo AND CS.AccountID = @AccountID) tmp
				GROUP BY CampaignID)
			, LastMonthClicks AS (		
				SELECT  E.CampaignID, COUNT(1) LastMonth FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @LastMonthFrom AND @LastMonthTo AND CS.AccountID = @AccountID
				GROUP BY E.CampaignID)
			,LastMonthContacts AS (
				SELECT CampaignID,count(1) LastMonthContacts from (
				SELECT DISTINCT E.CampaignID, VCR.ContactID FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN CampaignRecipients	VCR ON VCR.CampaignRecipientID = CS.CampaignRecipientID AND VCR.AccountID = @AccountID
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @LastMonthFrom AND @LastMonthTo AND CS.AccountID = @AccountID) tmp
				GROUP BY CampaignID)			
			, Last12MonthsClicks AS (
				SELECT  E.CampaignID, COUNT(1) Last12Months FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @Last12MonthsFrom AND @Last12MonthsTo AND CS.AccountID = @AccountID
				GROUP BY E.CampaignID)
			,Last12MonthSContacts AS (
				SELECT CampaignID,count(1) Last12MonthSContacts from (
				SELECT DISTINCT E.CampaignID, VCR.ContactID FROM CampaignStatistics (NOLOCK) CS
				INNER JOIN CampaignRecipients	VCR ON VCR.CampaignRecipientID = CS.CampaignRecipientID AND VCR.AccountID = @AccountID
				INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
				WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @Last12MonthsFrom AND @Last12MonthsTo AND CS.AccountID = @AccountID) tmp
				GROUP BY CampaignID)			
			, UniqueCampaigns AS (
				SELECT DISTINCT CampaignID FROM @EntitiesTable 
			)
			
			
			SELECT ET.CampaignID, C.Name, 
			COALESCE(CM.CurrentMonth,0) CurrentMonthClicks, 
			COALESCE(LM.LastMonth,0) LastMonthClicks,
			COALESCE(L12M.Last12Months,0) Last12MonthsClicks, 
			COALESCE(CMC.CurrentMonthContacts,0) CurrentMonthContacts,
			COALESCE(LMC.LastMonthContacts,0) LastMonthContacts,
			COALESCE(LTMC.Last12MonthsContacts,0) Last12MonthsContacts,
			0 Total FROM UniqueCampaigns ET 
			INNER JOIN Campaigns C (NOLOCK) ON C.CampaignID = ET.CampaignID
			LEFT JOIN CurrrentMonthClicks CM ON CM.CampaignID = ET.CampaignID
			LEFT JOIN LastMonthClicks LM ON LM.CampaignID = ET.CampaignID
			LEFT JOIN Last12MonthsClicks L12M ON L12M.CampaignID = ET.CampaignID
			LEFT JOIN CurrrentMonthContacts CMC ON CMC.CampaignID = ET.CampaignID
			LEFT JOIN LastMonthContacts LMC ON LMC.CampaignID = ET.CampaignID
			LEFT JOIN Last12MonthsContacts LTMC ON LTMC.CampaignID = ET.CampaignID
			ORDER BY ET.CampaignID DESC

		
		END
	ELSE
		BEGIN
			--select @FromDate 
			--select @ToDate
			--select @ToDate = DATEADD(dd, DATEDIFF(dd, 0, @ToDate) + 1, 0)
			print @ToDate
			   ;WITH TotalClicks AS (
				SELECT  E.CampaignID, COUNT(1) TotalClicks FROM CampaignStatistics (NOLOCK) CS
					INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
					WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @FromDate AND @ToDate AND CS.AccountID = @AccountID
					GROUP BY E.CampaignID)  
					,TotalContacts AS (
				   SELECT CampaignID,count(1) TotalContacts from (
						SELECT DISTINCT E.CampaignID, VCR.ContactID FROM CampaignStatistics (NOLOCK) CS
							INNER JOIN @EntitiesTable E ON E.LinkID = CS.CampaignLinkID
							INNER JOIN CampaignRecipients VCR ON VCR.CampaignRecipientID = CS.CampaignRecipientID AND VCR.AccountID = @AccountID
							WHERE CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @FromDate AND @ToDate AND CS.AccountID = @AccountID) tmp
							GROUP BY CampaignID)
   
			   SELECT C.CampaignID,C.Name,0 CurrentMonthClicks,0 LastMonthClicks,0 Last12MonthsClicks,
				   0 CurrentMonthContacts,0 LastMonthContacts,0 Last12MonthsContacts,  
				   TCl.TotalClicks TotalClicks, TCo.TotalContacts TotalContacts FROM TotalClicks TCl
					   INNER JOIN TotalContacts TCo on TCo.CampaignID = TCl.CampaignID
					   INNER JOIN Campaigns C on TCo.CampaignID = C.CampaignID
					   --GROUP BY C.CampaignID , C.Name
					   ORDER BY C.CampaignID DESC
		END

END

-- exec Get_ReEngamentClickSummaryReport 4218,'2016-04-03 ','2016-06-02 ',null,0
-- Entities table
--	ID	CampaignID	LinkId
--	1	9649		3017
--	2	9649		3018
--	3	11463		9239
--	4	11463		9240


