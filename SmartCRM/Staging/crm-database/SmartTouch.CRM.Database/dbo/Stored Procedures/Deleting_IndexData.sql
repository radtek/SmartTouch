
CREATE  PROCEDURE  [Deleting_IndexData]
AS
Begin

DECLARE @rowCount INT = 1
WHILE @rowCount > 0
BEGIN
 DELETE TOP(5000) IndexData WHERE Status = 2
 SET @rowCount = @@ROWCOUNT
END

end



