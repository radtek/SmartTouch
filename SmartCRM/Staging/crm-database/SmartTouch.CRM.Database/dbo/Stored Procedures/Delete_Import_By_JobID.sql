

CREATE PROCEDURE  [dbo].[Delete_Import_By_JobID]
(
@LeadAdapterJobLogID INT = 0
) 
AS
BEGIN

DELETE	FROM  [dbo].LeadAdapterJobLogDetails  WHERE	LeadAdapterJobLogID = @LeadAdapterJobLogID
DELETE	FROM  [dbo].ImportDataSettings  WHERE	LeadAdaperJobID = @LeadAdapterJobLogID
DELETE	FROM  [dbo].ImportTagMap  WHERE	LeadAdapterJobLogID = @LeadAdapterJobLogID
DELETE	FROM  [dbo].LeadAdapterJobLogs    WHERE LeadAdapterJobLogID = @LeadAdapterJobLogID

END 




