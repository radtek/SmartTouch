CREATE Proc spGetReIndexAccounts
AS
BEGIN
		SELECT ReIndexAccountID,ReIndexModule from ReIndexAccounts
END