

CREATE view [dbo].[GET_Primary_Contact_Phone_Number]
AS
SELECT	ContactID, PhoneNumber, AccountID,
		RANK() OVER(PARTITION BY ContactID ORDER BY IsPrimary DESC, ContactPhoneNumberID) SortOrder
FROM	ContactPhoneNumbers

