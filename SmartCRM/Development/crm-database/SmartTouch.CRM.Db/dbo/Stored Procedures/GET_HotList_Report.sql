create PROCEDURE [dbo].[GET_HotList_Report](
       @ToDate DATETIME ,
        @FromDate DATETIME ,
        @OwnerID VARCHAR(max),
        @AccountID INT,
        @IsAdmin BIT,
        @LifeCycleStage VARCHAR(max),
        @LeadSources VARCHAR(max),
        @StartNo INT,
        @EndNo INT,
		@IsDashboard BIT
)
AS 


	CREATE TABLE #HotListTemporaryTable	(	[ContactId] [int]  NULL,	[FullName] [nvarchar](1000)  NULL,	[AccountExec] [nvarchar](1000)  NULL,	[LifeCycleStageId] [SmallInt]  NULL,	[LeadScore] [INT]  NULL,	[PhoneNumber] [nvarchar](1000)  NULL,
	                       [Email] [nvarchar](1000)  NULL,	[EmailStatus] [SmallInt]  NULL,	[LeadSourceId] [SmallInt]  NULL,	[RowNum] [INT]  NULL,LeadSource [nvarchar](1000)  NULL,Lifecycle [nvarchar](1000)  NULL,ContactEmailId [INT]  NULL,
						   ContacPhoneNumberID [INT]  NULL,OwnerID [INT]  NULL
						   )

	DECLARE @LeadScores TABLE (ContactID INT, LeadScore  INT)

--        @FromDate DATETIME =  DATEADD(dd,-30,GETUTCDATE()),
--        @OwnerID VARCHAR(max),
--        @AccountID INT,
--        @IsAdmin BIT,
--        @LifeCycleStage VARCHAR(max),
--        @LeadSources VARCHAR(max),
--        @StartNo INT = 1,
  --      @EndNo INT = 500
 
 BEGIN 
    BEGIN TRY
   
--select @LifeCycleStage = coalesce(@LifeCycleStage +',' ,'') + convert(varchar(10), dropdownvalueid) from dropdownvalues (nolock) where dropdownid = 3 and isactive = 1 and isdeleted = 0 and accountid = @accountid
--select @LeadSources = coalesce(@LeadSources +',' ,'') + convert(varchar(10), dropdownvalueid) from dropdownvalues (nolock) where dropdownid = 5 and isactive = 1 and isdeleted = 0 and accountid = @accountid
--select @OwnerID = coalesce(@OwnerID+',','') + convert (varchar(10),UserID) from Users (nolock) where accountid = @accountid and isdeleted = 0 and status = 1

	--IF @IsAdmin = 1
	--BEGIN
	--	INSERT INTO @LeadScores
	--	SELECT LS.ContactID, SUM(LS.Score) FROM Contacts (NOLOCK) C
	--	INNER JOIN LeadScores (NOLOCK) LS ON LS.ContactID = C.ContactID
	--	WHERE LS.Score > 0 AND C.AccountID = @AccountId AND @AccountID != 2105
	--		AND C.LifecycleStage IN (SELECT datavalue FROM dbo.split_2(@LifeCycleStage,','))
	--		AND C.LastUpdatedOn BETWEEN @FROMDATE AND @TODATE AND C.IsDeleted = 0
	--		GROUP BY LS.ContactID
	--END
	--ELSE 
	--BEGIN
		INSERT INTO @LeadScores
		SELECT LS.ContactID, SUM(LS.Score) FROM Contacts (NOLOCK) C
		INNER JOIN LeadScores (NOLOCK) LS ON LS.ContactID = C.ContactID
		WHERE LS.Score > 0 AND C.AccountID = @AccountId AND @AccountID != 2105
			AND C.OwnerID IN (SELECT datavalue FROM dbo.split_2(@OwnerID,','))
			AND C.LifecycleStage IN (SELECT datavalue FROM dbo.split_2(@LifeCycleStage,','))
			AND C.LastUpdatedOn BETWEEN @FROMDATE AND @TODATE AND C.IsDeleted = 0
			GROUP BY LS.ContactID
	--END
		
		INSERT INTO #HotListTemporaryTable(ContactId,FullName,AccountExec,LifeCycleStageId,LeadScore,PhoneNumber,Email,EmailStatus,LeadSourceId,RowNum, OwnerID)
			SELECT C.ContactID, COALESCE(C.FirstName,'') + ' ' + COALESCE(C.LastName,'') AS FullName
				   ,COALESCE(U.FirstName,'') + ' ' + COALESCE(U.LastName,'') AS AccountExec, C.LifecycleStage LifeCycleStageId, C.LeadScore, CP.PhoneNumber, CM.Email,CM.EmailStatus ,LSM.LeadSouceID LeadSourceId
				   ,Row_Number()  OVER (ORDER BY  COALESCE(LS.LeadScore,0) DESC, C.FirstName, C.LastName) AS RowNum, C.OwnerID
			FROM Contacts C (NOLOCK)
				   INNER JOIN ContactLeadSourceMap LSM (NOLOCK) ON C.ContactId = LSM.ContactId AND LSM.IsPrimaryLeadSource = 1
				   INNER JOIN @LeadScores LS ON LS.ContactID = C.ContactID
				   LEFT JOIN Users U (NOLOCK) ON C.OwnerID = U.UserID
				   LEFT JOIN ContactPhoneNumbers CP (NOLOCK) ON C.ContactID = CP.ContactID AND CP.IsPrimary = 1 AND CP.IsDeleted = 0
				   LEFT JOIN ContactEmails CM (NOLOCK) ON C.ContactID = CM.ContactID AND CM.IsPrimary = 1 AND CM.IsDeleted = 0
			
		SELECT DISTINCT S.ContactId, S.FullName, S.AccountExec, S.LifeCycleStageId, S.LeadScore, S.PhoneNumber, S.Email,S.EmailStatus, S.LeadSourceId, S.RowNum,s.LeadSource,s.Lifecycle,s.ContacPhoneNumberID,s.ContactEmailId,s.OwnerID
			   , SUM(CASE WHEN LS.AddedOn BETWEEN @FromDate AND @ToDate THEN LS.Score ELSE 0 END) AS NewPoints
		FROM  #HotListTemporaryTable S
			   LEFT JOIN LeadScores LS (NOLOCK) ON S.ContactID = LS.ContactID
		WHERE RowNum BETWEEN @StartNo AND @EndNo
		GROUP BY S.ContactId, S.FullName, S.AccountExec, S.LifeCycleStageId, S.LeadScore, S.PhoneNumber, S.Email,S.EmailStatus, S.LeadSourceId, S.RowNum,S.LeadSource,s.Lifecycle,s.ContacPhoneNumberID,s.ContactEmailId,s.OwnerID
		ORDER BY RowNum
		
		IF (@IsDashboard = 0)
		BEGIN
			SELECT DISTINCT ContactId FROM #HotListTemporaryTable
		END

	END TRY

	BEGIN CATCH
	--Unsuccessful execution query-- 
	  SELECT 'DEL-002' ResultCode 
	--Error blocking statement in between catch --
	  INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
	  VALUES (CONVERT(SYSNAME, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH

END 


/* Execution */
--DECLARE @LifeCycleStage VARCHAR(MAX)
--DECLARE @LeadSources VARCHAR(MAX)
--DECLARE @OwnerID VARCHAR(MAX)
--select @LifeCycleStage = coalesce(@LifeCycleStage +',' ,'') + convert(varchar(10), dropdownvalueid) from dropdownvalues (nolock) where dropdownid = 3 and isactive = 1 and isdeleted = 0 and accountid = @accountid
--select @LeadSources = coalesce(@LeadSources +',' ,'') + convert(varchar(10), dropdownvalueid) from dropdownvalues (nolock) where dropdownid = 5 and isactive = 1 and isdeleted = 0 and accountid = @accountid
--select @OwnerID = coalesce(@OwnerID+',','') + convert (varchar(10),UserID) from Users (nolock) where accountid = @accountid and isdeleted = 0 and status = 1

--exec [GET_HotList_Report]
--  @ToDate = '1/1/2016 12:00:00 AM',
--  @FromDate = '12/1/2015 12:00:00 AM',
--  @OwnerID = @OwnerID,
--  @AccountID = 4218,
--  @IsAdmin = 1,
--  @LifeCycleStage = @LifeCycleStage,
--  @LeadSources = @LeadSources,
--  @StartNo = 1,
--  @EndNo = 500

/**/

