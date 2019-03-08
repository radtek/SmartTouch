
CREATE VIEW [dbo].[GET_Contact_Email_Phone_Zip]
AS
select c.contactid, isnull(ce.Email,'') Email, isnull(CASE WHEN (cp.CountryCode IS NOT NULL AND cp.CountryCode != '')  THEN '+' + cp.CountryCode + ' ' ELSE '' END
 + cp.PhoneNumber + CASE WHEN (cp.Extension IS NOT NULL AND cp.Extension != '') THEN ' Ext.' + cp.Extension ELSE '' END,'') PhoneNumber,isnull(ca.ZipCode,'') Zip from contacts (NOLOCK) c
left join (select contactid, email from contactemails(NOLOCK)  where IsPrimary = 1) ce on c.contactid = ce.ContactID 
left join (select contactid, PhoneNumber, CountryCode, Extension from ContactPhoneNumbers(NOLOCK) where IsPrimary = 1) cp on c.ContactID = cp.ContactID
left join (select ZipCode,a.AddressID,cam.contactid from Addresses(NOLOCK) a 
	left join ContactAddressMap(NOLOCK) cam on a.AddressID = cam.AddressID where a.IsDefault = 1) ca on c.ContactID = ca.ContactID



GO


