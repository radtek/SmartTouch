



CREATE VIEW [dbo].[GET_DropdownFieldValues]
as
select	c.AccountID, cp.ContactID, cp.PhoneType CustomFieldID, PhoneNumber FieldValue,CASE WHEN (cp.CountryCode IS NOT NULL AND cp.CountryCode != '')  THEN '+' + cp.CountryCode + ' ' ELSE '' END
 + cp.PhoneNumber + CASE WHEN (cp.Extension IS NOT NULL AND cp.Extension != '') THEN ' Ext.' + cp.Extension ELSE '' END PhoneWithFormat, IsPrimary,
		SortOrder = RANK() OVER(partition by cp.ContactID, cp.PhoneType ORDER BY cp.IsPrimary DESC, cp.ContactPhoneNumberID ASC)
from	ContactPhoneNumbers cp WITH (NOLOCK) inner join Contacts c WITH (NOLOCK) on c.ContactID = cp.ContactID
where	c.IsDeleted = 0
		and cp.IsDeleted = 0 











