
CREATE PROCEDURE [dbo].[Get_Database_Lifecycle_Report_ContactInfo]
	-- Add the parameters for the stored procedure here
	 @Entities VARCHAR(MAX),
	 @AccountID INT,
     @OwnerIds VARCHAR(MAX),
     @StartDate DATETIME,
     @EndDate DATETIME,
	 @IsAdmin BIT
AS
BEGIN
		DECLARE @UserIds TABLE(USERID INT)
		INSERT INTO @UserIds SELECT * FROM dbo.Split_2(@OwnerIds,',')
		IF @IsAdmin = 1
			BEGIN
				INSERT INTO @UserIds SELECT NULL
			END
		SELECT ContactID ,'' as firstname, '' as lastname, '' as email, '' as phonenumber FROM Contacts (NOLOCK) C
		INNER JOIN @UserIDs U ON COALESCE(U.USERID,0) = COALESCE(C.OwnerID,0)
		WHERE LifecycleStage IN  ((SELECT DataValue FROM dbo.Split(@Entities, ',')))
		AND LastUpdatedOn BETWEEN @StartDate AND @EndDate AND AccountID=@AccountID AND IsDeleted = 0
END


/*
	EXEC [dbo].[Get_Database_Lifecycle_Report_ContactInfo]
	 @Entities  = '4664',
     @OwnerIds  = '5493,5497,5505,6518,6535,6574,6604,6608,6799,6800,6802,6803,6809,6822,6834,6842,6849,6868,6869,6876,6877,6878,6880,6882,6883,6885,6886,6887,6889,6890,6891,6899,6907,6908,6909,6910,6911,6912,6913,6914,6915,6916,6923,6927,7941,7942,7943,7949,7979,7980,7984,7985,7986,7987,7995,7996,7997,7998,7999,8014,8234,8285,8286,8287,8288,8289,8299,8316,8317,8320,8975,8976,8978,8980,8981,8982,8983',
     @StartDate  = '1/18/2016 5:54:36 PM',
     @EndDate  = '5/25/2016 5:54:36 PM',
	 @AccountID = 4218

*/

GO


