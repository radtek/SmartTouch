-- =============================================
-- Author:		<Author,,SurendraBabu v>
-- Create date: <Create Date,,5/26/17>
-- Description:	<Description,,For Tag Contact count Calculation>
-- =============================================
CREATE PROCEDURE [dbo].[Get_Tags_Contact_Count]
	@TagIds dbo.Contact_List READONLY,
	@AccountId int
AS
BEGIN

		SELECT T.TagID AS Id,T.Count,T.AccountID FROM Tags(NOLOCK) T
		JOIN @TagIds TM ON TM.ContactID = T.TagID
		WHERE T.AccountID = @AccountId and T.IsDeleted=0

END
