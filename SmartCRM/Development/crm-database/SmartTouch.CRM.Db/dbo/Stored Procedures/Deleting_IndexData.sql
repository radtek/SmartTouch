CREATE PROCEDURE  [dbo].[Deleting_IndexData]
AS

BEGIN

   DECLARE @rowCount INT = 1

 WHILE @rowCount > 0
	BEGIN
	 DELETE TOP(5000) IndexData WHERE Status = 2
	 SET @rowCount = @@ROWCOUNT
	END

END