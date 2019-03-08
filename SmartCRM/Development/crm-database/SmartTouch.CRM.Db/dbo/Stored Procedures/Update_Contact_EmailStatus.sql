
CREATE   PROCEDURE [dbo].[Update_Contact_EmailStatus]
AS
BEGIN


	
		CREATE TABLE #TempContact (ContactEmailID int ,ContactID int ,Email NVARCHAR(500) ,EmailStatus INT ,IsPrimary INT ,AccountID INT ,NeverBounceEmailStatusID INT ,NeverBounceRequestID INT,CreatedOn DATETIME,EmailStatus_1 INT ,Statuses int)


		INSERT INTO #TempContact (ContactEmailID,ContactID,Email,EmailStatus,IsPrimary,AccountID,NeverBounceEmailStatusID,NeverBounceRequestID,CreatedOn,EmailStatus_1,Statuses) 
		SELECT CE.ContactEmailID,CE.ContactID,CE.Email,CE.EmailStatus,CE.IsPrimary,CE.AccountID,n.[NeverBounceEmailStatusID],n.[NeverBounceRequestID],n.[CreatedOn],N.EmailStatus,0 FROM  NeverBounceEmailStatus n WITH (NOLOCK)
		INNER JOIN ContactEmails CE WITH (NOLOCK) ON CE.ContactID = N.ContactID AND ce.ContactEmailID = n.ContactEmailID
		AND CE.IsPrimary = 1 AND IsDeleted = 0
		where N.UPDATEDON IS NULL 


		--select * from  #TempContact

		CREATE TABLE #TempContactEmails (ContactEmailID INT ,ContactID INT ,EmailStatus INT )

      DECLARE @rowCount INT = 1

   WHILE @rowCount > 0
    BEGIN
	
			INSERT into #TempContactEmails (ContactEmailID,ContactID,EmailStatus)
		   select top 1000 ContactEmailID,ContactID,EmailStatus_1 from  #TempContact WHERE Statuses = 0 


			UPDATE CE
			 SET CE.EmailStatus = n.EmailStatus
			from  ContactEmails CE  
			INNER JOIN  #TempContactEmails n  ON CE.ContactID = N.ContactID and ce.ContactEmailID = n.ContactEmailID
			WHERE  CE.IsPrimary = 1 AND ce.IsDeleted = 0 AND CE.EmailStatus != 54


			INSERT INTO IndexData ([ReferenceID],[EntityID],[IndexType],[CreatedOn],[Status],[IsPercolationNeeded])
			SELECT NEWID(),ContactID,1,GETUTCDATE(),1,1 
			FROM #TempContactEmails

			SET @rowCount = @@ROWCOUNT

			UPDATE T
			SET T.Statuses =1 
			FROM #TempContact T 
			INNER JOIN #TempContactEmails TT ON TT.ContactEmailID = T.ContactEmailID

			UPDATE N
			SET N.[UpdatedOn] = GETUTCDATE() 
			FROM NeverBounceEmailStatus N 
			INNER JOIN #TempContactEmails TT ON TT.ContactEmailID = N.ContactEmailID

			TRUNCATE TABLE #TempContactEmails

			

		END 


		
END
GO

