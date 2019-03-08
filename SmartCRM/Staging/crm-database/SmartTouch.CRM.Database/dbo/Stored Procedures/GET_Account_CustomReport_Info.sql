
CREATE PROCEDURE [dbo].[GET_Account_CustomReport_Info](
	@AccountID int,
	@ReportID int,
	@StartDate datetime,
	@EndDate datetime
)
AS
BEGIN
	DECLARE @SearchCriteria nvarchar(4000),
			@ColumnList nvarchar(2500),
			@TableList nvarchar(2500),
			@SQL nvarchar(MAX)

	SELECT	@SearchCriteria = SearchCriteria, @ColumnList = ColumnList, @TableList = TableList
	FROM	AccountCustomReports(NOLOCK)
	WHERE	AccountID = @AccountID
			and ReportID = @ReportID

	IF( LEN(ISNULL(@SearchCriteria, '')) > 0 )
	BEGIN
		SET @SQL = 'SELECT ' + @ColumnList + ' FROM ' + @TableList + ' WHERE ' + @SearchCriteria
		EXEC sp_executesql @SQL, N'@AccountID int, @StartDate datetime, @EndDate datetime', @AccountID, @StartDate, @EndDate
	END
END