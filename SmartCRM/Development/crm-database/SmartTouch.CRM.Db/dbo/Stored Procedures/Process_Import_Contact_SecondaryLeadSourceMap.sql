CREATE PROCEDURE  [dbo].[Process_Import_Contact_SecondaryLeadSourceMap]
(
  @LeadAdapterJobLogID INT,
  @AccountID INT
)
AS
BEGIN

       DECLARE @ResultID INT

		


		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Contact_SecondaryLeadSourceMap', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()


	SELECT * INTO #ImportData FROM (
	SELECT ccf.*,ICD.[ReferenceID],c.AccountID  FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK)
	INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
	INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
	WHERE JobID = @leadAdapterJobLogID AND  CCF.CustomFieldID IN (SELECT FieldID FROM  Fields  (NOLOCK) WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 and Title = 'Comments' AND   LeadAdapterType = 1))T


	SELECT * INTO #T FROM (
	SELECT   DISTINCT [Value],CCF.ContactID
	FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK) 
	INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
	INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
	WHERE    CCF.CustomFieldID IN (SELECT FieldID FROM  Fields (NOLOCK)  WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 and Title = 'Comments' AND   LeadAdapterType = 1 ) AND  JobID = @leadAdapterJobLogID )T 

	SELECT  * INTO #Field FROM ( 
    SELECT  ContactID, Substring(substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1)),1, case when charindex(' ',substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1)))>0 then 
(CharIndex(' ',substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1))))-1 else (len([Value])-(Charindex('via',[Value])+4 ) +1)   end ) AS [Value]  FROM  #T)T



     SELECT  *  INTO   #FieldValue 
	 FROM
	 (
	 SELECT  [Value],0 stat,ContactID
	,ROW_NUMBER() OVER(ORDER BY ContactID) AS RN  FROM  #Field

	 )T



	

	DECLARE @Value NVARCHAR(200) ,@Count int ,@RN INT ,@SortID INT ,@ContactID INT 
    SET @Count = (SELECT  COUNT(1) FROM #FieldValue WHERE stat = 0 )
	
	WHILE @Count > 0
		 BEGIN 

				 SELECT TOP 1 @Value ='BDX-'+[Value], @RN = RN ,@ContactID = ContactID FROM #FieldValue WHERE stat = 0 ORDER BY RN ASC 
				 

				 DECLARE @DropDownValueID INT = 0 
				SET @DropDownValueID = (SELECT TOP 1 DropDownValueID  FROM Dropdownvalues  (NOLOCK) where  DropDownID = 5 AND  AccountID = @AccountID and [DropdownValue] = @Value )


				 IF (@DropDownValueID IS NULL AND  lower(@Value) != lower('BDX-BDX') )
				  BEGIN  
     
					  
					 SET @SortID = (SELECT  MAX(SortID) FROM DropDownValues (NOLOCK)  WHERE DropDownID = 5 AND  AccountID = @AccountID )

					 INSERT INTO  [dbo].[DropdownValues] ([DropdownID],[AccountID],[DropdownValue],[IsDefault],[SortID],[IsActive],[DropdownValueTypeID],[IsDeleted])
					 SELECT   5,@AccountID,@Value,0,@SortID+1,1,3,0  

					 SET @DropDownValueID = SCOPE_IDENTITY()

				  END


				  else IF (@DropDownValueID IS NOT NULL AND  lower(@Value) != lower('BDX-BDX') 
				  AND @DropDownValueID NOT IN (SELECT LeadSouceID FROM ContactLeadSourceMap WHERE ContactID = @ContactID ))

				  BEGIN 
					   UPDATE C
					   SET C.IsPrimaryLeadSource = 0
					   FROM  ContactLeadSourceMap C
					   WHERE  C.ContactID = @ContactID


						INSERT INTO ContactLeadSourceMap (ContactID, LeadSouceID, IsPrimaryLeadSource)
						SELECT DISTINCT @ContactID,@DropDownValueID,1
                  END

				  

				  UPDATE  #FieldValue SET stat = 1 WHERE RN = @RN

				  SET @Value = 0
				  SET @Count = 0
				  SET @RN = 0
				  SET @SortID = 0
				  SET @DropDownValueID = 0

				  SET @Count = (SELECT  COUNT(1) FROM #FieldValue WHERE stat = 0 )


		 END


		 
     
	 UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID


END
GO

