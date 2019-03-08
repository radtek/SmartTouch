CREATE VIEW [dbo].[GET_ContactBasicInfo]
AS
SELECT C.ContactID, LTRIM(RTRIM(CONCAT(Title, ' ',firstname,' ', lastname))) Name,LTRIM(RTRIM(CONCAT(firstname,' ', lastname))) NameWithoutTitle, ce.Email, c.OwnerID, c.AccountID 
	FROM dbo.Contacts C
		LEFT JOIN dbo.ContactEmails CE ON C.ContactID = CE.ContactID
	WHERE C.IsDeleted = 0
GO