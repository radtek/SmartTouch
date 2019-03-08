CREATE PROCEDURE [dbo].[GrabonTagsCreatingsp]
(
 @TagName NVARCHAR(50) ,
@AccountID INT ,
@TagID int ,
@TaggedBy int ,
--@TagName_1 NVARCHAR(50),
@Domain NVARCHAR(50) 
)
AS
BEGIN

--declare @TagName_1 NVARCHAR(50)
--select  @TagName_1 = tagName from Tags WHERE AccountID = 2037 AND TagName Like '%Opt-in_Gmail_Splt%'
--print @TagName_1


CREATE TABLE #Tem (ID INT IDENTITY(1,1), Email NVARCHAR(300),TagID int,TagName varchar(50),ContactID INT,Status bit,RownNumber int )


;WITH CTE
AS
(
select  DISTINCT CE.Email,c.ContactID
 from Contacts c WITH (NOLOCK)
inner join contactEmails ce WITH (NOLOCK) on ce.contactID = C.contactID
inner join ContactTagMap ct WITH (NOLOCK) on ct.contactID = c.contactID
inner join Tags t WITH (NOLOCK) on t.TagID = CT.TagID
WHERE C.AccountID = @AccountID AND CE.AccountID = @AccountID AND T.AccountID = @AccountID
AND T.TagName   like  @TagName and EmailStatus IN (51,52,55)
AND CE.Email  like @Domain  AND C.IsDeleted = 0 AND Ce.IsDeleted = 0
AND t.IsDeleted = 0 and ct.AccountID = @AccountID
)
INSERT INTO #Tem (Email,ContactID,RownNumber)
SELECT  distinct Email,ContactID,ROW_NUMBER() OVER(ORDER BY Email,ContactID) as RownNumber
from CTE



declare @counter int = 0;
		declare @rowCount int = 0;
		declare @batchCount int = 10000;
		select @rowCount = Count(1) from #Tem
		
		select TagID,Row_Number() over(Order by TagID) RoNum INTO #Tags FROM Tags WITH(NOLOCK) WHERE AccountID = @AccountID And TagName Like  '%Opt-in_Hotmail_Splt%' and [count] != 10000 
		
		WHILE (1 = 1)
		BEGIN
		IF (Select Count(1) From #Tags) >0
		BEGIN
		set @TagID = (Select TagID From #Tags Where RoNum =@counter+1)
			INSERT INTO  [dbo].[ContactTagMap] (ContactID,[TagID],[TaggedBy],[TaggedOn],[AccountID])
			SELECT ContactID,@TagID,@TaggedBy,getutcdate(),@AccountID
			FROM #Tem ICD 
			WHERE ICD.RownNumber between (@counter * @batchCount + 1) and ((@counter * @batchCount) + @batchCount);
        END
			IF (@@ROWCOUNT = 0  AND @rowCount < ((@counter * @batchCount) + @batchCount))
			BEGIN
				BREAK
			END
	 
			set @counter = @counter + 1;
		END

END

GO


