-- =============================================
-- Author:		<Author,,SurendraBabu>
-- Create date: <Create Date,,10/25/17>
-- Description:	<Description,,Getting ContactIDs For Activity Report Drildown for Contacts,Notes,Tours>
-- =============================================
CREATE PROCEDURE [dbo].[GettingDrildownModuleContactIds] 
	@AccountId INT,
	@Users dbo.Contact_List readonly,
	@ModuleId tinyint,
	@StartDate datetime,
	@EndDate datetime
AS
BEGIN
		
	   CREATE  TABLE  #NoteActivites
	   (             
              EntityId INT
       )

	   CREATE  TABLE  #TourActivites
	   (             
              EntityId INT
       )

		IF(@ModuleId = 3)
		BEGIN
			;with CTE AS(
			SELECT EntityID  FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=4 AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate)

			SELECT DISTINCT EntityID FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=1 AND UAL.UserID NOT IN ( SELECT EntityID FROM CTE) 
			AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate
		END
		ELSE IF(@ModuleId = 6)
		BEGIN
			;with CTE AS(
			SELECT  EntityID  FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=4 AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate)

			INSERT INTO  #NoteActivites 
			SELECT EntityID FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=1 AND UAL.UserID NOT IN ( SELECT EntityID FROM CTE) 
			AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate

			SELECT DISTINCT ContactID FROM ContactNoteMap(NOLOCK) CNM
			JOIN #NoteActivites TM ON TM.EntityID=CNM.NoteID

		END
		ELSE 
		BEGIN
			
			;WITH CTE AS(
			SELECT  EntityID FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=4 AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate)

			INSERT INTO #TourActivites
			SELECT  EntityID  FROM UserActivityLogs (NOLOCK) UAL
			JOIN @Users U ON U.ContactID=UAL.UserID
			WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=1 AND UAL.UserID NOT IN ( SELECT EntityID FROM CTE) 
			AND UAL.ModuleID =@ModuleId AND LogDate BETWEEN @StartDate AND @EndDate

			SELECT DISTINCT ContactID FROM ContactTourMap(NOLOCK) CTM
			JOIN #TourActivites TM ON TM.EntityID=CTM.TourID

		END

END

/*


DECLARE @Users dbo.Contact_List

INSERT INTO @Users
SELECT 6899

EXEC [dbo].[GettingDrildownModuleContactIds] 4218,@Users,6,'07/25/2017 2:16:25 PM','10/26/2017 2:16:25 PM'

 */
