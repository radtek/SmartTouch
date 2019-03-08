-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TourDetailsAddingToContactSummary]
		@TourID dbo.Contact_List READONLY,
		@ContactIds dbo.Contact_List READONLY,
		@AccountID INT,
		@OwnerID INT

AS
BEGIN
		DECLARE @NoteCatageoryId SMALLINT
		DECLARE @TourCommunity VARCHAR(100)
		DECLARE @TourType VARCHAR(100)

		SELECT @NoteCatageoryId= DropdownValueID FROM DropdownValues (NOLOCK) WHERE AccountID=@AccountID AND DropdownID=11 AND DropdownValueTypeID=58 

		SELECT TOP 1 @TourCommunity=DV.DropdownValue FROM Tours (NOLOCK) T
		JOIN @TourID TM ON TM.ContactID=T.TourID
		JOIN DropdownValues (NOLOCK) DV ON DV.DropdownValueID=T.CommunityID

		SELECT TOP 1 @TourType=DV.DropdownValue FROM Tours (NOLOCK) T
		JOIN @TourID TM ON TM.ContactID=T.TourID
		JOIN DropdownValues (NOLOCK) DV ON DV.DropdownValueID=T.TourType
		

		IF EXISTS (SELECT 1 FROM @ContactIds)
		BEGIN
				DECLARE @NoteID INT

				INSERT INTO Notes(NoteDetails,CreatedBy,CreatedOn,AccountID,SelectAll,AddToContactSummary,NoteCategory)
				SELECT CASE WHEN (T.TourDetails IS NULL OR TourDetails ='') THEN 'Tour Date:'+ CONVERT(VARCHAR(10), T.TourDate, 101) + 'Tour Community:'+@TourCommunity+'Tour Type:'+@TourType ELSE T.TourDetails END,@OwnerID,GETUTCDATE(),T.AccountID,0,1,@NoteCatageoryId FROM Tours (NOLOCK) T 
				JOIN @TourID TT ON TT.ContactID=T.TourID

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
				
				SELECT T.TourID,T.TourDetails, ROW_NUMBER () OVER (ORDER BY T.TourID ASC) as RowIndex
				INTO #TempBulkActions
				FROM Tours (NOLOCK) T  
				JOIN @TourID TT ON TT.ContactID=T.TourID

				DECLARE @count INT = 0
				DECLARE @counter INT = 0

				SELECT @count = COUNT(1) FROM #TempBulkActions

				WHILE @counter < @count
				BEGIN

					 DECLARE @ID INT
					 DECLARE @ActualTourId INT

					 SELECT @ActualTourId= TourID FROM #TempBulkActions WHERE RowIndex = (@counter + 1)

					 INSERT INTO Notes(NoteDetails,CreatedBy,CreatedOn,AccountID,SelectAll,AddToContactSummary,NoteCategory)
					 SELECT CASE WHEN (T.TourDetails IS NULL OR TourDetails ='') THEN 'Tour Date:'+ CONVERT(VARCHAR(10), T.TourDate, 101) + ', Tour Community:'+@TourCommunity+', Tour Type:'+@TourType ELSE T.TourDetails END,@OwnerID,GETUTCDATE(),T.AccountID,0,1,@NoteCatageoryId FROM Tours (NOLOCK) T 
					 WHERE T.TourID=@ActualTourId

					 SET @ID  = SCOPE_IDENTITY();

					 INSERT INTO ContactNoteMap(ContactID,NoteID)
					 SELECT CTM.ContactID,@ID FROM ContactTourMap (NOLOCK) CTM
					 WHERE CTM.TourID=@ActualTourId --AND CTM.IsCompleted=0

					 --Last touched updation
					 UPDATE C
					 SET
					 C.LastContactedThrough=6,
					 C.LastUpdatedOn=GETUTCDATE()
					 FROM Contacts C
					 JOIN ContactTourMap CTM ON CTM.ContactID=C.ContactID
					 WHERE CTM.TourID=@ActualTourId --AND CTM.IsCompleted=0

					 INSERT INTO IndexData
				     SELECT NEWID(),CTM.ContactID,1,GETUTCDATE(),NULL,1,1 FROM ContactTourMap (NOLOCK) CTM
					 WHERE CTM.TourID=@ActualTourId --AND CTM.IsCompleted=0

				     SET @counter = @counter + 1
						
				END

		END
END
