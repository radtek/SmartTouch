CREATE PROC [dbo].[GetNotifications]
(
	@UserIds VARCHAR(MAX),
	@Impending BIT,
	@IsToday BIT,
	@Modules VARCHAR(MAX),
	@NotificationStatus TINYINT,
	@CountsOnly BIT = 0
)
AS
BEGIN
	DECLARE @fromDate DATETIME
	DECLARE @toDate DATETIME
	IF @NotificationStatus = 1
		BEGIN
			IF @Impending = 1
				BEGIN
					SET @fromDate = DATEADD(MINUTE,-2,GETUTCDATE())
					SET @toDate = GETUTCDATE()
				END
			ELSE
				BEGIN
					IF @IsToday = 1
						BEGIN
							SET @fromDate = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
							SET @toDate = GETUTCDATE()
						END
					ELSE
						BEGIN
							SET @fromDate = '1/1/1753 12:00:00 AM'
							SET @toDate = DATEADD(dd, -1, DATEDIFF(dd, 0, GETUTCDATE()))
						END
				END

		END
	ELSE
		BEGIN
					IF @IsToday = 1
						BEGIN
							SET @fromDate = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
							SET @toDate = GETUTCDATE()
						END
					ELSE
						BEGIN
							SET @fromDate = '1/1/1753 12:00:00 AM'
							SET @toDate = DATEADD(dd, 0, DATEDIFF(dd, 0, GETUTCDATE()))
						END
		END

		IF @CountsOnly =1 
			BEGIN
				SET @NotificationStatus = 1
				SET @fromDate = '1/1/1753 12:00:00 AM'
				SET @toDate = DATEADD(dd, 1, DATEDIFF(dd, 0, GETUTCDATE()))
			END

		;WITH NotificationsCTE AS (
		--Tours Module : 7
		--Tours
		SELECT T.TourID NotificationID, ISNULL(T.TourDetails,'Tour Reminder.') AS Details, T.ReminderDate AS [Time], ISNULL(T.TourDetails,'Tour Reminder.') AS [Subject], 2 Source, T.TourId AS EntityID, @notificationStatus as [Status], 7 ModuleID, T.CreatedBy AS UserId, '' AS DownloadFile,-- FROM Tours (NOLOCK) T
		C.ContactID AS ContactId, C.ContactType, C.FirstName+' '+ C.LastName as FullName, C.Company, C.IsDeleted FROM Tours (NOLOCK) T
		LEFT JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourID = T.TourID
		LEFT JOIN Contacts C (NOLOCK) ON C.ContactID = CTM.ContactID
		WHERE T.RemindbyPopup = 1 
			AND T.NotificationStatus = @NotificationStatus 
			AND T.ReminderDate BETWEEN @fromDate AND @toDate 
			AND T.CreatedBy IN (SELECT DataValue FROM dbo.Split(@UserIds,','))
			AND (SELECT COUNT(1) FROM dbo.Split(@Modules,',') WHERE DataValue = 7) = 1
		UNION
		-- Actions Module : 5
		--Actions
		SELECT A.ActionID NotificationID, A.ActionDetails as Details, A.RemindOn AS [Time], A.ActionDetails AS [Subject], 1 as [Source], A.ActionID AS EntityID, @notificationStatus as [Status],5 as ModuleID, A.CreatedBy, '' AS DownloadFile,
		C.ContactID, C.ContactType, C.FirstName+' '+ C.LastName as FullName, C.Company, C.IsDeleted FROM Actions (NOLOCK) A
		LEFT JOIN [dbo].[ContactActionMap](NOLOCK) CAM ON CAM.ActionID = A.ActionID
        LEFT JOIN Contacts(NOLOCK) C ON C.ContactID = CAM.ContactID
		WHERE A.RemindbyPopup = 1 
			AND A.NotificationStatus = @NotificationStatus 
			AND A.RemindOn BETWEEN @fromDate AND @toDate 
			AND A.CreatedBy IN (SELECT DataValue FROM dbo.Split(@UserIds,','))
			--AND (CAM.IsCompleted IS NULL OR CAM.IsCompleted = 0)
			AND (SELECT COUNT(1) FROM dbo.Split(@Modules,',') WHERE DataValue = 5) = 1
        UNION                            
		-- Other modules !35
		SELECT N.NotificationID, N.Details, N.NotificationTime AS [Time], N.Subject as [Subject], CASE WHEN N.ModuleID = 77 THEN 77 ELSE 10 END AS [Source], N.EntityID AS EntityID, N.Status as [Status], N.ModuleID, N.UserID, N.DownloadFile,
		C.ContactID, C.ContactType, C.FirstName+' '+ C.LastName as FullName,  C.Company, C.IsDeleted FROM Notifications(NOLOCK) N
		LEFT JOIN Contacts C (NOLOCK) ON C.ContactID = N.EntityID
		WHERE N.[Status] = @notificationStatus 
		AND N.NotificationTime BETWEEN @fromDate AND @toDate 
		AND N.ModuleID IN (SELECT DataValue FROM dbo.Split(@Modules,',')) AND N.ModuleID NOT IN (16, 35 )
		AND N.UserID IN (SELECT DataValue FROM dbo.Split(@UserIds,','))
		UNION
		SELECT N.NotificationID, N.Details, N.NotificationTime AS [Time], N.Subject as [Subject], 10 AS [Source], N.EntityID AS EntityID, N.Status as [Status], N.ModuleID, N.UserID, '' AS DownloadFile,
		O.OpportunityID AS ContactID, 0, O.OpportunityName as FullName,  '' , 0 FROM Notifications(NOLOCK) N
		INNER JOIN Opportunities O (NOLOCK) ON O.OpportunityID = N.EntityID
		WHERE N.[Status] = @notificationStatus 
		AND N.NotificationTime BETWEEN @fromDate AND @toDate 
		AND N.ModuleID IN (SELECT DataValue FROM dbo.Split(@Modules,',')) AND N.ModuleID = 16
		AND N.UserID IN (SELECT DataValue FROM dbo.Split(@UserIds,','))
		)

		SELECT * 
		INTO #notifications
		FROM NotificationsCTE

		IF @CountsOnly = 0
			BEGIN
				SELECT * FROM #notifications
				ORDER BY Time DESC
			END
		ELSE
			BEGIN
				SELECT ModuleID,COUNT(DISTINCT NotificationID) [NotificationCount], 1 IsToday FROM #notifications
				WHERE [Time] BETWEEN CONVERT(DATE, GETUTCDATE()) AND GETUTCDATE()
				GROUP BY ModuleID
				UNION
				SELECT ModuleID,COUNT(DISTINCT NotificationID) [NotificationCount], 0 IsToday FROM #notifications
				WHERE [Time] BETWEEN '1/1/1753 12:00:00 AM' AND CONVERT(DATE, GETUTCDATE())
				GROUP BY ModuleID
			END
END

