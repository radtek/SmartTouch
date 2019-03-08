

--CREATE VIEW [dbo].[GET_ContactBasicInfo]
--AS
--SELECT C.ContactID, LTRIM(RTRIM(CONCAT(Title, ' ',firstname,' ', lastname))) Name,LTRIM(RTRIM(CONCAT(firstname,' ', lastname))) NameWithoutTitle, ce.Email, c.OwnerID, c.AccountID 
--	FROM SmartCRM.dbo.Contacts C
--		LEFT JOIN SmartCRM.dbo.ContactEmails CE ON C.ContactID = CE.ContactID
--	WHERE C.IsDeleted = 0



