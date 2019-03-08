




CREATE VIEW [dbo].[GET_Contact_Email_Phone]
AS
select ce.ContactID, ce.Email, cp.PhoneNumber from contactemails ce 
left join (
select contactid, Phonenumber  from ContactPhoneNumbers where isnull(IsPrimary,'') = 1) cp on ce.ContactID = cp.ContactID where isnull(ce.IsPrimary,'') = 1 







