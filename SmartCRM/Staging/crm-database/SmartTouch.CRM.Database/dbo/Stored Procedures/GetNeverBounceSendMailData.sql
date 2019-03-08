-- =============================================
-- Author:		<Author,,Surendra Babu V>
-- Create date: <Create Date,,26\04\2017>
-- Description:	<Description,,Get NeverBounce SendMail Data>
-- =============================================
CREATE PROCEDURE [dbo].[GetNeverBounceSendMailData]
	@NeverBounceRequestId INT
AS
BEGIN
	  DECLARE @TotalImportedContactCount INT
	  DECLARE @BadEmailSCount INT
	  DECLARE @GoodEmailsCount NVARCHAR(10)
	  DECLARE @EntityType SMALLINT,
		@EntityNames VARCHAR(2000) ,
		@EntityIDs VARCHAR(1000)

	  DECLARE @NeverBounceMappings TABLE
	  (
	  	NeverBounceRequestID INT,
	  	EntityIds VARCHAR(MAX),
	  	EntityType INT
	  )
 
	  SELECT @TotalImportedContactCount = EmailsCount FROM NeverBounceRequests(NOLOCK) WHERE NeverBounceRequestID =  @NeverBounceRequestId

	  SELECT @BadEmailSCount= COUNT(1) FROM NeverBounceEmailStatus (NOLOCK) NES WHERE NES.NeverBounceRequestID=@NeverBounceRequestId AND NES.EmailStatus=53

	  SELECT @GoodEmailsCount= COUNT(1) FROM NeverBounceEmailStatus (NOLOCK) NES WHERE NES.NeverBounceRequestID=@NeverBounceRequestId AND NES.EmailStatus !=53

	  INSERT INTO @NeverBounceMappings
	  SELECT NBR.NeverBounceRequestID, STUFF((SELECT distinct ', ' + CAST(NBM.EntityID AS nvarchar)
									 FROM NeverBounceMappings (NOLOCK) NBM
									 WHERE NBM.NeverBounceRequestID = NBR.NeverBounceRequestID
										FOR XML PATH(''), TYPE
										).value('.', 'NVARCHAR(MAX)') 
									,1,2,'') EntityIds, (SELECT TOP 1 NeverBounceEntityType FROM NeverBounceMappings NB WHERE NB.NeverBounceRequestID = NBR.NeverBounceRequestID) AS EntityType 
							FROM NeverBounceRequests (NOLOCK) NBR 
							WHERE NBR.NeverBounceRequestID = @NeverBounceRequestId
	   SELECT TOP 1 @EntityType = EntityType, @EntityIDs = EntityIds FROM @NeverBounceMappings

	   IF (@EntityType = 1)
	   BEGIN
	   		SELECT @EntityNames = [FileName] FROM LeadAdapterJobLogs WHERE LeadAdapterJobLogID IN (SELECT DataValue FROM dbo.Split_2(@EntityIDs,','))
	   END
	   ELSE IF (@EntityType = 2)
	   BEGIN
	   	SELECT @EntityNames = STUFF((SELECT distinct ', ' + T.TagName
	   								 FROM vTags (NOLOCK)T
	   								 WHERE T.TagID IN (SELECT DataValue FROM dbo.Split_2(@EntityIDs,','))
	   									FOR XML PATH(''), TYPE
	   									).value('.', 'NVARCHAR(MAX)') 
	   								,1,2,'')
	   END
	   ELSE IF (@EntityType = 3)
	   BEGIN
	   	SELECT @EntityNames = STUFF((SELECT distinct ', ' + S.SearchDefinitionName
	   								 FROM  SearchDefinitions (NOLOCK) S
	   								 WHERE S.SearchDefinitionID IN (SELECT DataValue FROM dbo.Split_2(@EntityIDs,','))
	   									FOR XML PATH(''), TYPE
	   									).value('.', 'NVARCHAR(MAX)') 
	   								,1,2,'')
	   END

	  SELECT @TotalImportedContactCount AS ImportTotal,@BadEmailSCount AS BadEmails, @GoodEmailsCount AS GoodEmails, @EntityNames AS EntityNames, @EntityType AS EntityID

END


/* 
	[dbo].[GetNeverBounceSendMailData] 13
*/