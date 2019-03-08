



CREATE PROCEDURE [dbo].[GET_Campaigns_Count]
AS 
BEGIN
	SET NOCOUNT ON
		DECLARE @RowCount int = 15000
		Declare @body varchar (max) ,
		@subject varchar (max) 

		SET @RowCount = (SELECT MAX(CampaignID) FROM Campaigns WITH (NOLOCK))

		IF (@RowCount) >= 14000
		--IF (@RowCount) >= 7024
			BEGIN
			
			exec msdb..sp_send_dbmail
              @profile_name = 'ST-DB-PROD',
              @recipients = 'manohar.pathapati@landmarkit.in;santosh.srinivas@landmarkit.in;haripratap.elduri@landmarkit.in;ravindra.challagandla@landmarkit.in;dba@landmarkit.in',
                          
              @body = 'CampaignID is reached to max value 14000,so please go ahead and take the nessary actions',
              @subject = 'Campains Count',
              @body_format = 'HTML',
			  	@importance = 'High';
				--PRINT  'CampaignID is reached to max value 14000 and please go ahead and take the nessary action'
			END
		--ELSE
		--	BEGIN
				
		--	--exec msdb..sp_send_dbmail
  -- --           @profile_name = 'ST-Stage-Notification',
  -- --           @recipients = 'manohar.pathapati@landmarkit.in',
                          
  -- --           @body = 'CampaignID is not reached  to max value,now the count is + ' select @@RowCount ' + ,',
  -- --           @subject = 'campainscount',
  -- --           @body_format = 'HTML'
		--	--  	--@importance = 'High';
		--	END
	SET NOCOUNT OFF
END



