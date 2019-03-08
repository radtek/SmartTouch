
CREATE PROCEDURE [dbo].[Get_Send_Email_Details_Of_Contact]
	@contactId int,
	@accountId int
AS
BEGIN
	;WITH CTE
			AS
			(
			  SELECT SM.[From], SMD.[Subject],CEA.SentOn  FROM dbo.ContactEmailAudit(NOLOCK) CEA
			  JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID AND CE.AccountID = @accountId
			  JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID AND C.AccountID = @accountId
			  JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
			  JOIN EnterpriseCommunication.dbo.SentMails(NOLOCK) SM ON SM.RequestGuid = SMD.RequestGuid
			  WHERE CEA.[Status] = 1 AND C.ContactID = @contactId  AND CE.ContactID = @contactId 
			)
			SELECT * FROM CTE ORDER BY SentOn DESC
			 
END


/*
	EXEC [dbo].[Get_Send_Email_Details_Of_Contact] 1741720,4218
 */