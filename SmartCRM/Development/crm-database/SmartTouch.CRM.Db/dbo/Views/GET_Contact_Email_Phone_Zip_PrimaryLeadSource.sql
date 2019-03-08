


CREATE VIEW [dbo].[GET_Contact_Email_Phone_Zip_PrimaryLeadSource]
AS
select c.contactid, isnull(ce.Email,'') Email, isnull(cp.phonenumber,'') PhoneNumber,isnull(ca.ZipCode,'') Zip, isnull(ls.DropdownValue,'') LeadSource
 from contacts c
left join (select contactid, email from contactemails  where IsPrimary = 1) ce on c.contactid = ce.ContactID 
left join (select contactid, PhoneNumber from ContactPhoneNumbers where IsPrimary = 1) cp on c.ContactID = cp.ContactID
left join (select ZipCode,a.AddressID,cam.contactid from Addresses a 
	left join ContactAddressMap cam on a.AddressID = cam.AddressID where a.IsDefault = 1) ca on c.ContactID = ca.ContactID
	left join (select cls.ContactID, ddv.DropdownValue from ContactLeadSourceMap cls 
	left join dropdownvalues ddv on cls.LeadSouceID = ddv.DropdownValueID where cls.IsPrimaryLeadSource =1) ls on c.contactid = ls.contactid







