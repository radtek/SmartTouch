CREATE PROCEDURE [dbo].[Get_FormSubmissionsByForm] 
	@FormID INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@PageNumber INT,
	@PageSize INT
AS
BEGIN
	
	SELECT @PageSize = COALESCE(@PageSize, 2000000000)

	SELECT F.FormSubmissionID as Id, F.ContactID, F.IPAddress,DDV.DropdownValue,F.SubmittedOn, F.SubmittedData, COUNT(1) OVER() AS TotalCount 
	FROM FormSubmissions (NOLOCK) F 
	INNER JOIN Contacts C (NOLOCK) ON C.ContactID = F.ContactID
	INNER JOIN DropdownValues DDV (NOLOCK) ON DDV.DropdownValueID = F.LeadSourceID
	WHERE F.FormID = @FormID AND F.SubmittedData IS NOT NULL AND F.SubmittedOn BETWEEN COALESCE( @StartDate,CAST('1753-1-1' as datetime)) AND COALESCE(@EndDate,GETUTCDATE()) AND C.IsDeleted = 0
	ORDER BY F.FormSubmissionID DESC
	OFFSET (@PageNumber * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
END
