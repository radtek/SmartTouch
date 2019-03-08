



CREATE FUNCTION [dbo].[GetInfo]
(
	@AccountID INT,
	@Type VARCHAR(20),
	@SelectedList VARCHAR(MAX)
)
RETURNS @Info TABLE ( ID int, Name VARCHAR(255) )
AS
BEGIN
	IF(@Type = 'User')
	BEGIN
		INSERT INTO @Info
		SELECT	UserID, isnull(FirstName, '') + ' ' + isnull(LastName, '')
		FROM	dbo.users u INNER JOIN dbo.Split(@SelectedList, ',') l on u.UserID = l.DataValue
		WHERE	u.AccountID IN (@AccountID, 1)
	END
	ELSE IF(@Type = 'Community')
	BEGIN
		INSERT INTO @Info
		SELECT	dv.DropdownValueID, dv.DropdownValue
		FROM	DBO.DropdownValues dv INNER JOIN dbo.Split(@SelectedList, ',') i on dv.DropdownValueID = i.DataValue
		WHERE	accountID = @AccountID
				AND DropdownID = 7
				AND isnull(IsActive, 1) = 1
	END
	ELSE IF(@Type = 'TourType')
	BEGIN
		INSERT INTO @Info
		SELECT	dv.DropdownValueID, dv.DropdownValue
		FROM	DBO.DropdownValues dv INNER JOIN dbo.Split(@SelectedList, ',') i on dv.DropdownValueID = i.DataValue
		WHERE	accountID = @AccountID
				AND DropdownID = 8
				AND isnull(IsActive, 1) = 1
	END
	ELSE IF(@Type = 'LeadSource')
	BEGIN
		INSERT INTO @Info
		SELECT	dv.DropdownValueID, dv.DropdownValue
		FROM	DBO.DropdownValues dv INNER JOIN dbo.Split(@SelectedList, ',') i on dv.DropdownValueID = i.DataValue
		WHERE	accountID = @AccountID
				AND DropdownID = 5
				AND isnull(IsActive, 1) = 1
	END
	ELSE IF(@Type = 'LifeCycle')
	BEGIN
		INSERT INTO @Info
		SELECT	dv.DropdownValueID, dv.DropdownValue
		FROM	DBO.DropdownValues dv INNER JOIN dbo.Split(@SelectedList, ',') i on dv.DropdownValueID = i.DataValue
		WHERE	accountID = @AccountID
				AND DropdownID = 3
				AND isnull(IsActive, 1) = 1
	END

	RETURN
END






