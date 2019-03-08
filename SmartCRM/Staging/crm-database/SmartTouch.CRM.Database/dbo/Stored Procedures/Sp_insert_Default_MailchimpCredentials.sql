
CREATE PROCEDURE [dbo].[Sp_insert_Default_MailchimpCredentials]
AS 
BEGIN

DECLARE @accountid INT
DECLARE @accountsCount INT
DECLARE @index INT
DECLARE @newguid UNIQUEIDENTIFIER
DECLARE @createdby INT

Begin Try

set @index = 0
select @accountsCount = count(a.accountid) from dbo.accounts a 
where a.accountid not in(
select distinct sp.accountid from dbo.ServiceProviders sp
inner join EnterpriseCommunication.dbo.MailRegistration mr
on mr.[guid] = sp.Logintoken
where mr.MailProviderID = 3) AND A.IsDeleted = 0 AND A.Status = 1

  Begin TRANSACTION

while(@index < @accountsCount)
begin
 select @accountid = a.accountid from dbo.accounts(NOLOCK) a 
     where a.accountid not in(
     select distinct sp.accountid from dbo.ServiceProviders(NOLOCK) sp
     inner join EnterpriseCommunication.dbo.MailRegistration(NOLOCK) mr
     on mr.[guid] = sp.Logintoken
     where mr.MailProviderID = 3) AND A.IsDeleted = 0 AND A.Status = 1
     order by a.accountid
     offset @index rows fetch next 1 row only
   --insert provider
   select @createdby = createdby from accounts(NOLOCK) where accountid = @accountid

   SELECT @NewGuid=NEWID()
   ----Serviceprovider
   INSERT INTO ServiceProviders(CommunicationTypeID,LogINToken,CreatedBy,CreatedDate , ACCOUNTID,  ProviderName, EmailType, IsDefault,SenderPhoneNumber,ImageDomainID)
            VALUES (1, @NewGuid,@CreatedBy, GETUTCDATE(), @ACCOUNTID, 'MailChimp', 3 , 0,NULL,NULL)

   -----MailRegistration
   INSERT INTO EnterpriseCommunication.dbo.MailRegistration([Guid],Name,Host,APIKey , UserName,[Password],port,IsSSLEnabled,MailProviderID,MailChimpListID,ImageDomain,VMTA,SenderDomain)
            VALUES ( @NewGuid,'', '','', '', '' , 0,1,3,NULL,NULL,'',NULL)
 
  set @index = @index + 1
end
	  	 Commit Transaction  
End Try 
Begin Catch
 IF @@TRANCOUNT > 0

       ROLLBACK TRANSACTION
      
  
    SELECT
             ERROR_NUMBER() AS ErrorNumber
            ,ERROR_SEVERITY() AS ErrorSeverity
            ,ERROR_STATE() AS ErrorState
            ,ERROR_PROCEDURE() AS ErrorProcedure
            ,ERROR_LINE() AS ErrorLine
            ,ERROR_MESSAGE() AS ErrorMessage
            ,GETDATE()
End Catch


 end

 exec  [dbo].[Sp_insert_Default_MailchimpCredentials]