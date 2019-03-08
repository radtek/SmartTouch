CREATE PROCEDURE [dbo].[GetContactsByImport] 
	@LeadAdapterJobID INT,
	@recordStatus VARCHAR(20)
AS
BEGIN
	DECLARE @LeadAdapterJobLogID INT
	DECLARE @EntitysTable TABLE
	(
		ID UNIQUEIDENTIFIER
	)

	SELECT @LeadAdapterJobLogID = LeadAdapterJobLogID FROM LeadAdapterJobLogs(NOLOCK) WHERE LeadAdapterJobLogID = @LeadAdapterJobID
	IF (@recordStatus = 'Created')
	BEGIN
		INSERT INTO @EntitysTable
		SELECT ReferenceID AS ID FROM LeadAdapterJobLogDetails(NOLOCK) jld
        WHERE jld.LeadAdapterJobLogID = @LeadAdapterJobLogID AND jld.LeadAdapterRecordStatusID = 1
        AND jld.ReferenceID != '00000000-0000-0000-0000-000000000000'
	END
	ELSE IF (@recordStatus = 'Updated')
	BEGIN
		INSERT INTO @EntitysTable
		SELECT ReferenceID AS ID FROM LeadAdapterJobLogDetails(NOLOCK) jld
        WHERE jld.LeadAdapterJobLogID = @LeadAdapterJobLogID AND jld.LeadAdapterRecordStatusID = 3
        AND jld.ReferenceID != '00000000-0000-0000-0000-000000000000'
	END
	ELSE
	BEGIN
		INSERT INTO @EntitysTable
		SELECT ReferenceID AS ID FROM LeadAdapterJobLogDetails(NOLOCK) jld
        WHERE jld.LeadAdapterJobLogID = @LeadAdapterJobLogID AND jld.ReferenceID != '00000000-0000-0000-0000-000000000000'
	END
	SELECT DISTINCT C.ContactID FROM @EntitysTable ET
    INNER JOIN Contacts(NOLOCK) C ON C.ReferenceID = ET.ID

END

GO


