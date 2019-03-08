-- =============================================
-- Author:		<Author,,SurendraBabu V>
-- Create date: <Create Date,,08/11/2017>
-- Description:	<Description,,Checking Account(s) has already active or published Messages
-- =============================================
CREATE PROCEDURE [dbo].[MessageCenterValidator]
	@Accounts dbo.Contact_List READONLY,
	@MessageId INT,
	@From DATETIME,
	@To   DATETIME

AS
BEGIN
		SELECT DISTINCT A.AccountName FROM MarketingMessageAccountMap (NOLOCK) MAM
        JOIN MarketingMessages (NOLOCK) MM ON MM.MarketingMessageID = MAM.MarketingMessageID
		JOIN @Accounts TA ON TA.ContactID = MAM.AccountID
        JOIN Accounts (NOLOCK) A ON A.AccountID = MAM.AccountID
        WHERE MM.Status IN (1001,1002) AND MM.MarketingMessageID <> @MessageId AND MM.IsDeleted=0 AND 
        ((MM.ScheduleFrom IS NOT NULL AND MM.ScheduleFrom > @From) OR (MM.ScheduleFrom IS NULL)) AND 
        ((MM.ScheduleTo IS NOT NULL AND MM.ScheduleTo < @To) OR (MM.ScheduleTo IS NULL)) 

END
