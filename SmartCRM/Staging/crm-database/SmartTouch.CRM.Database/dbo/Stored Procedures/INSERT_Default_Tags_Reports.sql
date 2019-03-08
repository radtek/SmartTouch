

CREATE PROCEDURE [dbo].[INSERT_Default_Tags_Reports]
  (
     @AccountID INT
  )
AS 
BEGIN

DECLARE @CreatedBy INT

SET @CreatedBy = (SELECT CreatedBy FROM dbo.Accounts WHERE AccountID = @AccountID AND IsDeleted = 0 AND Status = 1)

INSERT INTO [dbo].[Reports]([ReportName],[AccountID],[LastRunOn],[ReportType],[CreatedBy],[CreatedOn],[LastUpdatedBy],[LastUpdatedOn])
SELECT 'Tags', @AccountID, GETUTCDATE(), 12, @CreatedBy, GETUTCDATE(), LastUpdatedBy, LastUpdatedOn
  FROM [dbo].[Reports] WHERE AccountID IS NULL AND ReportName = 'Tags'

END

/*
    EXEC INSERT_Default_Tags_Reports
	   @AccountID  = 4225

   
*/