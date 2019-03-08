

CREATE  PROCEDURE  [dbo].[Failed_Forms_Submissions_Report]
(
  @AccountID INT,
  @PageNumber int,
  @Limit int ,
  @DateRange SMALLINT 
)
AS
BEGIN 


  DECLARE @DateTime DATETIME
   
 set  @DateTime = (SELECT  CASE @DateRange WHEN 1 THEN  DATEADD(hh,0,DATEADD(day, DATEDIFF(day, 0, GETUTCDATE() - 7), 0))
                                  WHEN 2 THEN  DATEADD(hh,0,DATEADD(day, DATEDIFF(day, 0, GETUTCDATE() - 14), 0))
								  WHEN 3 THEN  DATEADD(hh,0,DATEADD(day, DATEDIFF(day, 0, GETUTCDATE() - 1), 0)) END )

							



DROP TABLE IF EXISTS  #SubmittedFormFieldData, #FormSubmissions,##New,#Forms


--DECLARE  @DateTime datetime = DATEADD(year,-1,GETDATE()) --DATEADD(hh,0,DATEADD(day, DATEDIFF(day, 0, GETUTCDATE() - 7), 0))

CREATE  TABLE  #FormSubmissions (ID INT IDENTITY(1,1),LeadSourceType nvarchar(50),SubmittedFormDataID INT,FirstName nvarchar(500),LastName nvarchar(500), Email nvarchar(500),MobilePhone nvarchar(50),Remarks NVARCHAR(max),TotalCount int)
CREATE TABLE  #Forms (ID INT IDENTITY(1,1),Name nvarchar(50),SubmittedFormDataID int,FirstName nvarchar(500),LastName nvarchar(500), Email nvarchar(500),PhoneNumber nvarchar(50),Remarks NVARCHAR(max))



SELECT * INTO #SubmittedFormFieldData 
From (
SELECT S.[SubmittedFormDataID],Field,[value],[Remarks],[CreatedOn],[FormID] FROM   [SubmittedFormFieldData] SD 
INNER JOIN  SubmittedFormData S ON S.SubmittedFormDataID = SD.SubmittedFormDataID
WHERE ACCOUNTID = @AccountID AND  CreatedOn Between @DateTime and GETUTCDATE() AND [Status] in (4) )t



ALTER TABLE  #SubmittedFormFieldData ADD  FieldValue NVARCHAR(MAX) 


update  S
  SET S.FieldValue = f.Title
FROM  #SubmittedFormFieldData S 
INNER JOIN Fields F ON F.FieldID = S.Field
WHERE  patindex('%[0-9]%', Field) > 0




		       DECLARE @Count int 
		set  @Count = (select  count(1) from  #SubmittedFormFieldData)

		IF @Count > 0
		BEGIN 

		    INSERT INTO  #SubmittedFormFieldData (SubmittedFormDataID,Field	,value,Remarks,CreatedOn,FormID,FieldValue)
            SELECT  0, 0, 0, 0, 0, 0,'Last Name'

			DECLARE @cols  AS NVARCHAR(MAX)='';
			DECLARE @query AS NVARCHAR(MAX)='';


			SELECT @cols = @cols + QUOTENAME(FieldValue) + ',' FROM (select distinct FieldValue FROM #SubmittedFormFieldData  WHERE FieldValue IS NOT NULL) as tmp
			select @cols = substring(@cols, 0, len(@cols)) --trim "," at end

	   
			set @query = 'INSERT INTO #Forms (Name,SubmittedFormDataID,FirstName,LastName,Email,Remarks) 
			 SELECT ''Forms'' Name,SubmittedFormDataID,[First Name],ISNULL([Last Name],NULL) LastName,[Email],Remarks from  
			(
				select [SubmittedFormDataID],[value],[Remarks],FieldValue from #SubmittedFormFieldData   WHERE  SubmittedFormDataID != 0
			) src
			pivot 
			(
				max(value) for FieldValue in (' + @cols + ')
			) piv'


			--PRINT @query

			EXEC SP_EXECUTESQL @query

		END


		


/*  API Submissions */
DROP TABLE IF EXISTS #T, #tt

SELECT  [APILeadSubmissionID],AccountID,[SubmittedData],[SubmittedOn],[Remarks],[Status],RN INTO  #T FROM  (
SELECT  [APILeadSubmissionID],AccountID,[SubmittedData],[SubmittedOn],[Remarks],0 AS [Status],ROW_NUMBER() OVER
(ORDER BY [APILeadSubmissionID] ASC ) AS  RN FROM  [dbo].[APILeadSubmissions]WHERE AccountID =  @AccountID
and IsProcessed =  4 AND  SubmittedOn between  @DateTime and  GETUTCDATE()   ) T



CREATE  TABLE  #tt (FirstName NVARCHAR(500),LastName NVARCHAR(500),CompanyName NVARCHAR(500),FullName NVARCHAR(500) ,LeadSource INT,
[Owner] INT ,Email NVARCHAR(500),Phone NVARCHAR(50) ,APILeadSubmissionID INT )

DECLARE @json Nvarchar(max),@Rcount int ,@APILeadSubmissionID INT 

SET @Rcount = (SELECT Count(1) from #T WHERE [Status] = 0 )   

WHILE   @Rcount > 0
 BEGIN 

        SET @json = (SELECT TOP 1  '{"data":['+[SubmittedData]+']}' FROM  #T WHERE   [Status] = 0 ORDER BY  RN  ) 
		SET @APILeadSubmissionID = (SELECT TOP 1  [APILeadSubmissionID] FROM  #T WHERE   [Status] = 0 ORDER BY  RN  ) 

	


		INSERT INTO #tt (FirstName,LastName,CompanyName,FullName,LeadSource,[Owner],Email,Phone,APILeadSubmissionID)
		SELECT
		JSON_Value (c.value, '$.FirstName') as FirstName, 
		JSON_Value (c.value, '$.LastName') as LastName, 
		JSON_Value (c.value, '$.CompanyName') as CompanyName, 
		JSON_Value (c.value, '$.FullName') as FullName,
		JSON_Value (p.value, '$.DropdownValueID') as LeadSource,
		JSON_Value (c.value, '$.OwnerId') as  [Owner],
		JSON_Value (E.value, '$.EmailId') as Email,
		ISNULL(JSON_Value (c.value,'$.Phones'),0) as Phone,
		@APILeadSubmissionID
		FROM OPENJSON (@json,'$.data') as c
		CROSS APPLY OPENJSON (c.value, '$.SelectedLeadSource') as p
		CROSS APPLY OPENJSON (c.value, '$.Emails') as E
		
		
		




		UPDATE  #T SET  [Status] =  1 WHERE [APILeadSubmissionID] =  @APILeadSubmissionID

		SET @json = 0
		SET @APILeadSubmissionID = 0
		SET @Rcount = 0

		SELECT  @Rcount = (SELECT Count(1) from #T WHERE [Status] = 0 ) 
END

DROP TABLE IF EXISTS  #TTT



SELECT  * INTO  #TTT from (
SELECT T.*,(U.FirstName+' '+U.LastName) CreatedBy,
[SubmittedOn],[Remarks],DropdownValue
 FROM #tt T 
left JOIN  Users U ON U.UserID  = T.[Owner]
INNER JOIN DropDownValues D ON D.DropdownValueID = T.LeadSource
INNER JOIN APILeadSubmissions A ON A.APILeadSubmissionID = T.APILeadSubmissionID )t



select  * into #f from (
select 'Forms' Name,SubmittedFormDataID,[FirstName],ISNULL([LastName],NULL)LastName,[Email],PhoneNumber,Remarks from #Forms
UNION ALL 
SELECT  'API',APILeadSubmissionID,FirstName,LastName,Email,Phone,Remarks FROM  #TTT
)t



INSERT INTO #FormSubmissions (LeadSourceType,SubmittedFormDataID,FirstName,LastName,Email,MobilePhone,Remarks,TotalCount)
SELECT  Name,SubmittedFormDataID,[FirstName],LastName,[Email],PhoneNumber,Remarks,COUNT(*) OVER () AS TotalCount  FROM #F



SELECT  * FROM  #FormSubmissions
ORDER BY ID
OFFSET @Limit * (@PageNumber -1) ROWS
FETCH NEXT @Limit ROWS  ONLY



END 


/*

EXEC Failed_Forms_Submissions_Report
  @AccountID=339,
  @PageNumber=1,
  @Limit=10 ,
  @DateRange=1 

*/


