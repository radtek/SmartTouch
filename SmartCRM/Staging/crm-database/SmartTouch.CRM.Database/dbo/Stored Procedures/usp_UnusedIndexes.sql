
/****** Object:  StoredProcedure [dbo].[usp_UnusedIndexes]    Script Date: 06/04/2014 11:01:22 ******/
CREATE Procedure [dbo].[usp_UnusedIndexes](
	@DatabaseName sysname = Null
)
AS
	SET NOCOUNT ON;
 
	DECLARE @tbl TABLE (
		  DatabaseName sysname
		, SchemaName sysname
		, TableName sysname
		, IndexName sysname
		, UserSeeks int
		, UserScans int 
		, UserLookups int
		, UserUpdates int
		, LastUserSeek datetime
		, LastUserScan datetime
		, LastUserLookup datetime
		, LastUserUpdate datetime
		, SystemSeeks int
		, SystemScans int 
		, SystemLookups int
		, SystemUpdates int
		, LastSystemUpdate datetime
	)

	DECLARE @sql varchar(max),
			@cr char(1),
			@tab char(4)
			
	SET @cr = char(10)
	SET @tab = space(4)
	SET @sql = 'SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;' + @cr
	
	IF @DatabaseName IS NULL
		SET @sql = @sql + 'USE [?];' + @cr
	ELSE
		SET @sql = @sql + 'USE [' + @DatabaseName + '];'
		
	SET @sql = @sql + 'select  DatabaseName = db_name(db_id())' + @cr
	SET @sql = @sql + ', SchemaName = sch.name' + @cr 
	SET @sql = @sql + @tab + ', TableName = object_name(i.object_id)' + @cr 
	SET @sql = @sql + @tab + ', IndexName = i.name' + @cr 
	SET @sql = @sql + @tab + ', UserSeeks = isnull(s.user_seeks,0)' + @cr
	SET @sql = @sql + @tab + ', UserScans = isnull(s.user_scans,0)' + @cr
	SET @sql = @sql + @tab + ', UserLookups = isnull(s.user_lookups,0)' + @cr
	SET @sql = @sql + @tab + ', UserUpdates = isnull(s.user_updates,0)' + @cr
	SET @sql = @sql + @tab + ', LastUserSeek = isnull(s.last_user_update, ''1/1/1900'')' + @cr
	SET @sql = @sql + @tab + ', LastUserScan = isnull(s.last_user_update, ''1/1/1900'')' + @cr
	SET @sql = @sql + @tab + ', LastUserLookup = isnull(s.last_user_update, ''1/1/1900'')' + @cr
	SET @sql = @sql + @tab + ', LastUserUpdate = isnull(s.last_user_update, ''1/1/1900'')' + @cr
	SET @sql = @sql + @tab + ', SystemSeeks = isnull(s.system_seeks,0)' + @cr
	SET @sql = @sql + @tab + ', SystemScans = isnull(s.system_scans,0)' + @cr
	SET @sql = @sql + @tab + ', SystemLookups = isnull(s.system_lookups,0)' + @cr
	SET @sql = @sql + @tab + ', SystemUpdates = isnull(s.system_updates,0)' + @cr
	SET @sql = @sql + @tab + ', LastSystemUpdate = isnull(s.last_system_update, ''1/1/1900'')' + @cr
	SET @sql = @sql + 'from sys.indexes i' + @cr
	SET @sql = @sql + 'inner join sys.objects o on i.object_id = o.object_id' + @cr
	SET @sql = @sql + 'inner join sys.schemas sch on o.schema_id = sch.schema_id' + @cr
	SET @sql = @sql + 'left join sys.dm_db_index_usage_stats s on s.object_id = i.object_id' + @cr
	SET @sql = @sql + @tab + 'and i.index_id = s.index_id' + @cr
	SET @sql = @sql + 'where (objectproperty(i.object_id, ''IsIndexable'') = 1)' + @cr
	SET @sql = @sql + @tab + 'AND (objectproperty(i.object_id, ''IsIndexed'') = 1)' + @cr 
	SET @sql = @sql + @tab + 'AND (i.type <> 0)'
	SET @sql = @sql + @tab + 'AND (s.database_id = db_id())' + @cr 
	SET @sql = @sql + @tab + 'AND (DB_NAME(DB_ID()) NOT IN (''master'', ''model'', ''msdb'', ''tempdb''))' + @cr
	SET @sql = @sql + @tab + 'AND (	(s.index_id is null)' + @cr -- and dm_db_index_usage_stats has no reference to this index
	SET @sql = @sql + @tab + @tab + 'OR (s.user_updates > 0 and s.user_seeks = 0 and s.user_scans = 0 and s.user_lookups = 0)' + @cr -- index is being updated, but not used by seeks/scans/lookups
	SET @sql = @sql + ');'
	
	IF @DatabaseName IS NULL
	BEGIN
		INSERT INTO @tbl (DatabaseName, SchemaName, TableName, IndexName, UserSeeks, UserScans, UserLookups, UserUpdates, LastUserSeek, LastUserScan
										, LastUserLookup, LastUserUpdate, SystemSeeks, SystemScans, SystemLookups, SystemUpdates, LastSystemUpdate)
			EXEC sp_MSforeachdb @sql;
	END 		
	ELSE
	BEGIN
		INSERT INTO @tbl (DatabaseName, SchemaName, TableName, IndexName, UserSeeks, UserScans, UserLookups, UserUpdates, LastUserSeek, LastUserScan
										, LastUserLookup, LastUserUpdate, SystemSeeks, SystemScans, SystemLookups, SystemUpdates, LastSystemUpdate)
			EXEC (@sql);
	END 		
			
		
		SELECT    DatabaseName 
				, SchemaName 
				, TableName 
				, IndexName 
				, UserSeeks 
				, UserScans 
				, UserLookups 
				, UserUpdates 
				, LastUserUpdate 
				, SystemSeeks 
				, SystemScans 
				, SystemLookups 
				, SystemUpdates 
				, LastSystemUpdate 
		FROM @tbl
		ORDER BY DatabaseName, SchemaName, TableName, IndexName;

