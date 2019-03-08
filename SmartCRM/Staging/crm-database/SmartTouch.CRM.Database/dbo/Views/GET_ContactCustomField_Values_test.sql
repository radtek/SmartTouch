

CREATE VIEW [dbo].[GET_ContactCustomField_Values_test]
AS
SELECT	cf.ContactID, cf.CustomFieldID, FieldInputTypeID, dbo.GetCustomFieldValue_test(FieldInputTypeID, Value) FieldValue
FROM	dbo.contacts c INNER JOIN ContactCustomFieldMap cf on c.ContactID = cf.ContactID
		INNER JOIN Fields f on f.FieldID = cf.CustomFieldID

