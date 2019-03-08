
CREATE PROCEDURE [dbo].[Process_Import_Phone_Custom_Field] 
	@leadAdapterJobLogID INT
	,@AccountID int
AS
BEGIN
		DECLARE @ResultID INT
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Phone_Custom_Field', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		--Update Phone Number
		;WITH CurrentData
		AS
		(
			SELECT ICD.ContactID, IPD.PhoneType, IPD.PhoneNumber, ICD.AccountID, Row_Number() over (order by ICD.ImportContactDataID) as RowNumber FROM dbo.ImportContactData(NOLOCK) ICD
				INNER JOIN dbo.ImportPhoneData(NOLOCK) IPD ON ICD.OrginalRefId = cast(IPD.ReferenceID as varchar(50))
			WHERE JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID
				AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0
		)
		select *  into #currendData from CurrentData;

		update C
		set C.IsDeleted = 0
		from dbo.ContactPhoneNumbers C
		join #currendData CD ON C.ContactId = CD.ContactId and C.PhoneType = CD.PhoneType and C.PhoneNumber = CD.PhoneNumber
		where C.IsDeleted <> 0;

		declare  @counter  int = 0;
		declare  @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #currendData

		WHILE (1 = 1)
		BEGIN
			insert into ContactPhoneNumbers (ContactID, PhoneNumber, PhoneType, IsPrimary, IsDeleted, AccountId)
			SELECT ICD.ContactID, ICD.PhoneNumber, ICD.PhoneType, 0, 0, ICD.AccountId
			FROM #currendData ICD 
			left join dbo.ContactPhoneNumbers(NOLOCK) C ON C.ContactId = ICD.ContactId and C.PhoneType = ICD.PhoneType and C.PhoneNumber = ICD.PhoneNumber
			WHERE C.ContactPhoneNumberID is null 
			and ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END	

		;WITH DefaultPhone
			AS(
			SELECT CPN.ContactID, CPN.ContactPhoneNumberID, ROW_NUMBER() OVER(PARTITION BY CPN.ContactID ORDER BY CPN.ContactPhoneNumberID) Ranks
				FROM dbo.ContactPhoneNumbers(NOLOCK) CPN
					INNER JOIN dbo.ImportContactData(NOLOCK) ICD ON CPN.ContactID = ICD.ContactID
				WHERE ICD.JobID = @leadAdapterJobLogID AND ICD.AccountID = @accountID
					AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)  AND CPN.IsDeleted = 0
					AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0					
				)					
				select CPN.ContactPhoneNumberID, IsPrimary
				into #dPhone
				FROM dbo.ContactPhoneNumbers(NOLOCK) CPN
				INNER JOIN DefaultPhone DP ON CPN.ContactPhoneNumberID = DP.ContactPhoneNumberID
				where CPN.IsPrimary <> 1 and DP.Ranks = 1 AND CPN.IsDeleted = 0;

				create index ix_dPhone ON #dPhone(ContactPhoneNumberID);

				UPDATE dbo.ContactPhoneNumbers
				SET IsPrimary	= 1
				FROM dbo.ContactPhoneNumbers CPN
					INNER JOIN #dPhone DP ON CPN.ContactPhoneNumberID = DP.ContactPhoneNumberID;
		
		--Update Custom Fields		
		;WITH uniqueData
		AS
		(
			SELECT ICD.ContactID, CD.FieldID, CD.FieldTypeID, CD.FieldValue, CD.ImportCustomDataID FROM ImportContactData ICD				
				INNER JOIN dbo.ImportCustomData(NOLOCK) CD ON ICD.OrginalRefId = cast(CD.ReferenceID as varchar(50))
			WHERE JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID
				AND CD.FieldTypeID NOT IN (1,6,11,12)
				AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND LEN(CD.FieldValue) > 0 AND ICD.ValidEmail = 1 and ICD.ContactStatusID <> 0
				 AND IsDuplicate=0
			UNION ALL

			SELECT ICD.ContactID, CD.FieldID, CD.FieldTypeID, ([dbo].[GET_Imports_CustomFieldValue](CD.FieldID, CD.FieldTypeID, CD.FieldValue)), CD.ImportCustomDataID
				FROM ImportContactData(NOLOCK) ICD				
				INNER JOIN dbo.ImportCustomData (NOLOCK)  CD ON ICD.OrginalRefId = cast(CD.ReferenceID as varchar(50))
			WHERE JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID
				AND CD.FieldTypeID IN (1,6,11,12)
				AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND LEN(CD.FieldValue) > 0 AND ICD.ValidEmail = 1 and ICD.ContactStatusID <> 0 AND IsDuplicate=0

				UNION ALL

			SELECT ICD.ContactID, CD.FieldID, CD.FieldTypeID,CD.FieldValue, CD.ImportCustomDataID
							FROM ImportContactData(NOLOCK) ICD				
							INNER JOIN dbo.ImportCustomData (NOLOCK)  CD ON ICD.OrginalRefId = cast(CD.ReferenceID as varchar(50))
						WHERE JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID
							AND CD.FieldTypeID NOT IN (1,6,11,12)
							AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
							AND ICD.ContactID > 0 AND LEN(CD.FieldValue) > 0 AND ICD.ValidEmail = 1 and ICD.ContactStatusID <> 0 AND IsDuplicate=1
		),
		CurrentData
		as
		(
			select ContactId, FieldID, FieldTypeID, MAX(ImportCustomDataID) as ImportCustomDataID
			from uniqueData
			group by ContactId, FieldID, FieldTypeID
		)
		,Data
		as
		(
			select ud.ContactId, ud.FieldID, ud.FieldTypeID, ud.FieldValue from uniqueData ud
			join CurrentData cd on cd.ImportCustomDataID = ud.ImportCustomDataID
		)
		select CD.ContactID, CD.FieldID, CD.FieldValue, ROW_NUMBER() over (order by CD.ContactID) as RowNumber into #customDataFields from Data CD

		set  @counter  = 0;
		set  @rowCount = 0;
		select @rowCount = Count(1) from #customDataFields

		WHILE (1 = 1)
		BEGIN
			UPDATE c
				SET Value = ICD.FieldValue
			FROM dbo.ContactCustomFieldMap C
			JOIN #customDataFields ICD ON C.ContactID = ICD.ContactID AND C.CustomFieldID = ICD.FieldID
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);
			
			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END	

		set  @counter  = 0;
		set  @rowCount = 0;
		select @rowCount = Count(1) from #customDataFields

		WHILE (1 = 1)
		BEGIN
			insert into dbo.ContactCustomFieldMap (ContactID, CustomFieldID, Value)
			SELECT ICD.ContactID, ICD.FieldID, ICD.FieldValue
			FROM #customDataFields ICD 
			LEFT JOIN dbo.ContactCustomFieldMap(NOLOCK) C ON C.ContactID = ICD.ContactID AND C.CustomFieldID = ICD.FieldID
			WHERE c.ContactCustomFieldMapID IS NULL AND ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

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