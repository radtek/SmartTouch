
CREATE PROCEDURE [dbo].[Get_Contact_Owner] 
	-- Add the parameters for the stored procedure here
	@ContactIds varchar(max),
	@UserIds varchar(max),
	@OwnerID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	DECLARE @ContactOwner TABLE (ID INT IDENTITY(1,1),ContactID int,OwnerID int)
	DECLARE @Contacts TABLE (ContactID INT)
	DECLARE @Users TABLE (UserID INT)

	INSERT INTO @Contacts
	SELECT DataValue FROM dbo.Split(@ContactIds, ',')

	INSERT INTO @Users
	SELECT DataValue FROM dbo.Split(@UserIds, ',')
	
	INSERT INTO @ContactOwner 
	SELECT C.ContactID, COALESCE(OwnerID, @OwnerID) FROM Contacts (NOLOCK) C
	INNER JOIN @Contacts CT ON CT.ContactID = C.ContactID
	WHERE (OwnerID IN (SELECT * FROM @Users) OR OwnerID IS NULL) 
	

	SELECT  C.ContactID,COALESCE(OwnerID,@OwnerID) AS OwnerID FROM  @ContactOwner CO
	RIGHT JOIN @Contacts C ON C.ContactID = CO.ContactID
END

/*
EXEC [dbo].[Get_Contact_Owner]
@ContactIds = '884020,1741761,1742769,1742768,887215,1742767,1742762,1742766,1741744,1741762',
@UserIds = '6889,6899,7943,8976',
@OwnerID = 6889
 */

 
 