
CREATE PROCEDURE [dbo].[GetOpportunitySummary]
	@ContactID INT,
	@Period DATETIME
AS
BEGIN
	DECLARE @Potential INT = 0
	DECLARE @ValueOfPotential INT = 0
	DECLARE @Won INT = 0
	DECLARE @ValueOfWon INT = 0
	DECLARE @LoopCounter INT = 0
	DECLARE @MaxOppId INT
	DECLARE @TempValue INT = 0
	DECLARE @NoPotential TABLE
	(
		PotentialValue INT
	)
	DECLARE @NoWon TABLE
	(
		WonValue INT
	)

	SELECT OCM.* INTO #OppTemp FROM OpportunityContactMap(NOLOCK)	OCM
	INNER JOIN Opportunities(NOLOCK) O ON O.OpportunityID = OCM.OpportunityID
	WHERE OCM.IsDeleted=0 AND OCM.ContactID = @ContactID AND OCM.ExpectedToClose > @Period
	ORDER BY OCM.ExpectedToClose, O.CreatedBy DESC

	SELECT @MaxOppId = MAX(OpportunityID), @LoopCounter = MIN(OpportunityID) FROM #OppTemp
	WHILE (@LoopCounter <= @MaxOppId)
	BEGIN

	   IF OBJECT_ID('TEMPDB..#OppSG') IS NOT NULL 
	   BEGIN
	   DROP TABLE #OppSG
       END

		SELECT TOP 1 OSG.*, T.Potential, T.OpportunityID INTO #OppSG FROM #OppTemp T
		INNER JOIN OpportunityStageGroups(NOLOCK) OSG ON OSG.DropdownValueID = T.StageID
		WHERE OpportunityID = @LoopCounter
		SET @TempValue = 1

		DECLARE @OppSGCount INT
		DECLARE @OppSGID INT
		DECLARE @Pon INT
		SELECT @Pon = Potential FROM #OppSG
		SELECT @OppSGID = OpportunityGroupID FROM #OppSG
		SELECT @OppSGCount = COUNT(1) FROM #OppSG		--Check if #OppSG is empty
		IF (@OppSGCount > 0 AND @OppSGID = 1)
		BEGIN
			SET @Potential = @Potential + 1
			SET @ValueOfPotential = @ValueOfPotential + @Pon
			INSERT INTO @NoPotential
			SELECT OpportunityID FROM #OppSG
		END
		ELSE IF (@OppSGCount > 0 AND @OppSGID = 2)
		BEGIN
			SET @Won = @Won + 1
			SET @ValueOfWon = @ValueOfWon + @Pon
			INSERT INTO @NoWon
			SELECT OpportunityID FROM #OppSG
		END

		SELECT @LoopCounter  = MIN(OpportunityID) FROM #OppTemp
		WHERE OpportunityID > @LoopCounter

		DELETE FROM #OppSG

	END
	DELETE FROM #OppTemp
	--IF(@TempValue ! =0)
	  

	SELECT @Potential AS Potential, @Won AS Won, @ValueOfPotential AS ValueOfPotential, @ValueOfWon AS ValueOfWon
	SELECT * FROM @NoPotential
	SELECT * FROM @NoWon
END

/*
	exec [dbo].[GetOpportunitySummary]
	@ContactID = 1769636,
	@Period = '1/1/1753 12:00:00 AM'

*/