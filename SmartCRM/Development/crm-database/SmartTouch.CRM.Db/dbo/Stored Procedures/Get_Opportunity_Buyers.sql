CREATE PROCEDURE [dbo].[Get_Opportunity_Buyers]
	 @opportunityID INT,
     @pageNumber INT = 1 ,
     @pageSize INT = 10,
     @accountID INT
AS
BEGIN
	SELECT OCM.OpportunityContactMapID,OCM.ContactID, C.FirstName, C.LastName,CE.Email,C.Company, OCM.StageID,OCM.Potential, OCM.ExpectedToClose,OCM.CreatedOn, OCM.Owner,c.ContactType 
	,COUNT(*) OVER() as TotalCount
	INTO #OpportunityContactMaps
	FROM OpportunityContactMap (NOLOCK) OCM
	INNER JOIN Contacts C (NOLOCK) ON C.ContactID = OCM.ContactID
	LEFT JOIN ContactEmails(NOLOCK) CE ON CE.ContactID = OCM.ContactID AND CE.IsPrimary = 1 AND CE.IsDeleted = 0
	WHERE OCM.OpportunityID = @opportunityID AND OCM.IsDeleted = 0 AND C.IsDeleted = 0 AND C.AccountID = @accountID
	ORDER BY OCM.ExpectedToClose ASC

	OFFSET (@pageNumber-1)*@pageSize ROWS
	FETCH NEXT @pageSize ROWS ONLY
 
 
	SELECT OCM.OpportunityContactMapID,OCM.ContactID,
	CASE WHEN (LEN(OCMT.FirstName) > 0 AND LEN(OCMT.LastName) >0) THEN OCMT.FirstName + ' ' + OCMT.LastName
	     WHEN OCMT.ContactType =1 THEN OCMT.Email
		 ELSE OCMT.Company END
	  AS Name,OCMT.Potential,OCMT.ExpectedToClose,OCM.Comments,U.FirstName + ' '+ U.LastName AS OwnerName,OCMT.CreatedOn,
	ROW_NUMBER() OVER(PARTITION BY OCM.ContactID ORDER BY OCM.IsDeleted ASC, OCM.CreatedOn DESC) AS RowNumber,OCMT.ContactType,DV.DropdownValue AS Stage,
	OCMT.TotalCount 
	FROM #OpportunityContactMaps OCMT
	INNER JOIN OpportunityContactMap (NOLOCK) OCM ON OCM.ContactID = OCMT.ContactID AND OCM.OpportunityID = @opportunityID
	JOIN Users(NOLOCK) U ON U.UserID= OCMT.Owner 
	JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID= OCMT.StageID
	
 	
END


/*
	exec [dbo].[Get_Opportunity_Buyers] 5552,1,10,4218
*/

--ORDER BY ABS(DateDiff(dd, entrydate, 01/05/03)) asc


