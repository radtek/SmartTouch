
CREATE VIEW [dbo].[GET_PhoneFields]
as
select	c.AccountID, cp.ContactID, cp.PhoneType CustomFieldID, 'D' FieldType, PhoneNumber FieldValue,
		SortOrder = RANK() OVER(partition by cp.ContactID, cp.PhoneType ORDER BY cp.IsPrimary DESC, cp.ContactPhoneNumberID ASC)
from	ContactPhoneNumbers cp inner join Contacts c on c.ContactID = cp.ContactID
where	c.IsDeleted = 0
		and cp.IsDeleted = 0

