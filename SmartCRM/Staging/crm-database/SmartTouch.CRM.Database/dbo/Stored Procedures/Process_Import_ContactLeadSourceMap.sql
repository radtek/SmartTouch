
CREATE PROCEDURE [dbo].[Process_Import_ContactLeadSourceMap] 
	@leadAdapterJobLogID INT
AS
BEGIN
       

	   DECLARE @ResultID INT,@AccountID INT

		SELECT @AccountID = AccountID FROM LeadAdapterAndAccountMap (NOLOCK) LAM
		INNER JOIN LeadAdapterJobLogs (NOLOCK) LAJ ON LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
		WHERE LAJ.LeadAdapterJobLogID = @LeadAdapterJobLogID

		
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_ContactLeadSourceMap', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		INSERT INTO  LogsTable  (Name,step,JobID,Dates) VALUES  ('Process_Import_ContactLeadSourceMap-STARTED',1,@leadAdapterJobLogID,GETUTCDATE())

		if object_Id('tempdb..#ImpPrimaryLeadSources') is not null  Begin  Drop table #ImpPrimaryLeadSources End
		if object_Id('tempdb..#ImpSecondaryLeadSources') is not null  Begin  Drop table #ImpSecondaryLeadSources End
		if object_Id('tempdb..#LeadSources') is not null  Begin  Drop table #LeadSources End

		;with  leadSources AS(
		SELECT DISTINCT  ICD.ContactID,ICD.FirstName,ICD.PrimaryEmail,ICD.LeadSource,ICD.LeadSourceID,DDV.DropdownValueID PrimaryLeadSourceID,ICD.LeadSourceID SecondaryLeadSourceID FROM ImportContactData ICD
		LEFT JOIN DropdownValues DDV
		ON ICD.LeadSource = DDV.DropdownValue  AND ICD.AccountID =  DDV.AccountID and DDV.DropdownID=5
		WHERE JobID=@leadAdapterJobLogID AND  ICD.ContactID ! = 0 
		)
		
		select * INTO #LeadSources  from leadSources

		INSERT INTO  LogsTable  (Name,step,JobID,Dates) VALUES  ('Inserted into #LeadSources',2,@leadAdapterJobLogID,GETUTCDATE())

		Update #LeadSources set PrimaryLeadSourceId=SecondaryLeadsourceId where PrimaryLeadSourceId IS NULL
		Update #LeadSources set SecondaryLeadsourceId=0 where PrimaryLeadSourceId=SecondaryLeadsourceId

		SELECT * INTO #ImpPrimaryLeadSources FROM (
		select LS.ContactID,PrimaryLeadSourceID LeadSourceID 
		From #LeadSources LS
		) T

		SELECT * INTO #ImpSecondaryLeadSources FROM (
		select LS.ContactID,SecondaryLeadSourceID LeadSourceID 
		From #LeadSources LS where LS.SecondaryLeadsourceId!=0
		) T

		INSERT INTO #ImpSecondaryLeadSources(ContactId,LeadSourceID)
		SELECT IPL.ContactId,IPL.LeadSourceID from #ImpPrimaryLeadSources IPL 
		INNER JOIN  ContactLeadSourceMap CSM ON IPL.ContactId= CSM.ContactId 
		where CSM.IsPrimaryLeadSource = 1 

		DELETE
		#ImpPrimaryLeadSources
		FROM #ImpPrimaryLeadSources IPL
		INNER JOIN #ImpSecondaryLeadSources ISL
		ON ISL.LeadSourceID = IPL.LeadSourceID and ISL.ContactID = IPL.ContactId

		MERGE ContactLeadSourceMap CM
		USING #ImpPrimaryLeadSources ILS
			ON ILS.ContactID = CM.ContactID and ILS.LeadSourceID = CM.LeadSouceID --and ILS.IsPrimaryLeadSource = CM.IsPrimaryLeadSource
		WHEN MATCHED  THEN
        
		UPDATE  SET CM.LastUpdatedDate = GETUTCDATE()
		WHEN NOT MATCHED BY TARGET THEN
		INSERT (ContactId,IsPrimaryLeadSource,LastUpdatedDate,LeadSouceID)
		VALUES( ILS.ContactId,1, GETUTCDATE(),ILS.LeadSourceID);

		INSERT INTO  LogsTable  (Name,step,JobID,Dates) VALUES  ('Inserted into ContactLeadSourceMap from  #ImpPrimaryLeadSources',3,@leadAdapterJobLogID,GETUTCDATE())
		
		MERGE ContactLeadSourceMap CM
		USING #ImpSecondaryLeadSources ILS
			ON ILS.ContactID = CM.ContactID and ILS.LeadSourceID = CM.LeadSouceID --and ILS.IsPrimaryLeadSource = CM.IsPrimaryLeadSource
		WHEN MATCHED  THEN

		UPDATE  SET CM.LastUpdatedDate = GETUTCDATE()
		WHEN NOT MATCHED BY TARGET THEN
		INSERT (ContactId,IsPrimaryLeadSource,LastUpdatedDate,LeadSouceID)
		VALUES( ILS.ContactId,0, GETUTCDATE(),ILS.LeadSourceID);

		INSERT INTO  LogsTable  (Name,step,JobID,Dates) VALUES  ('Inserted into ContactLeadSourceMap from  #ImpSecondaryLeadSources',4,@leadAdapterJobLogID,GETUTCDATE())
		
		
	UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID		
END

--EXEC [dbo].[Process_Import_ContactLeadSourceMap1] 73251