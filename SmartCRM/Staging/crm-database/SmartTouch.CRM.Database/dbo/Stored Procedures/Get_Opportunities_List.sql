-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Get_Opportunities_List] 	
   @AccountId INT ,
   @pageNumber INT,
   @pageSize INT,
   @Query varchar(100)='',
   @SortField varchar(50),
   @Users dbo.Contact_List readonly,
   @StartDate DATETIME,
   @EndDate DATETIME,
   @SortDirection varchar(10)
AS
BEGIN
		DECLARE @OpportunitiesTemp TABLE(OpportunityID INT,AccountID INT,OpportunityName VARCHAR(75),OpportunityType VARCHAR(75),ProductType VARCHAR(75),Potential MONEY,LastModifiedOn DATETIME,ContactID INT,Name NVARCHAR(MAX),ContactType TINYINT,RowNumber INT,MAXRowNumber INT)
		IF NOT EXISTS (SELECT TOP 1 * FROM @Users)
		BEGIN
					INSERT INTO @OpportunitiesTemp
					SELECT * FROM (
					SELECT O.OpportunityID,O.AccountID,O.OpportunityName,O.OpportunityType,O.ProductType,O.Potential, O.LastModifiedOn,--COUNT(*) OVER(PARTITION BY O.OpportunityID) as TotalCount ,
										C.ContactID,
												CASE WHEN (LEN(C.FirstName) > 0 AND LEN(C.LastName) >0) THEN C.FirstName + ' ' + C.LastName
													 WHEN LEN(C.Company) > 0 THEN C.Company
													 ELSE C.Email	 END
												  AS Name,C.ContactType ,
												ROW_NUMBER() OVER(PARTITION BY o.OpportunityID ORDER BY  C.ExpectedToClose ASC) AS RowNumber,
												COUNT(*) OVER(PARTITION BY O.OpportunityID) AS MAXRowNumber
										 FROM Opportunities (NOLOCK) O
										 LEFT JOIN (SELECT C.ContactId, C.FirstName, C.LastName, CE.Email, C.Company, C.ContactType, OCM.OpportunityID,OCM.ExpectedToClose  FROM 
											OpportunityContactMap OCM (NOLOCK) 
											INNER JOIN Contacts (NOLOCK) C ON C.ContactID = OCM.ContactID AND OCM.IsDeleted = 0 AND C.AccountID = @AccountId AND C.IsDeleted=0
											LEFT JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = C.AccountID AND CE.IsDeleted = 0 AND CE.IsPrimary = 1) 
											C ON C.OpportunityID = O.OpportunityID
										WHERE O.AccountID=@AccountId AND O.IsDeleted=0 AND O.OpportunityName like '%' + ISNULL(@Query,'') +'%' 
					
										) x

					--SELECT O.OpportunityID,O.AccountID,O.OpportunityName,O.OpportunityType,O.ProductType,O.Potential, O.LastModifiedOn,--COUNT(*) OVER(PARTITION BY O.OpportunityID) as TotalCount ,
					--C.ContactID,
					--		CASE WHEN (LEN(C.FirstName) > 0 AND LEN(C.LastName) >0) THEN C.FirstName + ' ' + C.LastName
					--			 WHEN LEN(C.Company) > 0 THEN C.Company
					--			 ELSE CE.Email	 END
					--		  AS Name,C.ContactType ,
					--		ROW_NUMBER() OVER(PARTITION BY o.OpportunityID ORDER BY  OCM.ExpectedToClose ASC) AS RowNumber,
					--		COUNT(*) OVER(PARTITION BY O.OpportunityID) AS MAXRowNumber
					-- FROM Opportunities (NOLOCK) O
					--LEFT JOIN OpportunityContactMap OCM (NOLOCK) ON OCM.OpportunityID = O.OpportunityID AND OCM.IsDeleted=0
					--LEFT JOIN Contacts (NOLOCK) C ON C.ContactID = OCM.ContactID AND C.IsDeleted = 0 AND C.AccountID = @AccountId
					--LEFT JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = C.AccountID AND CE.IsDeleted = 0 AND CE.IsPrimary = 1
					--WHERE O.AccountID=@AccountId AND O.IsDeleted=0 AND O.OpportunityName like '%' + ISNULL(@Query,'') +'%' 
		END
		ELSE
		BEGIN
					INSERT INTO @OpportunitiesTemp
									SELECT * FROM (
					SELECT O.OpportunityID,O.AccountID,O.OpportunityName,O.OpportunityType,O.ProductType,O.Potential, O.LastModifiedOn,--COUNT(*) OVER(PARTITION BY O.OpportunityID) as TotalCount ,
										C.ContactID,
												CASE WHEN (LEN(C.FirstName) > 0 AND LEN(C.LastName) >0) THEN C.FirstName + ' ' + C.LastName
													 WHEN LEN(C.Company) > 0 THEN C.Company
													 ELSE C.Email	 END
												  AS Name,C.ContactType ,
												ROW_NUMBER() OVER(PARTITION BY o.OpportunityID ORDER BY  C.ExpectedToClose ASC) AS RowNumber,
												COUNT(*) OVER(PARTITION BY O.OpportunityID) AS MAXRowNumber
										 FROM Opportunities (NOLOCK) O
										 INNER JOIN @Users U ON U.ContactID=O.CreatedBy
										 LEFT JOIN (SELECT C.ContactId, C.FirstName, C.LastName, CE.Email, C.Company, C.ContactType, OCM.OpportunityID,OCM.ExpectedToClose  FROM 
											OpportunityContactMap OCM (NOLOCK) 
											INNER JOIN Contacts (NOLOCK) C ON C.ContactID = OCM.ContactID AND OCM.IsDeleted = 0 AND C.AccountID = @AccountId AND C.IsDeleted=0
											LEFT JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = C.AccountID AND CE.IsDeleted = 0 AND CE.IsPrimary = 1) 
											C ON C.OpportunityID = O.OpportunityID
									WHERE O.AccountID=@AccountId AND O.IsDeleted=0 AND  O.OpportunityName like '%' + ISNULL(@Query,'') +'%'  AND O.CreatedOn BETWEEN @StartDate AND @EndDate
					
										) x
					--SELECT O.OpportunityID,O.AccountID,O.OpportunityName,O.OpportunityType,O.ProductType,O.Potential, O.LastModifiedOn,--COUNT(*) OVER(PARTITION BY O.OpportunityID) as TotalCount ,
					--C.ContactID,
					--		CASE WHEN (LEN(C.FirstName) > 0 AND LEN(C.LastName) >0) THEN C.FirstName + ' ' + C.LastName
					--			 WHEN LEN(C.Company) > 0 THEN C.Company
					--			 ELSE CE.Email	 END
					--		  AS Name,C.ContactType ,
					--		ROW_NUMBER() OVER(PARTITION BY o.OpportunityID ORDER BY  OCM.ExpectedToClose ASC) AS RowNumber,
					--		COUNT(*) OVER(PARTITION BY O.OpportunityID) AS MAXRowNumber
					 
					-- FROM Opportunities (NOLOCK) O
					--LEFT JOIN OpportunityContactMap OCM (NOLOCK) ON OCM.OpportunityID = O.OpportunityID AND OCM.IsDeleted=0
					--LEFT JOIN Contacts (NOLOCK) C ON C.ContactID = OCM.ContactID AND C.IsDeleted = 0 AND C.AccountID = @AccountId
					--LEFT JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.AccountID = C.AccountID AND CE.IsDeleted = 0 AND CE.IsPrimary = 1
					--WHERE O.AccountID=@AccountId AND O.IsDeleted=0 AND O.CreatedBy= @UserId AND  O.OpportunityName like '%' + ISNULL(@Query,'') +'%'  AND O.CreatedOn BETWEEN @StartDate AND @EndDate
		END


		--SELECT * FROM #opportunities WHERE RowNumber = 1
		SELECT OpportunityID,AccountID,OpportunityName,OpportunityType,ProductType,Potential, LastModifiedOn,ContactID,Name,ContactType ,RowNumber,MAXRowNumber
		INTO #opportunities FROM @OpportunitiesTemp

		declare @sql varchar(max)
		declare @PageNumber1 varchar(10)=Cast(@PageNumber as varchar(10)),
		@PageSize1 varchar(10)=Cast(@PageSize as varchar(10))
		
			set @sql = 'SELECT OpportunityID AS Id,	AccountID,	OpportunityName,	OpportunityType,	ProductType,	Potential,	LastModifiedOn, COUNT(*) OVER() as TotalCount,	ContactID,	'+
			            'CASE WHEN MAXRowNumber > 1 THEN [Name]' + '+''...''' +' ELSE [Name] END AS [ContactName],	ContactType FROM #opportunities WHERE RowNumber = 1 '+
						'ORDER BY ' + ISNULL(@SortField,'') +' ' + ISNULL(@SortDirection,'') + 
						' OFFSET ('+@PageNumber1+'-1)*'+@PageSize1+' ROWS' +
						' FETCH NEXT '+@PageSize1+' ROWS ONLY '
		print @sql
		EXEC (@sql)
END
/*
EXEC [dbo].[Get_Opportunities_List] 339,1,10,null,'LastModifiedOn',0,'2017-04-08 18:23:12.273','2017-05-08 18:23:29.447','desc'
*/