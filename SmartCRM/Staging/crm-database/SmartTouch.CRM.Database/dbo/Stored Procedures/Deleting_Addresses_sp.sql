CREATE     PROCEDURE [dbo].[Deleting_Addresses_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
SET NOCOUNT ON
BEGIN TRY


             DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ContactAddressMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactAddressMapID
							FROM dbo.ContactAddressMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Addresses AS CE ON CEA.AddressID=CE.AddressID 
							  INNER JOIN Contacts u on u.ContactID = CEA.ContactID
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.ContactAddressMapID = CEA.ContactAddressMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactAddressMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactAddressMap'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.UserAddressMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserAddressMapID
							FROM dbo.UserAddressMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Addresses AS CE ON CEA.AddressID=CE.AddressID 
							  INNER JOIN Users u on u.UserID = CEA.UserID
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserAddressMapID = CEA.UserAddressMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserAddressMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserAddressMap'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.AccountAddressMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AccountAddressMapID
							FROM dbo.AccountAddressMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Addresses AS CE ON CEA.AddressID=CE.AddressID 
							  INNER JOIN Accounts u on u.AccountID = CEA.AccountID
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.AccountAddressMapID = CEA.AccountAddressMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT AccountAddressMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountAddressMap'



	BEGIN
				DELETE	a
				FROM	dbo.Addresses  AS a  (NOLOCK)
				inner join ContactAddressMap cp on cp.AddressID = a.AddressID
				inner join Contacts C on c.ContactID = cp.ContactID
				WHERE	c.AccountID = @Accountid 
	        SELECT @@ROWCOUNT Addresses
			END
			PRINT  ' records deleted from  Addresses'




SELECT @@ROWCOUNT TotalCount
--successfull execution query-- 
SELECT 'DEL-001' ResultCode 


	END TRY

BEGIN CATCH

		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_Addresses_sp]
		@AccountID	= 2

*/



