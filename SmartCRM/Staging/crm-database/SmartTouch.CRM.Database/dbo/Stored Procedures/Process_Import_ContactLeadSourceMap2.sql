
CREATE PROCEDURE [dbo].[Process_Import_ContactLeadSourceMap2] 
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

		select ContactID, LeadSourceID, cast(1 as bit) as IsPrimaryLeadSource, 0 as RowNumber into #tLeadSourceMap from dbo.ImportContactData where 1 <> 1;


IF EXISTS (SELECT  LeadSource FROM ImportContactData WHERE JobID  =  @leadAdapterJobLogID and  LeadSource in  (select DropDownvalue from DropdownValues WHERE  AccountID = @AccountID) )
	 BEGIN 
	
		;WITH tempData
		as
		(
			SELECT ContactID, LeadSourceID, case   when LeadSource = 'Imports' THEN 1 
			                                       WHEN len(LeadSource) > 0 then 0 else 1 end as  IsPrimaryLeadSource  FROM dbo.ImportContactData(NOLOCK) WHERE JobID = @leadAdapterJobLogID AND ContactID > 0 
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL) AND ValidEmail = 1 AND IsDuplicate=0
			UNION
			select ICD.ContactID, DV.DropdownValueID as LeadSourceID , case when   EXISTS (SELECT IsPrimaryLeadSource FROM ContactLeadSourceMap WHERE ContactID IN (SELECT ContactID FROM ImportContactData WHERE  JobID = @leadAdapterJobLogID AND  ContactID =  ICD.ContactID) AND IsPrimaryLeadSource = 1 )  then 0 else 1 end as IsPrimaryLeadSource from ImportContactData(NOLOCK) ICD
			join DropdownValues(NOLOCK) DV on ICD.LeadSource = DV.DropdownValue and ICD.AccountID = DV.AccountID and DV.IsActive = 1 
			LEFT  JOIN ContactLeadSourceMap CLM ON CLM.ContactID = ICD.ContactID
			where JobID = @leadAdapterJobLogID AND ICD.ContactID > 0 
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL) AND ValidEmail = 1 AND IsDuplicate=0 
			and ICD.LeadSource <> 'Imports' and DV.DropdownID = 5 
		)
		insert into #tLeadSourceMap
		select td.ContactID, LeadSourceID, 
				case  when EXISTS (SELECT IsPrimaryLeadSource FROM ContactLeadSourceMap (NOLOCK) WHERE ContactID =td.ContactID AND IsPrimaryLeadSource = 1 )  then 0 
					  ELSE td.IsPrimaryLeadSource end as IsPrimaryLeadSource, 
				  ROW_NUMBER() OVER(ORDER BY td.ContactID, LeadSourceID) from tempData td
		LEFT join ContactLeadSourceMap CLM (nolock) on td.ContactID = CLM.ContactID and td.LeadSourceID = clm.LeadSouceID
		where CLM.ContactLeadSourceMapID is null;
		END
	ELSE 

	BEGIN 

	 insert into #tLeadSourceMap
	 select ContactID, LeadSourceID, 1 IsPrimaryLeadSource, ROW_NUMBER() OVER(ORDER BY ContactID, LeadSourceID) from  ImportContactData WHERE JobID = @leadAdapterJobLogID 
		
	 END 

		declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #tLeadSourceMap

		WHILE (1 = 1)
		BEGIN
			INSERT INTO ContactLeadSourceMap (ContactID, LeadSouceID, IsPrimaryLeadSource)
			SELECT ContactID, LeadSourceID, IsPrimaryLeadSource
			FROM #tLeadSourceMap ICD 
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END

		UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID

END