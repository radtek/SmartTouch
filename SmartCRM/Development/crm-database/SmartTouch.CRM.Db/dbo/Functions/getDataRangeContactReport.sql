CREATE function [dbo].[getDataRangeContactReport](@dateRange char(1), @lastUpdatedDate datetime, @StartDate datetime)
returns int
as
begin
	return CASE @dateRange
			WHEN 'D' THEN  DATEDIFF(dd, @StartDate, @lastUpdatedDate) + 1 
			WHEN 'M' THEN (DATEDIFF(Month, @StartDate, @lastUpdatedDate)) + 1 
			ELSE  DATEDIFF(wk,@startDate,@lastUpdatedDate) + 1  end
end