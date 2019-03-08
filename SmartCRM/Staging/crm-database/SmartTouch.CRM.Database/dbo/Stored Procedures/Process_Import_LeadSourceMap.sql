CREATE  PROCEDURE  [dbo].[Process_Import_LeadSourceMap]
(
@leadAdapterJobLogID INT
)
AS
BEGIN

        DECLARE @AccountID INT

		SELECT @AccountID = AccountID FROM LeadAdapterAndAccountMap (NOLOCK) LAM
		INNER JOIN LeadAdapterJobLogs (NOLOCK) LAJ ON LAJ.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
		WHERE LAJ.LeadAdapterJobLogID = @LeadAdapterJobLogID

		
		



        select ContactID, LeadSourceID, cast(1 as bit) as IsPrimaryLeadSource, 0 as RowNumber into #tLeadSourceMap from dbo.ImportContactData where 1 <> 1;



		;WITH tempData
		as
		(
            select ICD.ContactID, DV.DropdownValueID as LeadSourceID , case when EXISTS (SELECT IsPrimaryLeadSource FROM ContactLeadSourceMap 
			WHERE ContactID IN (SELECT ContactID FROM ImportContactData WHERE  JobID = @leadAdapterJobLogID AND  ContactID = ICD.ContactID) AND IsPrimaryLeadSource = 1 )  then 0 else 1 end as IsPrimaryLeadSource from ImportContactData(NOLOCK) ICD
			join DropdownValues(NOLOCK) DV on ICD.LeadSource = DV.DropdownValue and ICD.AccountID = DV.AccountID and DV.IsActive = 1 
			where JobID = @leadAdapterJobLogID AND ICD.ContactID > 0 
			and DV.DropdownID = 5 
		)
		insert into #tLeadSourceMap
		select DISTINCT td.ContactID, TD.LeadSourceID, 
				case  when EXISTS (SELECT IsPrimaryLeadSource FROM ContactLeadSourceMap (NOLOCK) WHERE ContactID =td.ContactID AND IsPrimaryLeadSource = 1 )  then 0 
					  ELSE td.IsPrimaryLeadSource end as IsPrimaryLeadSource, 
				  ROW_NUMBER() OVER(ORDER BY td.ContactID, TD.LeadSourceID) from tempData td
		LEFT join ContactLeadSourceMap CLM (nolock) on td.ContactID = CLM.ContactID and td.LeadSourceID = clm.LeadSouceID
		where CLM.ContactLeadSourceMapID is null;





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

	


END 