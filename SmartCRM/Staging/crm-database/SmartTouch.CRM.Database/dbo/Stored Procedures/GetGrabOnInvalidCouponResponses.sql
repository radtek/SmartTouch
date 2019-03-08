
CREATE PROCEDURE [dbo].[GetGrabOnInvalidCouponResponses]
	@PageNumber INT = 1, 
	@PageSize INT = 10 
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
	  SET NOCOUNT ON;

	  DECLARE @FormID INT = 970
	  DECLARE @FromDate DATETIME 
	  DECLARE @year INT = 2016
	  DECLARE @month INT = 07
	  DECLARE @day INT = 20

	  SELECT @FromDate =  CAST(CONVERT(VARCHAR, @year) + '-' + CONVERT(VARCHAR, @month) + '-' + CONVERT(VARCHAR, @day) AS DATETIME)
 
	  DECLARE @CouponTypeSubmissions TABLE (FormSubmissionId INT, ContactId INT, Issue VARCHAR(25),SubmittedOn DATETIME, TotalCount INT)
	  DECLARE @CouponSubmissions TABLE (FormSubmissionId INT, ContactId INT, Coupon VARCHAR(75),SubmittedOn DATETIME)

	  INSERT INTO @CouponTypeSubmissions
	  SELECT FS.FormSubmissionID, FS.ContactID, CFVO.Value, FS.SubmittedOn, COUNT(1) OVER() As TotalCount 
	  FROM FormSubmissions (NOLOCK) FS
	  INNER JOIN ContactCustomFieldMap CFM (NOLOCK) ON CFM.ContactID = FS.ContactID
	  INNER JOIN CustomFieldValueOptions CFVO (NOLOCK) ON CFVO.CustomFieldValueOptionID = CFM.Value
	  INNER JOIN Contacts C (NOLOCK) ON C.ContactID = FS.ContactId
	  WHERE FormID = @FormID AND CFM.CustomFieldID IN (2586) AND FS.SubmittedOn >= @FromDate
	  ORDER BY FS.FormSubmissionID DESC
	  

	  INSERT INTO @CouponSubmissions
	  SELECT FS.FormSubmissionID, FS.ContactID, CFM.Value, FS.SubmittedOn 
	  FROM FormSubmissions (NOLOCK) FS
	  INNER JOIN ContactCustomFieldMap CFM (NOLOCK) ON CFM.ContactID = FS.ContactID
	  INNER JOIN Contacts C (NOLOCK) ON C.ContactID = FS.ContactId
	  WHERE FormID = @FormID AND CFM.CustomFieldID IN (2594) AND  FS.SubmittedOn >= @FromDate
	  ORDER BY FS.FormSubmissionID DESC
	  
 
	  ;WITH CouponSubmissionsData(FormSubmissionId,ContactId,Issue,SubmittedOn,TotalCount,Coupon) AS
	  (
		   SELECT DISTINCT STUFF((SELECT ',' + rtrim(convert(char(10),FormSubmissionID))
			 FROM   @CouponTypeSubmissions b
			 WHERE  CTS.contactid = b.contactid
			 FOR XML PATH('')),1,1,'') FormSubmissionID,CTS.ContactId,CTS.Issue,CAST(CTS.SubmittedOn AS DATE), CTS.TotalCount,CS.Coupon -- CTS.ContactId
		   FROM @CouponTypeSubmissions CTS
		   INNER JOIN @CouponSubmissions CS ON CS.FormSubmissionId = CTS.FormSubmissionId
		   WHERE  NOT EXISTS (SELECT 1  FROM InvalidCouponsEngagedContacts (NOLOCK) ICEC WHERE ICEC.FormSubmissionID = CTS.FormSubmissionId )
		   GROUP BY CTS.FormSubmissionId, CTS.ContactId,CTS.Issue,CTS.SubmittedOn,CS.Coupon,CTS.TotalCount
	  )
  
	  SELECT CS.FormSubmissionID,CS.ContactId,CS.Issue ,CS.Coupon,CS.SubmittedOn , COUNT(1) OVER() As TotalCount  
	  FROM CouponSubmissionsData CS
	  ORDER BY CS.SubmittedOn DESC
	  OFFSET (@PageNumber-1)*@PageSize ROWS
	  FETCH NEXT @PageSize ROWS ONLY
END

/*
EXEC [dbo].[GetGrabOnInvalidCouponResponses]
*/