






CREATE FUNCTION [dbo].[UDF_Contact_OldLifcycle]
	(
		@ContactID		int
	) RETURNS VARCHAR(500)
AS
BEGIN
	
	DECLARE @OldLifcycle VARCHAR(500) = ''
	
	SELECT @OldLifcycle = (SELECT DropdownValue FROM dbo.DropdownValues 
						WHERE DropdownValueID = (SELECT LifecycleStage FROM 
												(SELECT TOP 2 LifecycleStage, (RANK() OVER(PARTITION BY ContactID ORDER BY AuditDate DESC)) RankNo
													FROM dbo.Contacts_Audit WHERE ContactID = @ContactID ORDER BY AuditDate DESC)
												 C WHERE RankNo = 2))

	RETURN @OldLifcycle
END


