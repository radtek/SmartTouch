
CREATE VIEW [dbo].[GET_ContactCustomField_Values]
AS
SELECT	cf.ContactID, cf.CustomFieldID, FieldInputTypeID, dbo.GetCustomFieldValue(FieldInputTypeID, Value) FieldValue
FROM	dbo.contacts c INNER JOIN ContactCustomFieldMap cf on c.ContactID = cf.ContactID
		INNER JOIN Fields f on f.FieldID = cf.CustomFieldID


