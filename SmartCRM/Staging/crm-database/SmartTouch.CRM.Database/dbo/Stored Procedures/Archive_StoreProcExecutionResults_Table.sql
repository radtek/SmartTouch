

CREATE    PROCEDURE [dbo].[Archive_StoreProcExecutionResults_Table]

AS
BEGIN

  DECLARE @COUNT int 

        SELECT  @COUNT = COUNT(distinct ResultID )
	    FROM SmartCRM.[dbo].[StoreProcExecutionResults]   WITH (NOLOCK) where [StartTime] < DATEADD(d, -30, getdate())
  
  print @COUNT

IF @COUNT > 1

  BEGIN
      BEGIN
	    
  
		  INSERT INTO SmartCRMArchive..StoreProcExecutionResults_Archive (ResultID,ProcName,StartTime,EndTime,TotalTime,[Status],AccountID,ParamList)
		  SELECT distinct ResultID,ProcName,StartTime,EndTime,TotalTime,[Status],AccountID,ParamList
		  FROM SmartCRM.[dbo].[StoreProcExecutionResults]   WITH (NOLOCK) where [StartTime] < DATEADD(d, -30, getdate())
		  order by  ResultID

      END
 

		SELECT  distinct  s.* INTO #tem
		FROM SmartCRM.[dbo].[StoreProcExecutionResults]  s  WITH (NOLOCK) where s.[StartTime] < DATEADD(d, -30, getdate())
		  order by  ResultID

		

	
	   
		
		 delete FROM SmartCRM.[dbo].[StoreProcExecutionResults]
		 WHERE ResultID IN (SELECT ResultID   from #tem)
		 
			
		 


		SELECT  @COUNT = COUNT(distinct ResultID )
	    FROM SmartCRM.[dbo].[StoreProcExecutionResults]   WITH (NOLOCK) where [StartTime] < DATEADD(d, -30, getdate())

   END
  
END
