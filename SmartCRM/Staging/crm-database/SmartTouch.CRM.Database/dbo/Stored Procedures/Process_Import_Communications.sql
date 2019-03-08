
CREATE PROCEDURE [dbo].[Process_Import_Communications] 
	@leadAdapterJobLogID INT
	,@AccountID INT
	,@AddressTypeID INT
AS
BEGIN
		DECLARE @ResultID INT
		INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
		VALUES('Process_Import_Communications', @AccountID, '@LeadAdapterJobLogID:' + cast(@LeadAdapterJobLogID as varchar(10)))
		SET @ResultID = scope_identity()

		select C.CommunicationID, C.ContactId
		,CASE WHEN LEN(ICD.FacebookUrl) > 0 THEN ICD.FacebookUrl ELSE CM.FacebookUrl END as FacebookUrl
		,CASE WHEN LEN(ICD.TwitterUrl) > 0 THEN ICD.TwitterUrl ELSE CM.TwitterUrl END as TwitterUrl
		,CASE WHEN LEN(ICD.GooglePlusUrl) > 0 THEN ICD.GooglePlusUrl ELSE CM.GooglePlusUrl END as GooglePlusUrl
		,CASE WHEN LEN(ICD.LinkedInUrl) > 0 THEN ICD.LinkedInUrl ELSE CM.LinkedInUrl END as LinkedInUrl
		,CASE WHEN LEN(ICD.BlogUrl) > 0 THEN ICD.BlogUrl ELSE CM.BlogUrl END as BlogUrl
		,CASE WHEN LEN(ICD.WebSiteUrl) > 0 THEN ICD.WebSiteUrl ELSE CM.WebSiteUrl END as WebSiteUrl, ROW_NUMBER() OVER(Order by C.CommunicationID) as RowNumber
		into #tCommunications from dbo.Contacts(NOLOCK) C
		INNER JOIN dbo.ImportContactData(NOLOCK) ICD ON C.ContactID = ICD.ContactID AND C.AccountID = ICD.AccountID AND C.CommunicationID = ICD.CommunicationID
		INNER JOIN dbo.Communications(NOLOCK) CM ON CM.CommunicationID = C.CommunicationID
		WHERE ICD.JobID = @leadAdapterJobLogID AND C.IsDeleted = 0  
			AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
			AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0

		declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 1000;
		select @rowCount = Count(1) from #tCommunications

		WHILE (1 = 1)
		BEGIN
			UPDATE C
				SET C.FacebookUrl = ICD.FacebookUrl
				,C.TwitterUrl = ICD.TwitterUrl
				,C.GooglePlusUrl = ICD.GooglePlusUrl
				,C.LinkedInUrl = ICD.LinkedInUrl
				,C.BlogUrl = ICD.BlogUrl
				,C.WebSiteUrl = ICD.WebSiteUrl
			FROM dbo.Communications C
			JOIN #tCommunications ICD ON C.CommunicationID = ICD.CommunicationID
			WHERE ICD.RowNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);

			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END

		DECLARE @ChangeResult TABLE (ChangeType VARCHAR(10), Id INT, ContactId INT)
		--Update Communications
		;WITH CurrentData
			AS (
				SELECT ICD.ContactID, ICD.AccountID, C.CommunicationID, ICD.FacebookUrl, ICD.TwitterUrl, ICD.GooglePlusUrl, ICD.LinkedInUrl, ICD.BlogUrl, ICD.WebSiteUrl 
					FROM dbo.ImportContactData(NOLOCK) ICD
						INNER JOIN dbo.Contacts(NOLOCK) C ON ICD.ContactID = C.ContactID and C.AccountID = ICD.AccountID AND C.IsDeleted =0 AND IsDuplicate=0
					WHERE ICD.JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID 
						AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
						AND ICD.ContactID > 0 AND ICD.ValidEmail = 1
						AND (LEN(FacebookUrl)> 0 OR LEN(TwitterUrl)> 0 OR LEN(GooglePlusUrl)> 0 OR LEN(LinkedInUrl)> 0 OR LEN(BlogUrl)> 0 OR LEN(WebSiteUrl)> 0)
				)
		MERGE dbo.Communications C
		USING CurrentData ICD ON C.CommunicationId = ICD.CommunicationId
		WHEN MATCHED THEN UPDATE
			SET		
			 FacebookUrl	= case when LEN(ICD.FacebookUrl)> 0 then ICD.FacebookUrl else C.FacebookUrl end,
			 TwitterUrl		= case when LEN(ICD.TwitterUrl)> 0 then ICD.TwitterUrl else C.TwitterUrl end,
			 GooglePlusUrl	= case when LEN(ICD.GooglePlusUrl)> 0 then ICD.GooglePlusUrl else C.GooglePlusUrl end,
			 LinkedInUrl	= case when LEN(ICD.LinkedInUrl)> 0 then ICD.LinkedInUrl else C.LinkedInUrl end,
			 BlogUrl		= case when LEN(ICD.BlogUrl)> 0 then ICD.BlogUrl else C.BlogUrl end,
			 WebSiteUrl		= case when LEN(ICD.WebSiteUrl)> 0 then ICD.WebSiteUrl else C.WebSiteUrl end
			WHEN NOT MATCHED THEN INSERT (FacebookUrl, TwitterUrl, GooglePlusUrl, LinkedInUrl, BlogUrl, WebSiteUrl)
			VALUES( ICD.FacebookUrl, ICD.TwitterUrl, ICD.GooglePlusUrl, ICD.LinkedInUrl, ICD.BlogUrl, ICD.WebSiteUrl)
			output $action, inserted.CommunicationId, ICD.ContactId into @ChangeResult;
		
		UPDATE C
			SET C.CommunicationID = CR.Id
		FROM dbo.Contacts C
			INNER JOIN @ChangeResult CR ON C.ContactID = CR.ContactId
		WHERE C.CommunicationID IS NULL and C.IsDeleted =0;

		DELETE FROM @ChangeResult
				
		--Update Addresses
		;WITH currentData
			AS (
				SELECT ICD.ContactID, ICD.AccountID, CA.AddressID PrimaryAddressId, ICD.State, ICD.Country, ICD.AddressLine1, ICD.AddressLine2, ICD.City, ICD.ZipCOde, S.StateID, CO.CountryID			
					FROM dbo.ImportContactData(NOLOCK) ICD
					INNER JOIN dbo.Contacts(NOLOCK) C ON ICD.ContactID = C.ContactID and C.AccountID = ICD.AccountID AND C.IsDeleted =0 AND IsDuplicate=0
					LEFT JOIN dbo.States(NOLOCK) S ON (ICD.State = S.StateID OR S.StateName = ICD.State OR RIGHT(S.StateID,LEN(S.StateID)-3) = ICD.State)
					LEFT JOIN dbo.Countries(NOLOCK) CO ON (ICD.Country = CO.CountryID OR CO.CountryName = ICD.Country)
					LEFT JOIN dbo.ContactAddressMap(NOLOCK) CAM ON C.ContactID = CAM.ContactID 
					LEFT JOIN dbo.Addresses(NOLOCK) CA ON CA.AddressID = CAM.AddressID AND CA.IsDefault = 1
				WHERE JobID = @LeadAdapterJobLogID AND ICD.AccountId = @AccountID 
					AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL)  AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
					AND ICD.ContactID > 0 AND ICD.ValidEmail = 1
					AND (LEN([State])> 0 OR  LEN(Country)> 0 OR LEN(ICD.AddressLine1)> 0 OR LEN(ICD.AddressLine2)> 0 OR LEN(ICD.City)> 0 OR LEN(ICD.ZipCode)> 0 )
				), ProcessAddress		
			AS
			(
			SELECT ROW_NUMBER() OVER(ORDER BY ContactID, AccountID, PrimaryAddressId) AS RowNum, ContactID, AccountID, PrimaryAddressId,
				case when (LEN(CD.ZipCode) > 0 AND CD.ZipCode IS NOT NULL)
					then CASE WHEN TR.CountryID = 1 THEN 'US-' + [STATECODE] WHEN TR.CountryID = 2 THEN 'CA-'+ [STATECODE] ELSE NULL END else CD.State end [State],
				case when (LEN(CD.ZipCode) > 0 AND CD.ZipCode IS NOT NULL)
					then CASE WHEN TR.CountryID = 1 THEN 'US' WHEN TR.CountryID = 2 THEN 'CA' ELSE NULL END 
					else NULL --CD.Country  
					end Country,
				AddressLine1, AddressLine2, City, CD.ZipCOde, StateID, CD.CountryID, @AddressTypeID AddressTypeId
			FROM CurrentData CD
			LEFT JOIN TaxRates TR ON CD.ZipCode = TR.ZIPCode WHERE @AddressTypeID IS NOT NULL
			), DisAddress
			AS
			(
				select ContactID, MAX(RowNum) as RowNum from ProcessAddress
				Group by ContactID
			), finalAddress
			AS
			(
				select PA.* from ProcessAddress PA
				JOIN DisAddress AD ON PA.RowNum = AD.RowNum
			)
			--select * from finalAddress
			MERGE dbo.Addresses C
			USING finalAddress ICD ON C.AddressId = ICD.PrimaryAddressId
			WHEN MATCHED THEN UPDATE
				SET	AddressLine1 = case when LEN(ICD.AddressLine1)> 0 then ICD.AddressLine1 else C.AddressLine1 end,
					AddressLine2 = case when LEN(ICD.AddressLine2)> 0 then ICD.AddressLine2 else C.AddressLine2 end,
					City = case when LEN(ICD.City)> 0 then ICD.City else C.City end,
					StateID = case when LEN(ICD.StateID)> 0 then ICD.StateID else C.StateID end,
					CountryID	 = case when LEN(ICD.CountryID)> 0 then ICD.CountryID else C.CountryID end,
					ZipCode = case when LEN(ICD.ZipCode)> 0 then ICD.ZipCode else C.ZipCode end
			WHEN NOT MATCHED THEN INSERT (AddressTypeID, AddressLine1, AddressLine2, City, StateID, CountryID, ZipCode, IsDefault)
			VALUES( AddressTypeId, ICD.AddressLine1, ICD.AddressLine2, ICD.City, ICD.StateID, ISNULL(Country, 'US'), ICD.ZipCode, 1)
			output $action, inserted.AddressId, ICD.ContactId into @ChangeResult;
		
		INSERT INTO dbo.ContactAddressMap (ContactID, AddressID)
		SELECT ContactId, Id FROM @ChangeResult WHERE ChangeType = 'INSERT' AND ContactID > 0
		
		;WITH DefaultAddress
		AS(
		SELECT CAM.ContactID, CAM.AddressID, ROW_NUMBER() OVER(PARTITION BY CAM.ContactID ORDER BY CAM.AddressID) Ranks
			FROM dbo.ContactAddressMap(NOLOCK) CAM
				INNER JOIN dbo.ImportContactData(NOLOCK) ICD ON CAM.ContactID = ICD.ContactID
			WHERE ICD.JobID = @leadAdapterJobLogID AND ICD.AccountID = @AccountID
				AND (IsBuilderNumberPass = 1 OR IsBuilderNumberPass IS NULL) AND (IsCommunityNumberPass = 1 OR IsCommunityNumberPass IS NULL)
				AND ICD.ContactID > 0 AND ICD.ValidEmail = 1 AND IsDuplicate=0
			)
			UPDATE dbo.Addresses
				SET IsDefault	= 1
				FROM dbo.Addresses A
					INNER JOIN DefaultAddress DA ON A.AddressID = DA.AddressID
				WHERE DA.Ranks = 1
		

		UPDATE	StoreProcExecutionResults
		SET		EndTime = GETDATE(),
				TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
				Status = 'C'
		WHERE	ResultID = @ResultID

END

