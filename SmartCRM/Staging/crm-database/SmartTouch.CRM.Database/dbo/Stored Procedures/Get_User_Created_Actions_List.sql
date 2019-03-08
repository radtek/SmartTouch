
 
 
 
CREATE PROCEDURE [dbo].[Get_User_Created_Actions_List]
	-- Add the parameters for the stored procedure here
	@AccountId int,
	@Users dbo.Contact_List readonly,
	@PageNumber INT,
	@PageSize INT,
	@SortColumn Nvarchar(20),
	@SortDirection varchar(4),
	@SearchBy VARCHAR(100),
	@Filter1ForACCompleted TINYINT,
	@Filter2ForACType SMALLINT,
	@StartDate DATETIME,
	@EndDate DATETIME
AS
BEGIN

		--DECLARE @ResultID INT
		--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		--VALUES('Get_User_Created_Actions_List', @AccountID, 
		--'Prarams: PageNumber:'+cast(@PageNumber as varchar(10)) +
		--', PageSize: ' + cast(@PageSize as varchar(10))+
		--', SortColumn :' + cast(@SortColumn as varchar(10)) +
		--', Search By: ' +@SearchBy +
		--', Filter1: '+  cast(@Filter1ForACCompleted as varchar(10)) +
		--', Filter2: '+ cast(@Filter2ForACType as varchar(10)) + 
		--', StartDate : '+ cast(@StartDate as varchar(20)) +
		--', EnDDate: '+ cast(@EndDate as varchar(20)) +
		--', SortDirection: ' + @SortDirection )
		
		--SET @ResultID = scope_identity()

		IF @SortColumn = '' OR @SortColumn = 'ActionTypeValue'
			BEGIN
				SET @SortColumn ='ActionDate'
			END

		;WITH ActionCompleted AS 
		(
			SELECT A.ActionID, CAM.IsCompleted, COUNT(1) CNT , MAX(CAM.ContactID) ContactID  ,A.ActionType
			FROM Actions (NOLOCK) A
			INNER JOIN ContactActionMap (NOLOCK) CAM ON CAM.ActionID = A.ActionID
			INNER JOIN Contacts C  WITH (nolock) on CAM.ContactID = C.ContactID AND  C.AccountID = A.AccountID
			INNER JOIN UserActionMap UAM (NOLOCK) ON UAM.ActionID = A.ActionID
			INNER JOIN @Users U ON U.ContactID=UAM.UserID
			WHERE A.AccountID = @AccountId AND C.IsDeleted = 0 AND A.ActionDetails LIKE '%'+COALESCE(@SearchBy,'')+'%' AND A.CreatedOn >= @StartDate AND A.CreatedOn <= @EndDate
			Group BY A.ActionID, CAM.IsCompleted , A.ActionType
		), UnFilteredActions AS (
		SELECT ActionID,ActionType,CASE WHEN COUNT(1) > 1 THEN 0 WHEN MAX(CONVERT(TINYINT, IsCompleted)) = 1 THEN 1 ELSE 0 END AS IsCompleted, MAX(ContactID) ContactId, SUM(CNT) TotalActionContacts
		FROM ActionCompleted
		GROUP BY ActionID,ActionType)

		SELECT * 
		INTO #temp
		FROM UnFilteredActions
		WHERE IsCompleted in (select datavalue from dbo.Split((CASE  WHEN @Filter1ForACCompleted = 0 THEN '0,1'
								                                     when @Filter1ForACCompleted=1 then '1'								  
																	 when @Filter1ForACCompleted=2 then '0' end ),',')) AND ActionType = IIF(@Filter2ForACType=0,ActionType,@Filter2ForACType )
	
		declare @SQL varchar(200),@PageNumber1 varchar(10)=Cast(@PageNumber as varchar(10)),
		@PageSize1 varchar(10)=Cast(@PageSize as varchar(10))


		CREATE TABLE #temp1 (Id INT IDENTITY(1,1), ActionId INT,ActionType SMALLINT ,IsComplete BIT, ContactId INT, TotalActionContacts INT)


		Set @SQL=' INSERT INTO #temp1 SELECT T.* FROM #temp T ' + 
		'INNER JOIN Actions (NOLOCK) A ON A.ActionID = T.ActionID'+' '+'
		 ORDER BY  '+ @SortColumn+' '+@SortDirection 

		Print @SQL
		EXEC (@SQL)

		DECLARE @tc INT

		SELECT @tc = COUNT(1) FROM #temp1

		SELECT t.Id,A.ActionId, A.ActionDetails, A.ActionDate, A.ActionStartTIme, A.ActionType AS ActionTypeId, T.IsComplete AS IsCompleted,A.CreatedOn, C.FirstName, C.LastName, C.Company, C.ContactType,T.TotalActionContacts,c.ContactID as ContactId,CE.Email , DDV.DropdownValue AS ActionTypeValue ,@tc TotalCount
		INTO #tempActions
		FROM Actions (NOLOCK) A
		INNER JOIN #temp1 T ON T.ActionID = A.ActionID
		INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = A.ActionType
		INNER JOIN Contacts C (NOLOCK) ON C.ContactID = T.ContactID AND C.AccountID = A.AccountID
		LEFT JOIN ContactEmails CE (NOLOCK) ON CE.ContactId = C.ContactID AND CE.AccountID = C.AccountID AND CE.IsDeleted = 0 AND CE.IsPrimary = 1
		ORDER BY t.Id ASC
		OFFSET ((@PageNumber-1)*@PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;

		DECLARE @count INT 
		DECLARE @counter INT = 1
				                                        
		SELECT @count = COUNT(1) from #tempActions
		CREATE TABLE #assignedTo (actionId INT, assignedTo VARCHAR(max))
		WHILE @counter<= @count
			BEGIN
				DECLARE @actionId int
				SELECT TOP(@counter) @actionId = actionId FROM #tempActions
		
				DECLARE @assignedTo VARCHAR(max)
						
				SELECT @assignedTo = coalesce(@assignedTo+',','') + u.FirstName + ' ' + u.LastName
				FROM UserActionMap (NOLOCK) uam
				INNER JOIN Users u (NOLOCK) ON u.userid = uam.userid
				WHERE uam.actionid = @actionId

				INSERT INTO #assignedTo
				SELECT @actionId, @assignedTo

				SET @counter = @counter + 1
				SET @assignedTo = NULL
			END

		SELECT ta.*, ast.assignedTo AS UserName FROM #tempActions ta 
		INNER JOIN #assignedTo ast ON ast.actionId = ta.actionId
		ORDER BY ta.Id ASC

		DROP TABLE #temp
		DROP TABLE #temp1
		DROP TABLE #tempActions
		DROP TABLE #assignedTo

--		UPDATE	StoreProcExecutionResults
--		SET		EndTime = GETDATE(),
--				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
--				Status = 'C'
--		WHERE	ResultID = @ResultID
END

/*
	EXEC [dbo].[Get_User_Created_Actions_List] 4218,6889,0,10,'ActionDate','DESC','',0,0,'1/1/1753 12:00:00 AM','12/31/9999 11:59:59 PM'

	SELECT TOP 1 * FROM ACTIONS(NOLOCK)
 */

