-- =============================================
-- Author:		<Author,,SurendraBabu Vadalisetty>
-- Create date: <Create Date,,31/05/17>
-- Description:	<Description,,Getting Never Bounce Email validation done Files>
-- =============================================
CREATE PROCEDURE [dbo].[Getting_NeverBounce_Validation_Completed_Files]
     @NeverBounceRequestId INT
AS
BEGIN

		DECLARE @FileNames NVARCHAR(MAX)

		SELECT EntityID,NeverBounceEntityType
		INTO #TempNeverBounceRequests
		FROM NeverBounceMappings (NOLOCK)
		WHERE NeverBounceRequestID=@NeverBounceRequestId

		IF EXISTS(SELECT 1 FROM #TempNeverBounceRequests WHERE NeverBounceEntityType = 1) --- FOR IMPORTS
		BEGIN
				SELECT @FileNames = LJL.[FileName] FROM LeadAdapterJobLogs (NOLOCK) LJL
				JOIN #TempNeverBounceRequests TNR ON TNR.EntityID=LJL.LeadAdapterJobLogID
		END
		ELSE IF EXISTS(SELECT 1 FROM #TempNeverBounceRequests WHERE NeverBounceEntityType = 2) -- FOR TAGS
		BEGIN
				SELECT @FileNames = COALESCE(@FileNames+',','') + T.TagName FROM Tags (NOLOCK) T
				JOIN #TempNeverBounceRequests TNR ON TNR.EntityID = T.TagID
		END
		ELSE
		BEGIN
				SELECT @FileNames = COALESCE(@FileNames+',','') + SD.SearchDefinitionName FROM SearchDefinitions (NOLOCK) SD
				JOIN #TempNeverBounceRequests TNR ON TNR.EntityID = SD.SearchDefinitionID
		END

		SELECT @FileNames


END

/* 
EXEC [dbo].[Getting_NeverBounce_Validation_Completed_Files] 10
*/