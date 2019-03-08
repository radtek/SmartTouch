

CREATE PROCEDURE [dbo].[InsertToSmartSearchQueue]
	
AS
BEGIN
	
	INSERT INTO SmartSearchQueue(SearchDefinitionID,IsProcessed,CreatedOn,AccountID)
	SELECT SearchDefinitionID,0 AS IsProcessed,GETUTCDATE() AS CreatedOn, AccountID FROM SearchDefinitions(NOLOCK) WHERE IsPreConfiguredSearch = 0 and selectallsearch = 0 and IsFavoriteSearch = 0
    
END

