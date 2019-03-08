
CREATE PROCEDURE  [dbo].[GetTagsReport]
@AccountId INT,
@UserId INT = 0,
@TagName NVARCHAR(200),
@SortingOrder BIT,
@SortField NVARCHAR(200),
@PageNumber INT,
@PageSize SMALLINT

AS
BEGIN
	
	DECLARE @SQL NVARCHAR(1000)
	DECLARE @SQLCOUNT 	NVARCHAR(1000)
	
	-- Getting Tags Report --
	BEGIN
		
		SET @SQL = 'SELECT VT.TagID AS Id, VT.TagName, COUNT(1) AS [Count], COUNT(1) OVER() as TotalTagCount FROM vTags (NOLOCK) VT
						INNER JOIN ContactTagMap (NOLOCK) CTM ON VT.TagID = CTM.TagID
						INNER JOIN CONTACTS (NOLOCK) C ON CTM.ContactID = C.ContactID AND C.AccountID = CTM.AccountID AND VT.AccountID = C.AccountID
					WHERE
						C.IsDeleted = 0 AND VT.IsDeleted = 0 AND VT.AccountID = ' + CAST(@AccountId AS VARCHAR(10))
		
		IF(@TagName <> '')
			SET @SQL = @SQL + ' AND VT.TAGNAME LIKE ''%'+@TagName+'%''';

		IF(@UserId > 0)
			SET @SQL = @SQL + ' AND C.OwnerID = ' + CAST(@UserId AS VARCHAR(10));

		SET @SQL = @SQL + 	
						' GROUP BY VT.TAGID, VT.TAGNAME'+
						' ORDER BY ' + @SortField  + CASE WHEN @SortingOrder = 0 THEN ' ASC ' WHEN @SortingOrder = 1 THEN ' DESC ' END 
		
		IF(@PageNumber <> 0)
			SET @SQL = @SQL + 	
						' OFFSET ' + CAST(((@PageNumber - 1) * @PageSize) AS VARCHAR(5)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS VARCHAR(5)) +' ROWS ONLY'
		
		EXEC(@SQL)
	END	
		SELECT @SQL AS TagsQuery			
END


-- exec [GetTagsReport] 4218,0,'a',0,'TagName',1,10