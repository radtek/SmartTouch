
CREATE PROC [dbo].[getSearchTitle]
@searchDefinitionID INT
as
BEGIn

DECLARE @result Table
(
	RowNumber INT,
	SearchData varchar(800)
)

	;with users_Cte
	as
	(
		select SF.SearchFilterID as FilterID, U.FirstName as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 25, 27, 58, 67 )
		JOIN Users(NOLOCK) U ON U.UserID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), states_Cte
	as
	(
		select SF.SearchFilterID as FilterID, U.StateName as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 18 )
		JOIN States(NOLOCK) U ON U.StateID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), countries_Cte
	as
	(
		select SF.SearchFilterID as FilterID, U.CountryName as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 20 )
		JOIN Countries(NOLOCK) U ON U.CountryID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), leadAdapters_Cte
	as
	(
		select SF.SearchFilterID as FilterID, U.Name as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 61 )
		JOIN LeadAdapterTypes(NOLOCK) U ON U.LeadAdapterTypeId = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), tags_Cte
	as
	(
		select SF.SearchFilterID as FilterID, U.TagName as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 47 )
		JOIN Tags(NOLOCK) U ON U.TagID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), forms_Cte as
	(
		select SF.SearchFilterID as FilterID, U.Name as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 48 )
		JOIN Forms(NOLOCK) U ON U.FormID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID
	), emailStatus_Cte as
	(
		select SF.SearchFilterID as FilterID, S.Name as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		join SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN (59)
		join Statuses(NOLOCK) S ON S.StatusID = SF.SearchText
		WHERE SD.SearchDefinitionID = @searchDefinitionID
	)
	, dropDown_Cte as
	(
		select SF.SearchFilterID as FilterID, U.DropdownValue as Value, cast(0 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID 
		JOIN DropdownValues(NOLOCK) U ON U.DropdownValueID = SF.DropdownValueID
		where SD.SearchDefinitionID = @searchDefinitionID
	) , tourType_Cte as
	(
		    select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN (56)
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	) , actionType_Cte as
	(
		    select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN (64)
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	) , noteCategory_Cte as
	(
		    select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN (71, 72)
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	), community_Cte as
	(
			select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN (42)
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	)
	, LeadSource_cte as
	(
			select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 24, 51 )
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	), LifecycleStage_cte as
	(
			select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 22 )
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	), PartnerType_cte as 
	(
			select SF.SearchFilterID as FilterID, D.DropdownValue as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 21 )
			JOIN DropdownValues(NOLOCK) D ON D.DropdownValueID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	), FirstSourceType as 
	(
			select SF.SearchFilterID as FilterID, C.FirstSource as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 44 )
			JOIN ContactSource(NOLOCK) C ON C.SourceID = SF.SearchText
			where SD.SearchDefinitionID = @searchDefinitionID
	), CustomFields_cte as 
    (
		select SF.SearchFilterID as FilterID, CVO.Value as Value, cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID > 200
		JOIN Fields(NOLOCK) F ON F.FieldID = SF.FieldID
		JOIN CustomFieldValueOptions(NOLOCK) CVO ON CVO.CustomFieldValueOptionID = SF.SearchText
		where SD.SearchDefinitionID = @searchDefinitionID AND F.FieldInputTypeID in (11, 6)
    ),CustomFields_Multi_Check_Cte as 
	(
		select SF.SearchFilterID as FilterID, CVO.Value as Value from SearchDefinitions(NOLOCK) SD
		JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID > 200
		JOIN Fields(NOLOCK) F ON F.FieldID = SF.FieldID
		JOIN CustomFieldValueOptions(NOLOCK) CVO ON CVO.CustomFieldValueOptionID in (select * from dbo.Split(SF.SearchText, '|'))
		where SD.SearchDefinitionID = @searchDefinitionID AND F.FieldInputTypeID in (12, 1)
	), CustomFields_Multi_Check_1 as
	(
		SELECT  DISTINCT    P.FilterID AS FilterID,
            STUFF((    SELECT ',' + SP.Value AS [text()]
                        FROM CustomFields_Multi_Check_Cte SP
                        WHERE SP.FilterID = P.FilterID
                        FOR XML PATH('')), 1, 1, '' ) Value, cast(1 as BIT) as IsSearchFilter
		FROM  CustomFields_Multi_Check_Cte P
    ), DoNotEmail_Cte as
	(
		select SF.SearchFilterID as FilterID, case when SearchText = 1 then 'Yes' else 'No' end as Value , cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 23 )
			where SD.SearchDefinitionID = @searchDefinitionID
	), LastTouchedMethod_Cte as
	(
		select SF.SearchFilterID as FilterID, case when SearchText = 3 then 'Action-Other' 
												   when SearchText = 4 then 'Campaign'
												   when SearchText = 25 then 'Send Mail'
												   when SearchText = 26 then 'Send Text'
												   when SearchText = 46 then 'Phone Call'
												   when SearchText = 48 then 'Appointment'
												   when SearchText = 47 then 'Email'
												   end as Value ,
		    cast(1 as BIT) as IsSearchFilter from SearchDefinitions(NOLOCK) SD
			JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID and SF.FieldID IN ( 41 )
			where SD.SearchDefinitionID = @searchDefinitionID
	),Union_CTE
	as
	(
	select * from users_Cte
	UNION
	select * from states_Cte
	UNION
	select * from countries_Cte
	UNION
	select * from leadAdapters_Cte
	UNION
	select * from tags_Cte
	UNION
	select * from forms_Cte
	UNION
	select * from emailStatus_Cte
	UNION
	select * from tourType_Cte
	UNION
	select * from actionType_Cte
	UNION
	select * from noteCategory_Cte
	UNION
	select * from community_Cte
	UNION
	select * from LeadSource_cte
	UNION
	select * from LifecycleStage_cte
	UNION
	select * from PartnerType_cte
	UNION
	select * from FirstSourceType
	UNION 
	select * from CustomFields_cte
	UNION
	select * from CustomFields_Multi_Check_1
	UNION
	select * from DoNotEmail_Cte
	UNION
	select * from LastTouchedMethod_Cte
	--UNION
	--select * from dropDown_Cte
	)
	insert into @result
	select ROW_NUMBER() over(order by SF.SearchFilterID) as RowNum 
	--,SD.SearchDefinitionName, F.Title, DV.DropdownValue, SQ.QualifierName, SF.SearchText, SD.CustomPredicateScript
	--,SF.FieldID, SF.DropdownValueID
	,case when DV.DropdownValue is null then '<b>' + Title + '</b>' else (case when DV.DropdownID = 1 then '<b>' + DV.DropdownValue + ' Phone </b>'
																											  else '<b>' + DV.DropdownValue + '</b>' end) end + ' ' 
	+ case when QualifierName = 'IsNot' then 'Is Not'
		   when QualifierName = 'IsEmpty' then 'Is Empty'
		   when QualifierName = 'IsNotEmpty' then 'Is Not Empty'
		   when QualifierName = 'IsGreaterThan' then 'Is Greater Than'
		   when QualifierName = 'IsGreaterThanEqualTo' then 'Is Greater Than Equal To'
		   when QualifierName = 'IsLessThan' then 'Is Less Than'
		   when QualifierName = 'IsLessThanEqualTO' then 'Is Less Than Equal TO'
		   when QualifierName = 'Contains' then 'Contains'
		   when QualifierName = 'Does not contains' then 'Does not contains'
		   else QualifierName end
	+ ' ' 	
	+ case when UC.Value is null then '' else UC.Value end
	+ case when UC.Value is null then (case when SF.SearchText is null then '' else SF.SearchText end) else '' end

	as Title 
	 from SearchDefinitions(NOLOCK) SD
	JOIN SearchFilters(NOLOCK) SF ON SF.SearchDefinitionID = SD.SearchDefinitionID
	LEFT JOIN Fields(NOLOCK) F ON SF.FieldID = F.FieldID
	LEFT JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID = SF.DropdownValueID
	JOIN SearchQualifierTypes(NOLOCK) SQ ON SF.SearchQualifierTypeID = SQ.SearchQualifierTypeID
	LEFT JOIN Union_CTE UC ON SF.SearchFilterID = UC.FilterID
	where SD.SearchDefinitionID = @searchDefinitionID
	ORDER BY SF.SearchFilterID

	--select * from @result

	declare @customPredicateScript VARCHAR(MAX)

	SELECT @customPredicateScript = CustomPredicateScript FROM SearchDefinitions WHERE SearchDefinitionID = @searchDefinitionID

	declare @currentRowNumber INT
	declare @currentTitle varchar(500)
	select @currentRowNumber = max(RowNumber)  from @result	

	IF @currentRowNumber = 1
	BEGIN		
		IF CHARINDEX(cast(@currentRowNumber as varchar(20)), @customPredicateScript) > 0
		BEGIN
			select @currentTitle = SearchData from @result
			--select @customPredicateScript = dbo.fnReplaceCharAtPos(@customPredicateScript
			--, CHARINDEX(cast(@currentRowNumber as varchar(20)) , @customPredicateScript), LEN(cast(@currentRowNumber as varchar(20))), @currentTitle);
			set @customPredicateScript = REPLACE(@customPredicateScript, @currentRowNumber , @currentTitle);
		END
	END
	while (@currentRowNumber is NOT NULL)
	begin
		select @currentTitle = SearchData from @result
		
		IF CHARINDEX(cast(@currentRowNumber as varchar(20)) + ' AND', @customPredicateScript) > 0
		BEGIN
			select @customPredicateScript = dbo.fnReplaceCharAtPos(@customPredicateScript
			, CHARINDEX(cast(@currentRowNumber as varchar(20)) + ' AND', @customPredicateScript), LEN(cast(@currentRowNumber as varchar(20))), @currentTitle);
			--set @customPredicateScript = REPLACE(@customPredicateScript, cast(@currentRowNumber as varchar(20)) + ' AND', @currentTitle);
		END

		ELSE IF CHARINDEX(cast(@currentRowNumber as varchar(20)) + ' OR', @customPredicateScript) > 0
		BEGIN
			select @customPredicateScript = dbo.fnReplaceCharAtPos(@customPredicateScript
			, CHARINDEX(cast(@currentRowNumber as varchar(20)) + ' OR', @customPredicateScript), LEN(cast(@currentRowNumber as varchar(20))), @currentTitle);
		END

		ELSE IF CHARINDEX('AND ' + cast(@currentRowNumber as varchar(20)), @customPredicateScript) > 0
		BEGIN
			select @customPredicateScript = dbo.fnReplaceCharAtPos(@customPredicateScript
			, CHARINDEX('AND ' + cast(@currentRowNumber as varchar(20)), @customPredicateScript) + 4, LEN(cast(@currentRowNumber as varchar(20))), @currentTitle);
		end

		ELSE IF CHARINDEX('OR ' + cast(@currentRowNumber as varchar(20)), @customPredicateScript) > 0
		BEGIN
			select @customPredicateScript = dbo.fnReplaceCharAtPos(@customPredicateScript
			, CHARINDEX('OR ' + cast(@currentRowNumber as varchar(20)), @customPredicateScript) + 3, LEN(cast(@currentRowNumber as varchar(20))), @currentTitle);
		end

		--select @currentTitle, @customPredicateScript, cast(@currentRowNumber as varchar(20)) + ' AND', cast(@currentRowNumber as varchar(20)) + ' OR'
		--	,'AND ' + cast(@currentRowNumber as varchar(20)), 'OR ' + cast(@currentRowNumber as varchar(20)), @customPredicateScript
		
		delete from @result where RowNumber = @currentRowNumber
		select @currentRowNumber = max(RowNumber)  from @result
	end

	select @customPredicateScript CustomPredicateScript

END
