CREATE  PROCEDURE [dbo].[Process_Import_ContactTagMap] 
	@leadAdapterJobLogID INT
	,@ContactSource INT
	,@leadAdapterAndAccountMapID INT
	,@ownerId INT
AS
BEGIN
		DECLARE @ResultID INT,@AccountID INT

		SELECT @AccountID = AccountID FROM LeadAdapterAndAccountMap (NOLOCK) LAM
		INNER JOIN LeadAdapterJobLogs (NOLOCK) LAJ ON LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
		WHERE LAJ.LeadAdapterJobLogID = @LeadAdapterJobLogID

		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_ContactTagMap', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		select ContactID
,TagID
,TaggedBy
,TaggedOn, 0 as RowNumber into #tContactTagMap from dbo.ContactTagMap where 1 <> 1;
		

		IF (@ContactSource = 2) --File Import
		BEGIN
			;with itm
			as
			(
				select ICD.ContactID, ITM.TagID, ICD.OwnerID from ImportContactData(NOLOCK) ICD
				JOIN ImportTagMap(NOLOCK) ITM ON ICD.JobID = ITM.LeadAdapterJobLogID
				where (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0
			)
			insert into #tContactTagMap
			select itm.ContactId, itm.TagId, CASE WHEN itm.OwnerID > 0 THEN itm.OwnerID ELSE @ownerId END, GETUTCDATE(), Row_Number() OVER(Order by itm.ContactId) from itm
			left JOIN dbo.ContactTagMap(NOLOCK) CTM ON itm.ContactId = CTM.ContactID and  itm.TagId = CTM.TagId AND CTM.AccountID = @AccountID
			where CTM.ContactTagMapId is null;
		END
		ELSE
		BEGIN
			;with tm
			as
			(
				select distinct @leadAdapterJobLogID as LeadAdapterJobLogID, TagID  from LeadAdapterTagMap LATM where LeadAdapterID = @leadAdapterAndAccountMapID
			)
			, itm
			as
			(					
				select ICD.ContactID, ITM.TagID, ICD.OwnerID from ImportContactData(NOLOCK) ICD
				JOIN tm ITM ON ICD.JobID = ITM.LeadAdapterJobLogID
				where (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0
			)
			insert into #tContactTagMap
			select itm.ContactId, itm.TagId, CASE WHEN itm.OwnerID > 0 THEN itm.OwnerID ELSE @ownerId END, GETUTCDATE(), Row_Number() OVER(Order by itm.ContactId) from itm
			left JOIN dbo.ContactTagMap(NOLOCK) CTM ON itm.ContactId = CTM.ContactID and  itm.TagId = CTM.TagId AND CTM.AccountID = @AccountID
			where CTM.ContactTagMapId is null;
		END

		declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #tContactTagMap

		WHILE (1 = 1)
		BEGIN
			INSERT INTO ContactTagMap ( ContactID, TagID, TaggedBy, TaggedOn,AccountID)
			SELECT ContactID, TagID, TaggedBy, TaggedOn,@AccountID
			FROM #tContactTagMap ICD 
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END

		--FOR CONTACTS TAGS ANALYTICS CALCULATION.
		SELECT DISTINCT TagID
		INTO #TempTagIds
		FROM #tContactTagMap

		INSERT INTO RefreshAnalytics(EntityID,EntityType,Status,LastModifiedOn)
		SELECT TagID,5,1,GETUTCDATE() FROM #TempTagIds

		UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID
END