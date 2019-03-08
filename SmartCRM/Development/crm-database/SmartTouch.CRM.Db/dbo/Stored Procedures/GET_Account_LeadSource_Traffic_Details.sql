
CREATE PROC [dbo].[GET_Account_LeadSource_Traffic_Details]
	(
		@StartDate          datetime,
		@EndDate            datetime,
		@UserIDs			VARCHAR(MAX),
		@CommunityIDs		VARCHAR(MAX),
		@DropdownvalueIDs	VARCHAR(MAX)
	)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY	

	IF (LEN(@UserIDs) > 0 AND LEN(@CommunityIDs) = 0)
		BEGIN
		  SELECT U.UserID, U.FirstName + U.LastName AS OwnerName, DV.DropdownValueID, DV.DropdownValue, c.ContactID
		   FROM DBO.DropdownValues(NOLOCK) DV  
				  INNER JOIN DBO.Users(NOLOCK) U  ON U.AccountID = DV.AccountID 
				  INNER JOIN DBO.Contacts_Audit(NOLOCK) CA ON CA.AccountID = U.AccountID
				  INNER JOIN dbo.Contacts(NOLOCK) C ON C.AccountID = U.AccountID AND C.ContactID = CA.ContactID
			WHERE CA.AuditDate >= @StartDate AND CA.AuditDate <= @EndDate AND DV.DropdownID = 5
				AND U.UserID IN (SELECT DataValue FROM dbo.Split(@UserIDs, ',')) 
				AND DV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@DropdownvalueIDs, ','))
			   GROUP BY U.UserID, U.FirstName, U.LastName, DV.DropdownValueID, DV.DropdownValue, C.ContactID
			   ORDER BY U.UserID
		END
	ELSE IF (LEN(@UserIDs) = 0 AND LEN(@CommunityIDs) > 0)
		BEGIN
		   SELECT CM.CommunityID, CM.CommunityName, DV.DropdownValueID, DV.DropdownValue, c.ContactID
		   FROM DBO.DropdownValues(NOLOCK) DV  
				  INNER JOIN DBO.Communities(NOLOCK) CM  ON CM.AccountID = DV.AccountID 
				  INNER JOIN DBO.Users(NOLOCK) U ON U.AccountID = CM.AccountID
				  INNER JOIN dbo.Tours(NOLOCK) T ON T.CommunityID = CM.CommunityID 
				  INNER JOIN dbo.Tours_Audit(NOLOCK) TA ON TA.TourID = T.TourID AND T.CommunityID = TA.CommunityID
				  INNER JOIN dbo.ContactTourMap(NOLOCK) CTM ON CTM.TourID = T.TourID 
				  INNER JOIN dbo.ContactTourMap_Audit(NOLOCK) CTMA ON CTMA.TourID = T.TourID AND CTM.ContactID = CTMA.ContactID
				  INNER JOIN DBO.Contacts_Audit(NOLOCK) CA ON CA.AccountID = CM.AccountID AND CTM.ContactID = CA.ContactID 			                                                                  
				  INNER JOIN dbo.Contacts(NOLOCK) C ON C.AccountID = CM.AccountID AND CA.ContactID = CTMA.ContactID 
			WHERE CTMA.AuditDate >= @StartDate AND CTMA.AuditDate <= @EndDate AND DV.DropdownID = 5
				AND CM.CommunityID IN (SELECT DataValue FROM dbo.Split(@CommunityIDs, ',')) 
				AND DV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@DropdownvalueIDs, ','))
			   GROUP BY CM.CommunityID, CM.CommunityName, DV.DropdownValueID, DV.DropdownValue, U.UserID, U.FirstName, U.LastName, c.ContactID
			   ORDER BY CM.CommunityID, U.UserID
		END
	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF


END

/*
	  EXEC [DBO].[GET_Account_LeadSource_Traffic_Details]
		 @StartDate         = '2014-11-01 00:00:00.000', 
		 @EndDate           = '2014-11-30 00:00:00.000',      
		 @UserIDs			= '9,11,16,43', 
		 @CommunityIDs		= '',
		 @DropdownvalueIDs   = '11,12,13'


*/





GO


