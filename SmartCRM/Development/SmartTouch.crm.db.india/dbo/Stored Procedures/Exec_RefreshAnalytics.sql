CREATE PROCEDURE [dbo].[Exec_RefreshAnalytics]
AS
BEGIN
	DECLARE @RefreshAnalytics TABLE (ID INT IDENTITY(1,1), RefreshAnalyticsID INT , EntityID INT, EntityType TINYINT)
	DECLARE @Counter INT = 1

	INSERT INTO @RefreshAnalytics
	SELECT RefreshAnalyticsID, EntityID, EntityType FROM dbo.RefreshAnalytics (NOLOCK) WHERE [Status] = 1

	WHILE @Counter > 0
	BEGIN
		DECLARE @EntityID INT
		DECLARE @EntityType INT
		DECLARE @ID INT
		DECLARE @RefreshAnalyticsID INT
		SELECT @EntityID = EntityID, @EntityType = EntityType, @ID = ID, @RefreshAnalyticsID = RefreshAnalyticsID FROM @RefreshAnalytics WHERE ID = @Counter
		IF @EntityType = 2
		BEGIN
			EXEC dbo.Calc_CampaignAnalytics @EntityID
		END
		ELSE IF @EntityType = 6
		BEGIN
			EXEC dbo.Calc_WorkflowAnalytics @EntityID
		ENd

		UPDATE dbo.RefreshAnalytics SET [Status] = 2, LastModifiedOn = GETUTCDATE() WHERE RefreshAnalyticsID = @RefreshAnalyticsID

		SET @Counter = 0
		SELECT @Counter = ID FROM @RefreshAnalytics WHERE ID = (@ID + 1)
	END
	
END


