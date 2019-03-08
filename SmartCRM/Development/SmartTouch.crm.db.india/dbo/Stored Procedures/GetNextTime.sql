
CREATE PROCEDURE [dbo].[GetNextTime]
(
	@WorkflowTimerActionID INT,
	@PreviousActionTime DATETIME
)
AS
BEGIN
	DECLARE
	@WorkflowActionID int,
	@TimerType int ,
	@DelayPeriod tinyint,
	@DelayUnit tinyint,
	@RunOn tinyint,
	@RunAt time,
	@RunType tinyint,
	@RunOnDate datetime,
	@StartDate datetime,
	@EndDate datetime,
	@RunOnDay  varchar(15),
	@DaysOfWeek varchar(15),
	@CurrentActionDateTime DATETIME =@PreviousActionTime
	DECLARE @BiggerDate INT

SELECT @WorkflowActionID	=	WorkflowActionID	,
		@TimerType	=	TimerType	,
		@DelayPeriod	=	DelayPeriod	,
		@DelayUnit	=	DelayUnit	,
		@RunOn	=	RunOn	,
		@RunAt	=	RunAt	,
		@RunType	=	RunType	,
		@RunOnDate	=	RunOnDate	,
		@StartDate	=	StartDate	,
		@EndDate	=	EndDate	,
		@RunOnDay	=	RunOnDay	,
		@DaysOfWeek	=	DaysOfWeek	
FROM WorkflowTimerActions (NOLOCK) WHERE WorkflowTimerActionID = @WorkflowTimerActionID
		IF @TimerType = 1 --TimeDelay
			BEGIN
				IF @DelayUnit = 1 --Years
					BEGIN
						SET @CurrentActionDateTime = DATEADD(YEAR, @DelayPeriod, @PreviousActionTime)
					END
				ELSE IF @DelayUnit = 2 --Months
					BEGIN
						SET @CurrentActionDateTime = DATEADD(MONTH, @DelayPeriod, @PreviousActionTime)
					END
				ELSE IF @DelayUnit = 3 --Weeks
					BEGIN
						SET @CurrentActionDateTime = DATEADD(WEEK, @DelayPeriod, @PreviousActionTime)
					END	
				ELSE IF @DelayUnit = 4 --Days
					BEGIN
						SET @CurrentActionDateTime = DATEADD(DAY, @DelayPeriod, @PreviousActionTime)
					END
				ELSE IF @DelayUnit = 5 --Hours
					BEGIN
						SET @CurrentActionDateTime = DATEADD(HOUR, @DelayPeriod, @PreviousActionTime)
					END
				ELSE IF @DelayUnit = 6 --Minutes
					BEGIN
						SET @CurrentActionDateTime = DATEADD(MINUTE, @DelayPeriod, @PreviousActionTime)
					END
				ELSE IF @DelayUnit = 7 --Seconds
					BEGIN
						SET @CurrentActionDateTime = DATEADD(SECOND, @DelayPeriod, @PreviousActionTime)
					END	
				IF @RunOn = 2-- AnyWeekDay
					BEGIN
						DECLARE @DatePart INT 
						SET @DatePart =  DATEPART(DW,@PreviousActionTime)
						IF @DatePart = 0 --Sunday
							BEGIN
								SET @DatePart = 1
							END
						ELSE IF @DatePart = 6 --Saturday
							BEGIN
								SET @DatePart = 2
							END
						ELSE 
							BEGIN
								SET @DatePart = 0
							END
						SET @CurrentActionDateTime = DATEADD(DAY, @DatePart, @PreviousActionTime)
					END

				SET @BiggerDate  = DATEDIFF(SECOND, @RunAt, CAST(@CurrentActionDateTime AS time))
				IF @BiggerDate > 0
					BEGIN
						SET @CurrentActionDateTime = DATEADD(SECOND, @BiggerDate, @PreviousActionTime)  
					END

			END
		ELSE IF @TimerType = 2 --Date
			BEGIN
				IF @RunType = 1 --OnADate
					BEGIN
						SET @BiggerDate  = DATEDIFF(ms,  @RunAt, CAST(@RunOnDate AS TIME))
						IF @BiggerDate > 0
							BEGIN
								SET @CurrentActionDateTime = DATEADD(ms, @BiggerDate, @RunOnDate)  
							END
						IF @RunOnDate > @PreviousActionTime
							SET @CurrentActionDateTime = @RunOnDate
					END
				ELSE IF @RunType = 2 --Between Dates
					BEGIN
						IF ((@EndDate < @PreviousActionTime) OR (@PreviousActionTime > @StartDate AND @PreviousActionTime < @EndDate) OR @PreviousActionTime < @StartDate)
							SET @CurrentActionDateTime = @PreviousActionTime
					ENd
			END
		ELSE IF @TimerType = 3 -- On a Week
			BEGIN
			    DECLARE @RunOnDays TABLE (RowID INT NOT NULL PRIMARY KEY IDENTITY(1,1), [ID] INT NOT NULL)
				DECLARE @dayOfWeek TINYINT
				SET @dayOfWeek = DATEPART(dw, @PreviousActionTime)
				IF @DaysOfWeek IS NOT NULL AND LEN(@DaysOfWeek) > 0
				BEGIN
					INSERT INTO @RunOnDays ([ID])
					SELECT CAST(DataValue AS INT) FROM dbo.Split_2(@DaysOfWeek, ',')
				END   
				IF EXISTS (SELECT [ID] FROM @RunOnDays WHERE ID = @dayOfWeek)          
					SET @CurrentActionDateTime = @PreviousActionTime
				ELSE
				BEGIN
					DECLARE @Days TABLE ( [Day] INT NOT NULL, DaysUntilDayOfWeek INT NOT NULL)
					DECLARE @RowsToProcess TINYINT, 
							@CurrentRow INT,
							@Day INT,
							@NewDate DATETIME,
							@MinDate INT
					SELECT @RowsToProcess = COUNT(*) FROM @RunOnDays
					SET @CurrentRow = 0
					WHILE @CurrentRow < @RowsToProcess
					BEGIN
						SET @CurrentRow = @CurrentRow + 1
						SELECT @Day = [ID] FROM @RunOnDays WHERE RowID = @CurrentRow
						INSERT INTO @Days
						SELECT @Day, (@Day - (@dayOfWeek - 1) + 7) % 7
					END
					SELECT TOP 1 @MinDate = DaysUntilDayOfWeek FROM @Days ORDER BY DaysUntilDayOfWeek ASC
					SET @CurrentActionDateTime = DATEADD(DAY, @MinDate, @PreviousActionTime)
				END
		     END

		SELECT @CurrentActionDateTime AS NextExecutionTime  
END

/*
EXEC  [dbo].[GetNextTime] 566,'2016-03-30 19:00:00.000'
select * from workflowtimeractions (nolock) where workflowtimeractionid=566
*/
GO


