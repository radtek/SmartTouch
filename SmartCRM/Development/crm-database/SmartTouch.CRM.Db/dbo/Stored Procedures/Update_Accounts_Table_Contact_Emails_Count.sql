/****** Object:  UserDefinedFunction [dbo].[Accounts_ContactsCount]    Script Date: 12/28/2016 8:35:51 PM ******/

create Procedure [dbo].[Update_Accounts_Table_Contact_Emails_Count]
AS
BEGIN 

/* UPDATING Accounts Table ContactCount Column  */

	SELECT * INTO #TEM
	FROM
	(
	SELECT distinct C.AccountID,COUNT(C.AccountID) Counts FROM [dbo].[Contacts] C WITH (NOLOCK) 
	INNER JOIN Accounts A ON A.AccountID = C.AccountID WHERE   C.IsDeleted = 0  AND A.IsDeleted = 0  and a.[status] = 1 
	group by C.AccountID
	)T

	

	
	UPDATE A
	SET A.ContactsCount = T.Counts
	FROM  Accounts A  WITH (NOLOCK) 
	INNER JOIN #TEM T WITH (NOLOCK) ON T.AccountID = A.AccountID AND A.ContactsCount != T.Counts
	WHERE A.IsDeleted = 0 
	


/* UPDATING Accounts Table EmailsCount Column  */
	  
   SELECT * INTO #TEM_1
   FROM 
        (
   		 SELECT sp.AccountID, COUNT(DISTINCT(SM.RequestGuid))Counts  FROM [EnterpriseCommunication].dbo.[SentMails] SM
         WITH (NOLOCK)  INNER JOIN [dbo].ServiceProviders SP WITH (NOLOCK) ON SP.LoginToken = SM.TokenGuid
		 WHERE  SP.CommunicationTypeID=1
		 GROUP BY sp.AccountID
		)TT
   
  


   --   SELECT a.AccountID,A.EmailsCount,TT.AccountID,TT.Counts FROM Accounts A  WITH (NOLOCK) 
	  --INNER JOIN #TEM_1 TT ON TT.AccountID = A.AccountID
	  --WHERE A.IsDeleted = 0 

	
		UPDATE A
		SET A.EmailsCount = TT.Counts
		FROM  Accounts A  WITH (NOLOCK) 
		INNER JOIN #TEM_1 TT WITH (NOLOCK) ON TT.AccountID = A.AccountID and  A.EmailsCount != TT.Counts
		WHERE A.IsDeleted = 0 



END
GO

