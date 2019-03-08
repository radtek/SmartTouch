
CREATE PROC [dbo].[UPDATE_Actions_Tours_Lastupdatedby]
(

  @AccountID INT
)
AS
BEGIN

		UPDATE Tours
		SET LastUpdatedBy = CreatedBy,
			LastUpdatedOn = CreatedOn
		WHERE AccountID = @AccountID AND LastUpdatedBy IS NULL  

		UPDATE Tours_Audit
		SET LastUpdatedBy = CreatedBy,
			LastUpdatedOn = CreatedOn
		WHERE AccountID = @AccountID AND LastUpdatedBy IS NULL

		UPDATE ContactTourMap
		SET LastUpdatedBy = t.CreatedBy,
			LastUpdatedOn = t.CreatedOn
		from ContactTourMap ctm
			inner join Tours t on t.TourID = ctm.TourID AND T.AccountID = @AccountID 
		where ctm.LastUpdatedBy is NULL

		UPDATE ContactTourMap_Audit
		SET LastUpdatedBy = t.CreatedBy,
			LastUpdatedOn = t.CreatedOn
		from ContactTourMap_Audit ctma
			inner join Tours t on t.TourID = ctma.TourID AND AccountID = @AccountID 
			inner join ContactTourMap ctm on ctm.TourID = ctma.TourID
		where ctma.LastUpdatedBy is NULL

/*
SELECT * FROM Actions A
    INNER JOIN ContactActionMap CAM ON CAM.ActionID = A.ActionID
WHERE A.AccountID = 9 AND LastUpdatedBy IS NULL
SELECT * FROM ContactActionMap  WHERE ContactID= 142887
*/


		UPDATE Actions
		SET LastUpdatedBy = CreatedBy,
			LastUpdatedOn = CreatedOn
		WHERE AccountID = @AccountID AND LastUpdatedBy IS NULL


		UPDATE Actions_Audit
		SET LastUpdatedBy = CreatedBy,
			LastUpdatedOn = CreatedOn
		WHERE AccountID = @AccountID AND LastUpdatedBy IS NULL


		UPDATE ContactActionMap
		SET LastUpdatedBy = A.CreatedBy,
			LastUpdatedOn = A.CreatedOn
		FROM Actions A
			INNER JOIN ContactActionMap CAM ON CAM.ActionID = A.ActionID
		WHERE CAM.LastUpdatedBy IS NULL


		UPDATE ContactActionMap_Audit
		SET LastUpdatedBy = A.CreatedBy,
			LastUpdatedOn = A.CreatedOn
		FROM ContactActionMap_Audit CAMAA
			INNER JOIN Actions A ON CAMAA.ActionID = A.ActionID AND A.AccountID = @AccountID
			INNER JOIN ContactActionMap CAM ON CAM.ActionID = A.ActionID	
		WHERE CAMAA.LastUpdatedBy IS NULL

END

/*

  DBO.UPDATE_Actions_Tours_Lastupdatedby
   @AccountID = 1021 
 
*/

