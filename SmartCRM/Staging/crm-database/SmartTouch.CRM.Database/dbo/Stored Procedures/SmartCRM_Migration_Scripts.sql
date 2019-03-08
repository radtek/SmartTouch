
CREATE PROCEDURE [dbo].[SmartCRM_Migration_Scripts]
	
AS
BEGIN

	DECLARE @AccountID  DECIMAL(21,0),
			@AccID		INT,
			@AccName	NVARCHAR(500),
			@GetAccounts CURSOR
	 
	SET @GetAccounts = CURSOR FOR
	SELECT DISTINCT aid FROM crmdata1.dbo.masteracct WHERE ACTIVE = 1
	
	OPEN @GetAccounts
	FETCH NEXT FROM @GetAccounts INTO @AccountID
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @AccName = ( SELECT TOP 1 aname FROM crmdata1.dbo.masteracct WHERE aid = @AccountID AND ACTIVE = 1 )
			SET @AccID = ( SELECT TOP 1 AccountID FROM SmartCRM.dbo.Accounts WHERE AccountName = @AccName )

			PRINT @AccName 
			PRINT 'AccountName :' + CAST(ISNULL(@AccName, 0) AS NVARCHAR(500))

			PRINT @AccountID
			PRINT 'AccountID :' + CAST(ISNULL(@AccountID, 0) AS NVARCHAR(75))

			PRINT @AccID
			PRINT 'SmartCRM AccID :' + CAST(ISNULL(@AccID, 0) AS NVARCHAR(75)) 

			INSERT INTO SmartCRM.dbo.Contacts (FirstName, LastName, Company, CommunicationID, Title, ContactImageUrl, AccountID, HomePhone, WorkPhone,
					MobilePhone, PrimaryEmail, ContactType, SSN, LifecycleStage, DoNotEmail, LastContacted, IsDeleted, ProfileImageKey, ImageID,
					ReferenceID, LastUpdatedBy, LastUpdatedOn, OwnerID, PartnerType)

			SELECT fname, lname, company, 2, rtitle, NULL, @AccID, hphone, wphone, mphone, email, 1, ssn, 1, 1, rdate, 0, null,null,null,null, lsdatetime,null,null
				FROM crmdata1.dbo.records 
				WHERE aid = @AccountID
			
			FETCH NEXT FROM @GetAccounts INTO @AccountID
		END
	CLOSE @GetAccounts
	DEALLOCATE @GetAccounts

END

/*
	EXEC [dbo].[SmartCRM_Migration_Scripts]

*/

