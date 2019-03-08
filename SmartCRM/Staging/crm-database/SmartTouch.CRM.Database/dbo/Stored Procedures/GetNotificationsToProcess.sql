-- =============================================
-- Author:		<Author,,SurendraBabu>
-- Create date: <Create Date,,12/13/17>
-- Description:	<Description,,Get Notifications to send mobile for Mobile APP>
-- =============================================
CREATE  PROCEDURE [dbo].[GetNotificationsToProcess]
@UserID int,
@NotificationLmit int
AS
BEGIN
		DECLARE @notificationCount int

		SELECT @notificationCount=COUNT(1) FROM Notifications(NOLOCK) 
		WHERE UserID=@UserID AND PushNotificationStatusID=1 

		IF(@notificationCount > @NotificationLmit)
		BEGIN
				;WITH CTE AS
				(
					SELECT TOP (@NotificationLmit) * FROM Notifications(NOLOCK) 
					WHERE UserID=@UserID AND PushNotificationStatusID=1 
					ORDER BY NotificationTime DESC
				)

				SELECT * INTO #TEMPNotification FROM CTE

				UPDATE N
				SET 
				N.PushNotificationStatusID=4
				FROM Notifications N where N.UserID=@UserID AND N.PushNotificationStatusID=1 AND N.NotificationID NOT IN ( SELECT NotificationID FROM #TEMPNotification)

				SELECT * FROM #TEMPNotification
		END
		ELSE
		BEGIN
				SELECT * FROM Notifications(NOLOCK) 
				WHERE UserID=@UserID AND PushNotificationStatusID=1 
				ORDER BY NotificationTime DESC
		END
END
