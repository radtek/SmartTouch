
CREATE PROCEDURE [dbo].[Get_Account_All_LeadSource_Report_ContactInfo] 
	@AccountID INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@GroupType INT,
	@ColIndex INT,
	@Entities VARCHAR(MAX),
	@LeadSourceID INT
AS
BEGIN
	IF(@GroupType = 1)
	BEGIN
		IF(@ColIndex = 2)
		BEGIN 
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
			AND CLM.isprimaryleadsource = 1 
		END
		ELSE 
		BEGIN
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
			
		END
	END
	ELSE IF (@GroupType = 2)
	BEGIN
		IF (@ColIndex = 2)
		BEGIN
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) LifeCycleIds ON LifeCycleIds.DataValue = C.LifecycleStage
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
			AND CLM.isprimaryleadsource = 1
		END
		ELSE
		BEGIN
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) LifeCycleIds ON LifeCycleIds.DataValue = C.LifecycleStage
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
			
		END
	END
	ELSE IF (@GroupType = 3)
	BEGIN
		DECLARE @ContactCommunityMap TABLE (ContactCommunityMapID INT IDENTITY(1,1), ContactID INT, CommunityID INT, RowNumber INT)
		INSERT INTO @ContactCommunityMap 
		SELECT ContactID, CommunityID, RowNumber
		FROM (SELECT CCM.ContactCommunityMapID, CCM.ContactID, CCM.CommunityID, ROW_NUMBER() OVER(PARTITION BY CCM.ContactID ORDER BY CCM.CreatedON ASC) RowNumber FROM Contacts C (NOLOCK)
		INNER JOIN ContactCommunityMap (NOLOCK) CCM ON CCM.ContactID = C.ContactID
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND CCM.IsDeleted = 0) x
		UNION
		SELECT tm.ContactID, t.CommunityID,1 FROM Tours t (NOLOCK)
		INNER JOIN ContactTourMap TM (NOLOCK) ON TM.TourID = t.TourID
		WHERE t.AccountID = @AccountID

		IF (@ColIndex = 2)
		BEGIN
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate 
			INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
			JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) CommunityIds ON CommunityIds.DataValue = CCM.CommunityID
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
			AND CLM.isprimaryleadsource = 1
		END
		ELSE
		BEGIN	
			SELECT C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
			INNER JOIN ContactLeadSourceMap (NOLOCK) CLM ON CLM.ContactID = C.ContactID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate 
			INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
			JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) CommunityIds ON CommunityIds.DataValue = CCM.CommunityID
			WHERE C.AccountId = @AccountID AND C.IsDeleted = 0 AND CLM.LeadSouceID = @LeadSourceID
		END
	END
END

GO


