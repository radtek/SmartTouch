



CREATE FUNCTION [dbo].[SentEmail_Count]
(
   @AccountID    int

) RETURNS INT
AS 
BEGIN
    
	DECLARE  @EmailCount  int
	
		SELECT @EmailCount = ( SELECT  COUNT(DISTINCT(SM.RequestGuid)) FROM [EnterpriseCommunication].[dbo].[SentMails] SM
           WITH (NOLOCK)  INNER JOIN [dbo].ServiceProviders SP ON SP.LoginToken = SM.TokenGuid
		  WHERE SP.AccountID = @AccountID AND SP.CommunicationTypeID=1)

 RETURN @EmailCount
END
