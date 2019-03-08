
CREATE PROC [dbo].[GET_Account_FormsCountSummary_Report](
	@AccountID int,
	@FromDate datetime,
	@ToDate datetime,
	@IsAdmin tinyint,
	@OwnerID int,
	@GroupID int,
	@SelectedIDs VARCHAR(MAX),
	@ContactListID smallint = 0,
	@accountMapID int = 0,
	@Debug bit = 0
)
AS 
BEGIN
	SET NOCOUNT ON

	DECLARE @ResultID int, @ParamList varchar(4000)

	SET @ParamList = '@FromDate = ''' + CONVERT(varchar(20), @FromDate, 120) + ''', @ToDate = ''' + CONVERT(varchar(20), @ToDate, 120) + ''', @IsAdmin = ' + CAST(@IsAdmin AS VARCHAR) + ', @OwnerID = ' + CAST(@OwnerID AS VARCHAR) + ', @GroupID = ' + CAST(@GroupID AS VARCHAR) + ', @SelectedIDs = ''' + CAST(@SelectedIDs AS VARCHAR) + ''', @ContactListID = ' + CAST(@ContactListID AS VARCHAR) + ', @accountMapID = ''' + CAST(@accountMapID AS VARCHAR) 

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID, ParamList)
	VALUES('GET_Account_FormsCountSummary_Report', @AccountID, @ParamList)

	SET @ResultID = scope_identity()

	BEGIN TRY
		DECLARE @SQL nvarchar(MAX)
		IF(@ContactListID > 0)
		BEGIN
			IF (@GroupID = 0)
			BEGIN
				SELECT DISTINCT C.ContactID, c.FirstName, c.LastName, ce.email, cp.phonenumber, 0 AS ContactEmailID
				FROM	dbo.Forms(NOLOCK) FM 
						--INNER JOIN dbo.Users U ON U.AccountID IN (FM.AccountID, 1) AND U.UserID = FM.CreatedBy
						INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.AccountID = FM.AccountID AND DV.DropdownID = 5
						INNER JOIN dbo.FormSubmissions(NOLOCK) FMS ON FMS.FormID = FM.FormID AND DV.DropdownValueID = FMS.LeadSourceID
						INNER JOIN dbo.Contacts(NOLOCK) c ON fm.AccountID = c.AccountID AND c.ContactID = fms.contactid
						LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
						LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
				WHERE	FM.AccountID = @AccountID
						AND c.IsDeleted = 0
						AND FM.FormID IN (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))
						AND CONVERT(VARCHAR(10), FMS.SubmittedOn, 120) BETWEEN @FromDate AND @ToDate
						AND (case @IsAdmin when 0 then @OwnerID else ISNULL(c.OwnerID, 0) end) = ISNULL(c.OwnerID, 0)
						--AND FMS.LeadSourceID = @ContactListID
				ORDER BY c.FirstName, c.LastName
			END
			ELSE IF (@GroupID = 1)
			BEGIN
				SELECT DISTINCT c.ContactID, c.FirstName, c.LastName, ce.email, cp.phonenumber, 0 AS ContactEmailID

				--FROM	dbo.Contacts C INNER JOIN dbo.Contacts_Audit ca on ca.AccountID = c.AccountID AND ca.ContactID = c.ContactID AND ca.AuditAction = 'I'
				--		INNER JOIN dbo.LeadAdapterTypes LAT ON LAT.LeadAdapterTypeID = ca.SourceType
				--		INNER JOIN dbo.LeadAdapterAndAccountMap LAM ON C.AccountID = LAM.AccountID AND LAT.LeadAdapterTypeID = LAM.LeadAdapterTypeID
				--		INNER JOIN dbo.DropdownValues DV ON DV.AccountID = c.AccountID AND DV.DropdownID = 5 AND dv.DropdownValueID = LAM.LeadSourceType

						FROM	dbo.Contacts(NOLOCK) C 
					    INNER JOIN dbo.LeadAdapterJobLogDetails(NOLOCK) LJD on C.ReferenceID = LJD.ReferenceID
						INNER JOIN dbo.LeadAdapterJobLogs(NOLOCK) LAJ ON LJD.LeadAdapterJobLogID = LAJ.LeadAdapterJobLogID
						INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAM ON  LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID AND LAM.AccountID = C.AccountID
						INNER JOIN dbo.LeadAdapterTypes(NOLOCK) LAT ON LAT.LeadAdapterTypeID = LAM.LeadAdapterTypeID										
						INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.AccountID = c.AccountID AND DV.DropdownID = 5 AND dv.DropdownValueID = LAM.LeadSourceType

						LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
						LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
				WHERE	LAM.AccountID = @AccountID
						AND c.IsDeleted = 0
						AND LAM.IsDelete = 0
						--AND C.ContactSource = 1
						AND LAM.LeadAdapterAndAccountMapID = @accountMapID
						AND LAT.LeadAdapterTypeID IN (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))
						AND CONVERT(VARCHAR(10), LAJ.CreatedDateTime, 120) BETWEEN @FromDate AND @ToDate
						AND (case @IsAdmin when 0 then @OwnerID else ISNULL(c.OwnerID, 0) end) = ISNULL(c.OwnerID, 0)
						--AND LAM.LeadSourceType = @ContactListID
				ORDER BY c.FirstName, c.LastName
			END
		END
		ELSE
		BEGIN
			IF (@GroupID = 0)
			BEGIN
				SELECT DISTINCT FM.FormID, FM.IsAPIFORM, FM.Name,  COUNT(DISTINCT FMS.ContactID) UniqueSubmissions,COUNT(FMS.ContactID) Total
				FROM	dbo.Forms(NOLOCK) FM 
						--INNER JOIN dbo.Users U ON U.AccountID IN (FM.AccountID, 1) AND U.UserID = FM.CreatedBy
						INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.AccountID = FM.AccountID AND DV.DropdownID = 5
						INNER JOIN dbo.FormSubmissions(NOLOCK) FMS ON FMS.FormID = FM.FormID AND DV.DropdownValueID = FMS.LeadSourceID
						INNER JOIN dbo.Contacts(NOLOCK) c ON fm.AccountID = c.AccountID AND c.ContactID = fms.contactid
				WHERE	FM.AccountID = @AccountID
						AND c.IsDeleted = 0
						AND FM.FormID IN (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))
						AND CONVERT(VARCHAR(10), FMS.SubmittedOn, 120) BETWEEN @FromDate AND @ToDate
						AND (case @IsAdmin when 0 then @OwnerID else ISNULL(c.OwnerID, 0) end) = ISNULL(c.OwnerID, 0)
						--AND (case @IsAdmin when 0 then @OwnerID else ISNULL(FM.CreatedBy, 0) end) = ISNULL(FM.CreatedBy, 0)
				GROUP BY  FM.Name, FM.FormID, FM.IsAPIFORM
				ORDER BY FM.Name
			END
			ELSE IF (@GroupID = 1)
			BEGIN
				SELECT LAM.LeadAdapterAndAccountMapID, CAST(LAM.LeadAdapterTypeID AS int) AS FormID, CASE WHEN LAM.LeadAdapterTypeID = 13 THEN FL.Name ELSE LAT.Name END AS Name,  
				dv.DropdownValue, dv.DropdownValueID, COUNT(DISTINCT C.ContactID) UniqueSubmissions, COUNT(C.ContactID) Total

				--FROM	dbo.Contacts C INNER JOIN dbo.Contacts_Audit ca on ca.AccountID = c.AccountID AND ca.ContactID = c.ContactID AND ca.AuditAction = 'I'
				--		INNER JOIN dbo.LeadAdapterTypes LAT ON LAT.LeadAdapterTypeID = ca.SourceType
				--		INNER JOIN dbo.LeadAdapterAndAccountMap LAM ON C.AccountID = LAM.AccountID AND LAT.LeadAdapterTypeID = LAM.LeadAdapterTypeID
				--		INNER JOIN dbo.DropdownValues DV ON DV.AccountID = c.AccountID AND DV.DropdownID = 5 AND dv.DropdownValueID = LAM.LeadSourceType

					FROM	dbo.Contacts(NOLOCK) C 
					    INNER JOIN dbo.LeadAdapterJobLogDetails(NOLOCK) LJD on C.ReferenceID = LJD.ReferenceID
						INNER JOIN dbo.LeadAdapterJobLogs(NOLOCK) LAJ ON LJD.LeadAdapterJobLogID = LAJ.LeadAdapterJobLogID
						INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAM ON  LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID AND LAM.AccountID = C.AccountID
						INNER JOIN dbo.LeadAdapterTypes(NOLOCK) LAT ON LAT.LeadAdapterTypeID = LAM.LeadAdapterTypeID										
						INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.AccountID = c.AccountID AND DV.DropdownID = 5 AND dv.DropdownValueID = LAM.LeadSourceType
						LEFT JOIN dbo.FacebookLeadAdapter(NOLOCK) FL ON FL.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID

				WHERE	LAM.AccountID = @AccountID
						AND c.IsDeleted = 0
						--AND C.ContactSource = 1
						AND LAM.IsDelete = 0
						AND LAT.LeadAdapterTypeID IN (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))
						AND CONVERT(VARCHAR(10), LAJ.CreatedDateTime, 120) BETWEEN @FromDate AND @ToDate
						--AND  ISNULL(ca.OwnerID, 0) end = ISNULL(ca.OwnerID, 0)
						AND (case @IsAdmin when 0 then @OwnerID else ISNULL(c.OwnerID, 0) end) = ISNULL(c.OwnerID, 0)
						--AND (case @IsAdmin when 0 then @OwnerID else ISNULL(LAM.CreatedBy,0) end) = ISNULL(LAM.CreatedBy, 0)
				GROUP BY LAT.Name, dv.DropdownValue, LAM.LeadAdapterAndAccountMapID, dv.DropdownValueID, LAM.LeadAdapterTypeID, FL.Name, LAM.LeadAdapterAndAccountMapID
				ORDER BY LAT.Name

				/*IF @Debug = 1
				BEGIN
					SELECT	c.* --LAT.LeadAdapterTypeID FormID, LAT.Name, dv.DropdownValue, dv.DropdownValueID, COUNT(DISTINCT C.ContactID) UniqueSubmissions, COUNT(C.ContactID) Total
					FROM	dbo.Contacts C INNER JOIN dbo.Contacts_Audit ca on ca.AccountID = c.AccountID AND ca.ContactID = c.ContactID AND ca.AuditAction = 'I'
							INNER JOIN dbo.LeadAdapterTypes LAT ON LAT.LeadAdapterTypeID = c.SourceType
							INNER JOIN dbo.LeadAdapterAndAccountMap LAM ON C.AccountID = LAM.AccountID AND LAT.LeadAdapterTypeID = LAM.LeadAdapterTypeID
							INNER JOIN dbo.DropdownValues DV ON DV.AccountID = c.AccountID AND DV.DropdownID = 5 AND dv.DropdownValueID = LAM.LeadSourceType
					WHERE	LAM.AccountID = @AccountID
							AND ca.ContactSource = 1
							AND LAM.IsDelete = 0
							AND LAT.LeadAdapterTypeID IN (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))
							AND CONVERT(VARCHAR(10), LAM.ModifiedDateTime, 120) BETWEEN @FromDate AND @ToDate
					ORDER BY LAT.Name
				END*/
			END
		END
  	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF

    PRINT @SQL

	exec sp_executesql @SQL

	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID
END

/*
	  EXEC [dbo].[GET_Account_FormsCountSummary_Report]
		 @AccountID			= 4218,
		 @FromDate          = '2016-03-26 00:00:00.000',       
		 @ToDate            = '2016-04-25 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 6887,
		 @GroupID           = 1,
		 @SelectedIDs       = '3414,3415,3416,3426,3432,3434,3440,5320,5321,5322,5325,5327,5328,5331,5334,5335,5336,5337,5338,5339,5340,5341,5342,5343,5345,5346,5347,5348,5349,5350,5351,5355,5359,5360,5361,5362,5363,5364,5365,5366,5370,5372,5373,5374,5375,5376,5378,5379,5380,5381,5382,5383,5388,5393,5394,5395,5396,5398,5399,5400,5601,5602,5603,5604,5605,5606,5607,5608,5609,5610,5611,5612'


	  EXEC [dbo].[GET_Account_FormsCountSummary_Report]
		 @AccountID			= 4218,
		 @FromDate          = '2016-03-26 00:00:00.000',       
		 @ToDate            = '2016-04-25 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 6887,
		 @GroupID           = 1,
		@ContactListID  = 1,
		 @SelectedIDs       = '1',
		 @Debug = 1

*/