
CREATE  PROC  [dbo].[GET_Account_Activity_Report_Contacts]
(
@AccountID INT ,
@StartDate DATETIME ,
@EndDate  DATETIME ,
@UserIDs NVARCHAR(MAX),
@ModuleIDs NVARCHAR(MAX)
)
AS
BEGIN
--select * from users where accountid =  339 and isdeleted=0 and status =  1
--/* Contacts 3 */
	;with CTE AS(
	SELECT EntityID  FROM UserActivityLogs (NOLOCK) UAL
	INNER JOIN dbo.Split(@UserIDs , ',')  U ON U.DataValue=UAL.UserID
	INNER JOIN dbo.Split(@ModuleIDs , ',') M ON M.DataValue=UAL.ModuleID
	WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=4 AND UAL.LogDate BETWEEN @StartDate AND @EndDate)

	SELECT DISTINCT EntityID as ContactID FROM UserActivityLogs (NOLOCK) UAL
	INNER JOIN dbo.Split(@UserIDs , ',')  U ON U.DataValue=UAL.UserID
	INNER JOIN dbo.Split(@ModuleIDs , ',') M ON M.DataValue=UAL.ModuleID
	WHERE UAL.AccountID=@AccountId AND UAL.UserActivityID=1 AND UAL.EntityID NOT IN ( SELECT EntityID FROM CTE) 
	AND UAL.LogDate BETWEEN @StartDate AND @EndDate
UNION
--/* Campaigns 4 */

SELECT DISTINCT CC.ContactID   fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN Campaigns C (NOLOCK) ON C.CampaignID = UA.EntityID
INNER JOIN CampaignRecipients CR (NOLOCK) ON CR.CampaignID = C.CampaignID AND CR.AccountId = C.AccountID
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue = 4
INNER JOIN Contacts CC ON CC.ContactID = CR.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
and  CR.SentOn BETWEEN @StartDate AND @EndDate
AND C.IsDeleted = 0  AND CC.IsDeleted = 0 

UNION
/* Actions 5 */

SELECT  DISTINCT CC.ContactID   fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN actions A (NOLOCK) ON A.ActionID = UA.EntityID
INNER JOIN ContactActionMap CA (NOLOCK) ON CA.ActionID = A.ActionID
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue = 5
INNER JOIN Contacts CC ON CC.ContactID = CA.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
AND CC.IsDeleted = 0 

UNION
----select * from modules  where moduleid in (4,5,6,7,10,16)

/* Notes  6 */

SELECT DISTINCT CC.ContactID  fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN Notes N (NOLOCK) ON N.NoteID = UA.EntityID
INNER JOIN ContactNoteMap CN (NOLOCK) ON CN.NoteID = n.NoteID 
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue = 6
INNER JOIN Contacts CC ON CC.ContactID = CN.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
AND CC.IsDeleted = 0 

UNION

/* Tours   7 */


SELECT DISTINCT CC.ContactID fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN Tours T (NOLOCK) ON T.TourID = UA.EntityID
INNER JOIN ContactTourMap CT (NOLOCK) ON CT.TourID = T.TourID 
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue =7
INNER JOIN Contacts CC ON CC.ContactID = CT.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
AND CC.IsDeleted = 0 


UNION

/* Forms  10 */

SELECT DISTINCT CC.ContactID  fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN Forms F (NOLOCK) ON F.FormID = UA.EntityID
INNER JOIN FormSubmissions FS (NOLOCK) ON FS.FormID = F.FormID 
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue =10
INNER JOIN Contacts CC ON CC.ContactID = FS.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
AND  FS.SubmittedOn BETWEEN @StartDate AND @EndDate
AND CC.IsDeleted = 0 

UNION

--SELECT  TOP 1 * FROM UserActivityLogs (NOLOCK) WHERE  AccountID = @AccountID AND  LogDate BETWEEN @StartDate AND @EndDate  AND MODULEID = 16

/*  Opportunity 16 */


SELECT DISTINCT CC.ContactID  fROM  UserActivityLogs UA (NOLOCK)
INNER JOIN Opportunities O (NOLOCK) ON O.OpportunityID = UA.EntityID
INNER JOIN OpportunityContactMap OC (NOLOCK) ON OC.OpportunityID = O.OpportunityID 
INNER JOIN dbo.Split(@ModuleIDs , ',') S ON S.DataValue =16
INNER JOIN Contacts CC ON CC.ContactID = OC.ContactID
WHERE UA.AccountID = @AccountID AND UA.LogDate BETWEEN @StartDate AND @EndDate 
AND UA.UserID IN (SELECT DataValue FROM dbo.Split( @UserIDs ,','))
AND CC.IsDeleted = 0 

END
