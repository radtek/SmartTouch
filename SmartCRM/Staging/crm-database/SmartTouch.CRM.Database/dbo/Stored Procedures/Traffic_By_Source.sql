
CREATE PROC [dbo].[Traffic_By_Source]
(
       @StartDate          DATETIME,
       @EndDate            DATETIME,
       @UserId	           INT,
       @DropdownvalueID    SMALLINT

)
AS 
BEGIN
      DECLARE @Date DATETIME


  SELECT U.UserID, U.FirstName + U.LastName AS OwnerName, DV.DropdownValueID, DV.DropdownValue, 
  CASE WHEN (SELECT COUNT(ContactID) FROM [dbo].Contacts(NOLOCK) WHERE LastContacted = @StartDate  AND  IsDeleted = 0 GROUP BY ContactID) > 0 THEN CONVERT(bit, 1)
			ELSE CONVERT(bit, 0) END ContactsCount 
  
   FROM DBO.DropdownValues(NOLOCK) DV  
          INNER JOIN DBO.Users(NOLOCK) U  ON U.AccountID = DV.AccountID 
		  INNER JOIN DBO.Contacts_Audit(NOLOCK) CA ON CA.AccountID = U.AccountID
		  INNER JOIN dbo.Contacts(NOLOCK) C ON C.AccountID = U.AccountID AND C.ContactID = CA.ContactID
		  

    WHERE CA.AuditDate = @StartDate AND U.UserID= @UserId AND DV.DropdownValueID = @DropdownvalueID AND C.LastUpdatedOn = @EndDate AND DV.DropdownID = 5
	   --GROUP BY DV.DropdownValueID, C.ContactID, U.UserID
	   ORDER BY C.ContactID
       


END

/*
  EXEC [DBO].[Traffic_By_Source]
     @StartDate         = '2014-02-10 16:11:26.910',       
     @EndDate           = '2014-02-15 16:11:26.910',      
     @UserId	        = 11,      
     @DropdownvalueID   = 11


*/