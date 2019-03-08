﻿

CREATE FUNCTION [dbo].[Split]
(
   @List VARCHAR(MAX),
   @Delimiter VARCHAR(2)
)
RETURNS @Items TABLE (DataValue NVARCHAR(4000))
 WITH SCHEMABINDING
    AS
    BEGIN
       DECLARE @ll INT = LEN(@List) + 1, @ld INT = LEN(@Delimiter);
     
       WITH a AS
       (
           SELECT
               [start] = 1,
               [end]   = COALESCE(NULLIF(CHARINDEX(@Delimiter, 
                           @List, 1), 0), @ll),
               [value] = SUBSTRING(@List, 1, 
                         COALESCE(NULLIF(CHARINDEX(@Delimiter, 
                           @List, 1), 0), @ll) - 1)
           UNION ALL
           SELECT
               [start] = CONVERT(INT, [end]) + @ld,
               [end]   = COALESCE(NULLIF(CHARINDEX(@Delimiter, 
                           @List, [end] + @ld), 0), @ll),
               [value] = SUBSTRING(@List, [end] + @ld, 
                         COALESCE(NULLIF(CHARINDEX(@Delimiter, 
                           @List, [end] + @ld), 0), @ll)-[end]-@ld)
           FROM a
           WHERE [end] < @ll
       )
       INSERT @Items SELECT [value]
       FROM a
       WHERE LEN([value]) > 0
       OPTION (MAXRECURSION 0);
     
       RETURN;
	END   
--RETURNS TABLE
--WITH SCHEMABINDING AS
--RETURN
--  WITH E1(N)        AS ( SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 
--                         UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 
--                         UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1),
--       E2(N)        AS (SELECT 1 FROM E1 a, E1 b),
--       E4(N)        AS (SELECT 1 FROM E2 a, E2 b),
--       E42(N)       AS (SELECT 1 FROM E4 a, E2 b),
--       cteTally(N)  AS (SELECT 0 UNION ALL SELECT TOP (DATALENGTH(ISNULL(@List,1))) 
--                         ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM E42),
--       cteStart(N1) AS (SELECT t.N+1 FROM cteTally t
--                         WHERE (SUBSTRING(@List,t.N,1) = @Delimiter OR t.N = 0))
--  SELECT DataValue = SUBSTRING(@List, s.N1, ISNULL(NULLIF(CHARINDEX(@Delimiter,@List,s.N1),0)-s.N1,8000))
--    FROM cteStart s;


