CREATE PROCEDURE [dbo].[Deleting_CroneJobHistory]

AS
BEGIN

  DECLARE @COUNT int
  SELECT
    @COUNT = COUNT(CronJobHistoryID)
  FROM CronJobHistory WITH (NOLOCK)
  WHERE StartTime < GETUTCDATE() - 10


  IF @COUNT > 1

  BEGIN
  BEGIN

    Set Identity_insert EnterpriseCommunication..CronJobHistory_ArchiveS on
	INSERT INTO EnterpriseCommunication..CronJobHistory_ArchiveS (CronJobHistoryID, CronJobID, StartTime, EndTime, Remarks)
      SELECT CronJobHistoryID,CronJobID,StartTime,EndTime,Remarks
      FROM EnterpriseCommunication..CronJobHistory WITH (NOLOCK)
      WHERE StartTime < GETUTCDATE() - 10
	  Set Identity_insert EnterpriseCommunication..CronJobHistory_ArchiveS off

   END
 

    SELECT
      c.* INTO #tem
    FROM CronJobHistory c WITH (NOLOCK)
    WHERE StartTime < GETUTCDATE() - 10

    ;
    WITH Tem (CronJobHistoryID)
    AS (SELECT
      CronJobHistoryID
    FROM #TEM)
    DELETE FROM CronJobHistory
    WHERE CronJobHistoryID IN (SELECT
        CronJobHistoryID
      FROM Tem);


    SELECT
      COUNT(*)
    FROM CronJobHistory WITH (NOLOCK)
    WHERE StartTime < GETUTCDATE() - 10
  END
  
END