-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE dbo.ScheduleRefreshAnalytics
	@EntityType INT,
	@EntityID INT,
	@Status INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO dbo.RefreshAnalytics
	VALUES (@EntityID, @EntityType, @Status, GETUTCDATE())
END
