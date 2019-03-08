-- =============================================
-- Author:		Avinash Reddy
-- Create date: 4th May 2017
-- Description:	to get the list of Never Bounce requests
-- =============================================
CREATE PROCEDURE [dbo].[GET_NeverBounce_Requests]
	@AccountID INT, @Take INT, @Skip INT
AS
BEGIN
	DECLARE @NeverBounceRequests TABLE (NeverBounceRequestID INT, ServiceStatus SMALLINT, AccountName VARCHAR(250), CreatedDate DATETIME, TotalScrubQueCount INT, EmailsCount INT)

	IF (@AccountID != 1)
		INSERT INTO @NeverBounceRequests
		SELECT NeverBounceRequestID, ServiceStatus, AccountName, NBR.CreatedOn, COUNT(1) OVER() as TotalScrubQueCount, NBR.EmailsCount FROM NeverBounceRequests (NOLOCK) NBR
		JOIN Accounts (NOLOCK) A ON A.AccountID = NBR.AccountID
		WHERE NBR.AccountID = @AccountID AND NBR.ServiceStatus != 900
		ORDER BY NBR.NeverBounceRequestID DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

	ELSE
		INSERT INTO @NeverBounceRequests
		SELECT NeverBounceRequestID, ServiceStatus, AccountName, NBR.CreatedOn, COUNT(1) OVER() as TotalScrubQueCount, NBR.EmailsCount FROM NeverBounceRequests (NOLOCK) NBR
		JOIN Accounts (NOLOCK) A ON A.AccountID = NBR.AccountID
		WHERE NBR.ServiceStatus != 900
		ORDER BY NBR.NeverBounceRequestID DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

    ;WITH NEVERBOUNCECTE AS (
	SELECT NBR.NeverBounceRequestID, NBR.AccountName, LAJ.FileName, NBR.CreatedDate, NBR.ServiceStatus AS Status, COUNT(LAJD.LeadAdapterJobLogID) AS TotalImportedContacts,
	NBR.TotalScrubQueCount, NBM.NeverBounceEntityType AS EntityType 
	FROM @NeverBounceRequests NBR
	JOIN NeverBounceMappings (NOLOCK) NBM ON NBM.NeverBounceRequestID = NBR.NeverBounceRequestID
	JOIN LeadAdapterJobLogs (NOLOCK) LAJ ON LAJ.LeadAdapterJobLogID = NBM.EntityID
	JOIN LeadAdapterJobLogDetails (NOLOCK) LAJD ON LAJD.LeadAdapterJobLogID = LAJ.LeadAdapterJobLogID
	WHERE LAJ.LeadAdapterJobStatusID = 1 AND LAJ.ProcessedFileName IS NOT NULL AND NBM.NeverBounceEntityType = 1
	GROUP BY NBR.NeverBounceRequestID, LAJD.LeadAdapterJobLogID, NBR.AccountName, LAJ.FileName, NBR.CreatedDate, NBR.ServiceStatus, NBR.TotalScrubQueCount, NBM.NeverBounceEntityType

	UNION

	SELECT NBR.NeverBounceRequestID, NBR.AccountName, STUFF((SELECT distinct ', ' + T.TagName
								 FROM NeverBounceMappings (NOLOCK) NBM
								 JOIN vTags (NOLOCK) T ON T.TagID = NBM.EntityID
								 WHERE NBM.NeverBounceRequestID = NBR.NeverBounceRequestID AND NBM.NeverBounceEntityType = 2
									FOR XML PATH(''), TYPE
									).value('.', 'NVARCHAR(MAX)') 
								,1,2,'') AS [FileName], NBR.CreatedDate, NBR.ServiceStatus AS Status, NBR.EmailsCount AS TotalImportedContacts, NBR.TotalScrubQueCount, 
								NBM.NeverBounceEntityType AS EntityType
	FROM @NeverBounceRequests NBR
	JOIN NeverBounceMappings (NOLOCK) NBM ON NBM.NeverBounceRequestID = NBR.NeverBounceRequestID
	WHERE NBM.NeverBounceEntityType = 2
	GROUP BY NBR.NeverBounceRequestID, NBR.AccountName, NBR.CreatedDate, NBR.ServiceStatus, NBR.TotalScrubQueCount, NBM.NeverBounceEntityType, NBR.EmailsCount

	UNION

	SELECT NBR.NeverBounceRequestID, NBR.AccountName, STUFF((SELECT distinct ', ' + SD.SearchDefinitionName
								 FROM NeverBounceMappings (NOLOCK) NBM
								 JOIN SearchDefinitions (NOLOCK) SD ON SD.SearchDefinitionID = NBM.EntityID
								 WHERE NBM.NeverBounceRequestID = NBR.NeverBounceRequestID AND NBM.NeverBounceEntityType = 3
									FOR XML PATH(''), TYPE
									).value('.', 'NVARCHAR(MAX)') 
								,1,2,'') AS [FileName], NBR.CreatedDate, NBR.ServiceStatus AS Status, NBR.EmailsCount AS TotalImportedContacts, NBR.TotalScrubQueCount,
								 NBM.NeverBounceEntityType AS EntityType
	FROM @NeverBounceRequests NBR
	JOIN NeverBounceMappings (NOLOCK) NBM ON NBM.NeverBounceRequestID = NBR.NeverBounceRequestID
	WHERE NBM.NeverBounceEntityType = 3
	GROUP BY NBR.NeverBounceRequestID, NBR.AccountName, NBR.CreatedDate, NBR.ServiceStatus, NBR.TotalScrubQueCount, NBM.NeverBounceEntityType, NBR.EmailsCount
	)

    SELECT DISTINCT N.NeverBounceRequestID,AccountName,FileName,createdDate,Status,TotalImportedContacts,totalScrubQueCount,EntityType, 
	COUNT(NE.NeverBounceEmailStatusID)
	AS BadEmailsCount, 
	CAST(ROUND(CAST(SUM (CASE WHEN NE.EmailStatus = 53 THEN 1 ELSE 0 END) AS FLOAT)/NULLIF(CAST(TotalImportedContacts AS FLOAT), 0) * 100,2)  
	AS NVARCHAR(MAX))+'%' + ' | ' + CAST(SUM (CASE WHEN NE.EmailStatus = 53 THEN 1 ELSE 0 END) AS NVARCHAR(MAX)) 
	AS [BadEmailsPercentage],
	SUM (CASE WHEN NE.EmailStatus = 51 THEN 1 ELSE 0 END) GoodEmailsCount,
	CAST(ROUND(CAST(SUM (CASE WHEN NE.EmailStatus = 51 THEN 1 ELSE 0 END) AS FLOAT)/NULLIF(CAST(TotalImportedContacts AS FLOAT), 0) * 100,2)  
	AS NVARCHAR(MAX))+'%' + ' | ' + CAST(SUM (CASE WHEN NE.EmailStatus = 51 THEN 1 ELSE 0 END) AS NVARCHAR(MAX)) 
	AS [GoodEmailsPercentage]
	FROM NEVERBOUNCECTE n
	LEFT JOIN NeverBounceEmailStatus (NOLOCK) NE ON NE.NeverBounceRequestID = n.NeverBounceRequestID --AND NE.EmailStatus = 53
	GROUP BY N.NeverBounceRequestID,AccountName,FileName,createdDate,Status,TotalImportedContacts,totalScrubQueCount,EntityType
	ORDER BY N.NeverBounceRequestID DESC
END
