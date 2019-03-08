
CREATE PROCEDURE [dbo].[Get_ReengagedContacts] 
@AccountId int,
@StartDate datetime ,
@EndDate datetime,
@CampaignId int,
@LinkIds varchar(max), 
@IsDefaultDateRange bit,
@HasSelectedLinks bit,
@DrilldownPeriod tinyint

as
BEGIN
	declare @isDebug bit = 0
	DECLARE @CurrentMonthTo DATETIME = DATEADD(dd, DATEDIFF(dd, 0, getutcdate()) + 1, 0)
	DECLARE @CurrentMonthFrom DATETIME = DATEADD(dd,-1*DATEPART(dd,@CurrentMonthTo) + 1 , @CurrentMonthTo)
	DECLARE @LastMonthTo DATETIME = DATEADD(SS, -1, DATEADD(MONTH, DATEDIFF(MONTH, -1, getutcdate())-1, 0))
	DECLARE @LastMonthFrom DATETIME = DATEADD(MM, DATEDIFF(MM, 0, getutcdate())-1, 0)
	DECLARE @Last12MonthsTo DATETIME = @CurrentMonthTo
	DECLARE @Last12MonthsFrom DATETIME = DATEADD(YY, -1, @Last12MonthsTo)
	DECLARE @fromDate datetime
	DECLARE @toDate datetime

	select @fromDate = 	CASE @DrilldownPeriod 
					  WHEN 1 THEN @CurrentMonthFrom 
					  WHEN 2 THEN @LastMonthFrom  
					  WHEN 3 THEN @Last12MonthsFrom 
					  WHEN 4 THEN @StartDate 
					END 

	select @toDate = 	CASE @DrilldownPeriod 
					  WHEN 1 THEN @CurrentMonthTo
					  WHEN 2 THEN @LastMonthTo 
					  WHEN 3 THEN @Last12MonthsTo
					  WHEN 4 THEN @EndDate 
					END 
	IF @isDebug = 1
		BEGIN		
			select @fromDate
			select @toDate
		END
	IF @IsDefaultDateRange = 1
		BEGIN
			IF @HasSelectedLinks = 0
			BEGIN
        		SELECT distinct c.ContactID from vCampaignStatistics(NOLOCK) cs
					inner join CampaignLinks(NOLOCK) cl on cl.CampaignLinkID = cs.CampaignLinkID
					inner join WorkflowCampaignActionLinks(NOLOCK) wca on cl.CampaignLinkID = wca.LinkID
					inner join vCampaignRecipients(NOLOCK) cr on cs.CampaignRecipientID = cr.CampaignRecipientID AND cr.AccountID = @AccountId
					inner join contacts(NOLOCK) c on cr.ContactID = c.ContactID
				WHERE c.IsDeleted = 0 and c.AccountID = @AccountId AND cl.CampaignID = @CampaignId and cs.ActivityDate between @fromDate and @toDate AND CS.AccountID = @AccountId
				if @isDebug = 1
				BEGIN
					select * from dbo.Split_2('1',',')
				END
			END
			ELSE
			BEGIN
				SELECT distinct c.ContactID from vCampaignStatistics(NOLOCK) cs
					inner join CampaignLinks(NOLOCK) cl on cl.CampaignLinkID = cs.CampaignLinkID
					inner join WorkflowCampaignActionLinks(NOLOCK) wca on cl.CampaignLinkID = wca.LinkID
					inner join vCampaignRecipients(NOLOCK) cr on cs.CampaignRecipientID = cr.CampaignRecipientID AND cr.AccountID = @AccountId
					inner join contacts(NOLOCK) c on cr.ContactID = c.ContactID 
				WHERE c.IsDeleted = 0 and c.AccountID = @AccountId	and cl.CampaignLinkID in (select * FROM dbo.Split_2(@LinkIds,',')) and cs.ActivityDate between @fromDate and @toDate AND CS.AccountID = @AccountId
				if @isDebug = 1
					select * from dbo.Split_2('1,2',',')
			END		
		END
	ELSE
		BEGIN
			IF @HasSelectedLinks = 0
				BEGIN
        			SELECT distinct cr.ContactID from vCampaignStatistics(NOLOCK) cs
						inner join CampaignLinks(NOLOCK) cl on cl.CampaignLinkID = cs.CampaignLinkID
						inner join WorkflowCampaignActionLinks(NOLOCK) wca on cl.CampaignLinkID = wca.LinkID
						inner join vCampaignRecipients(NOLOCK) cr on cs.CampaignRecipientID = cr.CampaignRecipientID AND cr.AccountID = @AccountId
						inner join contacts(NOLOCK) c on cr.ContactID = c.ContactID
					WHERE c.IsDeleted = 0 and c.AccountID = @AccountId AND cl.CampaignID = @CampaignId  and cs.ActivityDate between @StartDate and @EndDate AND CS.AccountID = @AccountId
					if @isDebug = 1
						select * from dbo.Split_2('1,2,3',',')
				END
			ELSE
				BEGIN
					SELECT distinct cr.ContactID from vCampaignStatistics(NOLOCK) cs
						inner join CampaignLinks(NOLOCK) cl on cl.CampaignLinkID = cs.CampaignLinkID
						inner join WorkflowCampaignActionLinks(NOLOCK) wca on cl.CampaignLinkID = wca.LinkID
						inner join vCampaignRecipients(NOLOCK) cr on cs.CampaignRecipientID = cr.CampaignRecipientID AND cr.AccountID = @AccountId
						inner join contacts(NOLOCK) c on cr.ContactID = c.ContactID 
					WHERE c.IsDeleted = 0 and c.AccountID = @AccountId	and cl.CampaignLinkID in (select * FROM dbo.Split_2(@LinkIds,','))  and cs.ActivityDate between @StartDate and @EndDate AND CS.AccountID = @AccountId
					if @isDebug = 1
						select * from dbo.Split_2('1,2,3,4',',')
				END		
		END
END

-- exec Get_ReengagedContacts 4218,'2015-04-13 09:03:30.977' ,'2015-04-13 09:03:30.977', 11463,1,0,'', 1

--exec Get_ReengagedContacts 4218,'1/1/1753 12:00:00 AM' ,'1/1/1753 12:00:00 AM', 11463,'',1,0, 1
GO


