

CREATE function [dbo].[getPhoneNumberTable](@contactId int, @accountId int, @phoneData varchar(2000))
returns @phones table (ContactId int, AccountId int, PhoneType varchar(20), PhoneNumber varchar(20))
as
begin

	DECLARE @phoneRawList table(Val varchar(50))

	IF LEN(@phoneData) > 0
	BEGIN
		insert into @phoneRawList
		SELECT * FROM Split(@phoneData, '~')

		INSERT INTO @phones
		select @contactId, @accountId, SUBSTRING(Val, 1, CHARINDEX('|', Val)-1), SUBSTRING(Val, CHARINDEX('|', Val)+1, LEN(Val))  from @phoneRawList
	END
	RETURN;
end

