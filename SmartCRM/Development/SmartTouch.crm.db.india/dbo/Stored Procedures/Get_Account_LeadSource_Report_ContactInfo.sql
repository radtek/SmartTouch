

CREATE PROCEDURE [dbo].[Get_Account_LeadSource_Report_ContactInfo] 
	@AccountID INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@GroupType INT,
	@Entities VARCHAR(MAX),
	@LeadSourceID VARCHAR(MAX)
AS
BEGIN
	IF (@GroupType = 1 OR @GroupType = 2)
	BEGIN
		SELECT  C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber
		FROM Contacts (NOLOCK) C 
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND CLM.IsPrimaryLeadSource = 1
		JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) Users ON Users.DataValue = CASE 
																								WHEN @GroupType =1 THEN C.OwnerID
																								WHEN @GroupType = 2 THEN C.LifecycleStage 
																							END
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
		AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		AND CLM.LeadSouceID IN (SELECT DataValue FROM dbo.Split(@LeadSourceID, ','))
	END
	ELSE
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

		SELECT  C.ContactID as contactID, '' as firstname, '' as lastname, '' as email, '' as phonenumber
		FROM Contacts (NOLOCK) C 
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
		JOIN (SELECT DataValue FROM dbo.Split(@Entities, ',')) Communities ON Communities.DataValue = CCM.CommunityID
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
		AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		AND CLM.LeadSouceID IN (SELECT DataValue FROM dbo.Split(@LeadSourceID, ','))
	END
END
GO


