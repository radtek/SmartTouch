

CREATE PROCEDURE  [dbo].[Process_Import_Contact_SecondaryLeadSourceMap]
(
  @LeadAdapterJobLogID INT,
  @AccountID INT,
  @LeadAdapterType INT 
)
AS
BEGIN

       DECLARE @ResultID INT

		


		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Contact_SecondaryLeadSourceMap', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		DECLARE  @Title  NVARCHAR(MAX) 

        SET @Title = CASE WHEN @LeadAdapterType = 6 THEN 'source' ELSE 'Comments' END

		IF  @LeadAdapterType = 5 
		BEGIN 
		--SET @Title = '' 
		SET @Title = CASE WHEN  @LeadAdapterType = 5 THEN 'message'  ELSE '0' END
		END
						   

        CREATE  TABLE #FieldValue ( [Value] NVARCHAR(MAX) ,stat BIT,ContactID INT ,RN INT   )


	    SELECT * INTO #ImportData FROM (
		SELECT ccf.*,ICD.[ReferenceID],c.AccountID  FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK)
		INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
		INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
		WHERE JobID = @leadAdapterJobLogID AND  CCF.CustomFieldID IN (SELECT FieldID FROM  Fields  (NOLOCK) WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 AND Title = @Title AND   LeadAdapterType = @LeadAdapterType))T


	IF  (@LeadAdapterType ! = 6 AND @LeadAdapterType ! = 5)

	BEGIN 

			SELECT * INTO #T FROM (
			SELECT   DISTINCT [Value],CCF.ContactID
			FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK) 
			INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
			INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
			WHERE    CCF.CustomFieldID IN (SELECT FieldID FROM  Fields (NOLOCK)  WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 and Title = @Title AND   LeadAdapterType = @LeadAdapterType ) AND  JobID = @leadAdapterJobLogID  	AND  [Value] LIKE  '%Via%')T 

			SELECT  * INTO #Field FROM ( 
			SELECT  ContactID, Substring(substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1)),1, case when charindex(' ',substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1)))>0 then 
		    (CharIndex(' ',substring([Value],Charindex('via',[Value])+4,(len([Value])-Charindex('via',[Value])+1))))-1 else (len([Value])-(Charindex('via',[Value])+4 ) +1)   end ) AS [Value]  FROM  #T)T


			INSERT INTO  #FieldValue  ([Value],stat,ContactID,RN)
			SELECT  [Value],0 stat,ContactID,ROW_NUMBER() OVER(ORDER BY ContactID) AS RN FROM  #Field


	END 
	  ELSE IF  @LeadAdapterType  = 6
	   BEGIN 
      
		    SELECT * INTO #NHM FROM (
			SELECT   DISTINCT [Value],CCF.ContactID
			FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK) 
			INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
			INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
			WHERE    CCF.CustomFieldID IN (SELECT FieldID FROM  Fields (NOLOCK)  WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 and Title = @Title AND   LeadAdapterType = @LeadAdapterType ) AND  JobID = @leadAdapterJobLogID )T 

			INSERT INTO  #FieldValue  ([Value],stat,ContactID,RN)
		    SELECT  [Value],0 stat,ContactID,ROW_NUMBER() OVER(ORDER BY ContactID) AS RN FROM  #NHM


	  END 
	 IF  @LeadAdapterType  = 5
	   BEGIN 
      
		    SELECT * INTO #NH5 FROM (
			SELECT   DISTINCT [Value],CCF.ContactID
			FROM  dbo.ContactCustomFieldMap  CCF (NOLOCK) 
			INNER JOIN  Contacts  c (NOLOCK) ON CCF.ContactID = c.ContactID
			INNER JOIN  ImportContactData ICD ON ICD.ReferenceID = c.ReferenceID
			WHERE    CCF.CustomFieldID IN (SELECT FieldID FROM  Fields (NOLOCK)  WHERE  AccountID = @AccountID AND IsLeadAdapterField = 1 and Title = @Title AND   LeadAdapterType = @LeadAdapterType ) AND  JobID = @leadAdapterJobLogID )T 

			INSERT INTO  #FieldValue  ([Value],stat,ContactID,RN)
		    SELECT  ISNULL(Case WHEN [Value] like  '%Zillow%' THEN 'Zillow' 
            WHEN [Value] like  '%Trulia%' THEN 'Trulia' ELSE  NULL END,'Zillow')  AS [Value],0 stat,ContactID,ROW_NUMBER() OVER(ORDER BY ContactID) AS RN FROM  #NH5 --where ([Value] like '%Zillow%' or [Value] like '%Trulia%')


	  END   
	   


	

		DECLARE @Value NVARCHAR(200) ,@Count int ,@RN INT ,@SortID INT ,@ContactID INT ,@Name  NVARCHAR(100),@Name1  NVARCHAR(300)
		SET @Count = (SELECT  COUNT(1) FROM #FieldValue WHERE stat = 0 )
		SELECT  @Name = Name FROM  [LeadAdapterTypes] WHERE  LeadAdapterTypeID = @LeadAdapterType
		IF @LeadAdapterType = 5 
			BEGIN
			  SELECT  @Name1 = Name+'1' FROM  [LeadAdapterTypes] WHERE  LeadAdapterTypeID = @LeadAdapterType
			END 
		ELSE 
			BEGIN
		      SELECT  @Name1 = Name+'-'+Name FROM  [LeadAdapterTypes] WHERE  LeadAdapterTypeID = @LeadAdapterType
		    END
	
	
		WHILE @Count > 0
			 BEGIN 

					 
			      IF @LeadAdapterType = 5 
					  BEGIN 
					   SELECT TOP 1 @Value = [Value], @RN = RN ,@ContactID = ContactID FROM #FieldValue WHERE stat = 0 ORDER BY RN ASC
					  END
				  ELSE  
					  BEGIN 
						 SELECT TOP 1 @Value = @Name+'-'+[Value], @RN = RN ,@ContactID = ContactID FROM #FieldValue WHERE stat = 0 ORDER BY RN ASC 
					  END

				 

					 DECLARE @DropDownValueID INT = 0 
					 SET @DropDownValueID = (SELECT TOP 1 DropDownValueID  FROM Dropdownvalues  (NOLOCK) where  DropDownID = 5 AND  AccountID = @AccountID and [DropdownValue] = @Value )


					 IF (@DropDownValueID IS NULL AND  lower(@Value) != lower(@Name1) )
					   BEGIN  
     
					  
							 SET @SortID = (SELECT  MAX(SortID) FROM DropDownValues (NOLOCK)  WHERE DropDownID = 5 AND  AccountID = @AccountID )

							 INSERT INTO  [dbo].[DropdownValues] ([DropdownID],[AccountID],[DropdownValue],[IsDefault],[SortID],[IsActive],[DropdownValueTypeID],[IsDeleted])
							 SELECT   5,@AccountID,@Value,0,@SortID+1,1,3,0  

							 SET @DropDownValueID = SCOPE_IDENTITY()

					   END


					  IF (@DropDownValueID IS NOT NULL AND  lower(@Value) != lower(@Name1) 
					      AND @DropDownValueID NOT IN (SELECT LeadSouceID FROM ContactLeadSourceMap WHERE ContactID = @ContactID ))

						  BEGIN 
							   UPDATE C
							   SET C.IsPrimaryLeadSource = 0
							   FROM  ContactLeadSourceMap C
							   WHERE  C.ContactID = @ContactID


								INSERT INTO ContactLeadSourceMap (ContactID, LeadSouceID, IsPrimaryLeadSource)
								SELECT DISTINCT @ContactID,@DropDownValueID,1
						  END

					  INSERT INTO dbo.IndexData(ReferenceID, EntityID, IndexType, CreatedOn, [Status],[IsPercolationNeeded])
				      SELECT NEWID(),@ContactID,1,GETUTCDATE(),1,1

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
  
