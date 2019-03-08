CREATE Procedure [dbo].[GET_BouncedEmailReport]
(
  @StartDate DATETIME ,
  @EndDate DATETIME ,
  @AccountIdS VARCHAR(100),
  @Take INT, @Skip INT
)
WITH RECOMPILE

AS

BEGIN 

	

	DROP TABLE  IF EXISTS #T

	CREATE TABLE #T (AccountID INT)

	INSERT INTO #T (AccountID)
	SELECT Datavalue  FROM dbo.Split (@AccountIdS ,',')

	DECLARE @TotalCount varchar(max) 
	SET  @TotalCount = 
	(select  Count(1)
		 from  BouncedEmailData BED WITH (NOLOCK)
		 INNER JOIN #t AA  ON BED.AccountID= AA.AccountID
		 WHERE [SentOn] BETWEEN @StartDate AND @EndDate and BED.AccountId = Aa.AccountID)


		 declare @Take1 varchar(max)=Cast(@Take as varchar(max)),
		@Skip1 varchar(max)=Cast(@Skip as varchar(max))

		

	DECLARE @strQuery As VARCHAR(MAX)

     SET @strQuery = 'SELECT  BED.Email, AccountName AS Account, S.Name AS BounceType ,[SentOn] as [Date],'+@TotalCount+' AS TotalCount,BouncedReason
		FROM BouncedEmailData BED WITH (NOLOCK)
		INNER JOIN Accounts A WITH (NOLOCK) ON A.AccountID = BED.AccountId
		INNER JOIN [dbo].[Statuses] S WITH (NOLOCK) ON S.StatusID = BED.StatusID
		INNER JOIN #T AA  ON A.AccountID= AA.AccountID
		WHERE [SentOn] BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
		ORDER BY [SentOn] DESC
		OFFSET '+@Skip1+' ROWS
		FETCH NEXT '+@Take1+' ROWS ONLY
		'

     EXECUTE (@strQuery) 
	
END

--EXEC [dbo].[GET_BouncedEmailReport] '2016-05-09 09:26:21.170', '2017-05-09 09:26:21.170', '339', 10, 0
GO

