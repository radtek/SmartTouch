CREATE PROCEDURE [dbo].[ActionDetailsAddingToNoteSummary] 
		@ActionIds dbo.Contact_List READONLY,
		@ContactIds dbo.Contact_List READONLY,
		@AccountID INT,
		@OwnerID INT

AS
BEGIN
		DECLARE @NoteCatageoryId SMALLINT

		SELECT @NoteCatageoryId= DropdownValueID FROM DropdownValues (NOLOCK) WHERE AccountID=@AccountID AND DropdownID=11 AND DropdownValueTypeID=57 

		IF EXISTS (SELECT 1 FROM @ContactIds)
		BEGIN
				DECLARE @NoteID INT

				INSERT INTO Notes(NoteDetails,CreatedBy,CreatedOn,AccountID,SelectAll,AddToContactSummary,NoteCategory)
				SELECT A.ActionDetails,@OwnerID,GETUTCDATE(),A.AccountID,0,1,@NoteCatageoryId FROM Actions (NOLOCK) A 
				JOIN @ActionIds TA ON TA.ContactID=A.ActionID

				SET @NoteID  = SCOPE_IDENTITY();

				INSERT INTO ContactNoteMap(ContactID,NoteID)
				SELECT ContactID,@NoteID FROM @ContactIds

				--For LastTouched Updation
				UPDATE C
				SET
				C.LastContactedThrough=6,
				C.LastUpdatedOn=GETUTCDATE()
				FROM Contacts C
				JOIN @ContactIds TC ON TC.ContactID=C.ContactID

				INSERT INTO IndexData
				SELECT NEWID(),ContactID,1,GETUTCDATE(),NULL,1,1 FROM @ContactIds
		END
		ELSE
		BEGIN
				
				SELECT A.ActionID,A.ActionDetails, ROW_NUMBER () OVER (ORDER BY A.ActionID ASC) as RowIndex
				INTO #TempBulkActions
				FROM Actions (NOLOCK) A 
				JOIN @ActionIds TA ON TA.ContactID=A.ActionID

				DECLARE @count INT = 0
				DECLARE @counter INT = 0

				SELECT @count = COUNT(1) FROM #TempBulkActions

				WHILE @counter < @count
				BEGIN

					 DECLARE @ID INT
					 DECLARE @ActionId INT

					 SELECT @ActionId=ActionID FROM #TempBulkActions WHERE RowIndex = (@counter + 1)

					 INSERT INTO Notes(NoteDetails,CreatedBy,CreatedOn,AccountID,SelectAll,AddToContactSummary,NoteCategory)
					 SELECT A.ActionDetails,@OwnerID,GETUTCDATE(),A.AccountID,0,1,@NoteCatageoryId FROM Actions (NOLOCK) A 
					 WHERE A.ActionID=@ActionId

					 SET @ID  = SCOPE_IDENTITY();

					 INSERT INTO ContactNoteMap(ContactID,NoteID)
					 SELECT CAM.ContactID,@ID FROM ContactActionMap (NOLOCK) CAM
					 WHERE CAM.ActionID=@ActionId AND CAM.IsCompleted=0

					 --Last touched updation
					 UPDATE C
					 SET
					 C.LastContactedThrough=6,
					 C.LastUpdatedOn=GETUTCDATE()
					 FROM Contacts C
					 JOIN ContactActionMap CAM ON CAM.ContactID=C.ContactID
					 WHERE CAM.ActionID=@ActionId AND CAM.IsCompleted=0

					 INSERT INTO IndexData
				     SELECT NEWID(),CAM.ContactID,1,GETUTCDATE(),NULL,1,1 FROM ContactActionMap (NOLOCK) CAM
					 WHERE CAM.ActionID=@ActionId AND CAM.IsCompleted=0

				     SET @counter = @counter + 1
						
				END

		END
END
GO

