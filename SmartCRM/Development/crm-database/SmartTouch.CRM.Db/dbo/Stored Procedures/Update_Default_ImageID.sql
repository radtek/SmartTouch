

/*
		Purpose		: Update the ImageID
		Input		: FriendlyName, StorageName, OriginalName
		Output		: Update the ImageID
		Created By	: Krishna
		Created On	: Jan 21, 2015
		Modified On	: 
*/


CREATE PROC [dbo].[Update_Default_ImageID]
(
   @FriendlyName   VARCHAR(50),
   @StorageName    VARCHAR(50),
   @OriginalName   VARCHAR(50)
)
AS
BEGIN
DECLARE @imageid        int  

INSERT INTO [dbo].[Images]([FriendlyName],[StorageName],[OriginalName],[CreatedBy],[CreatedDate],[ImageCategoryID],[AccountID])
SELECT @FriendlyName, @StorageName, @OriginalName, 1, GETUTCDATE(), 2, NULL

SET @imageid  = (SELECT ImageID FROM dbo.Images WHERE FriendlyName = @FriendlyName AND AccountID IS NULL)

UPDATE CampaignTemplates
SET ThumbnailImage = @imageid
WHERE Name = @FriendlyName

END


/*
   EXEC [dbo].[Update_Default_ImageID]
       @FriendlyName = 'Layout 4',
       @StorageName  = 'layout4.png',
       @OriginalName = 'layout4.png'

*/


