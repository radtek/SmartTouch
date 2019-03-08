CREATE PROCEDURE [dbo].[MarketingMessageActivator]
AS
BEGIN

	DECLARE @inactive TABLE (id INT)
	DECLARE @active TABLE (id INT)

	-- all messages where schedule to < utc should be in inactive
	INSERT INTO @inactive
	SELECT MM.MarketingMessageID FROM MarketingMessages (nolock) MM
	WHERE MM.[Status] = 1002 AND MM.IsDeleted = 0 AND MM.ScheduleTo < GETUTCDATE()
	ORDER BY MM.ScheduleFrom DESC


	-- all messages where schedule from < utc and schedule to > utc should be active
	INSERT INTO @active
	SELECT MM.MarketingMessageID FROM MarketingMessages (nolock) MM
	WHERE MM.[Status] = 1001 AND MM.IsDeleted = 0 AND 
	((MM.ScheduleFrom IS NOT NULL AND MM.ScheduleFrom < GETUTCDATE()) OR (MM.ScheduleFrom IS NULL)) AND 
	((MM.ScheduleTo IS NOT NULL AND MM.ScheduleTo > GETUTCDATE()) OR (MM.ScheduleTo IS NULL))
	ORDER BY MM.ScheduleFrom DESC

	UPDATE MarketingMessages
	SET [Status] = 1003
	WHERE MarketingMessageID IN (SELECT * FROM @inactive)


	UPDATE MarketingMessages
	SET [Status] = 1002
	WHERE MarketingMessageID IN (SELECT * FROM @active)

END
