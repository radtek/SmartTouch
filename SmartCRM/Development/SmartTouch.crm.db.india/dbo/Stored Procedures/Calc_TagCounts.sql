CREATE PROCEDURE [dbo].[Calc_TagCounts]
AS
BEGIN
	DELETE FROM Tag_Counts
	INSERT INTO Tag_Counts
	SELECT TagID, TagName, Count, AccountID FROM vTags
END

--CREATE TABLE Tag_Counts (TagId INT, TagName VARCHAR(200), Count INT, AccountID INT)


GO

