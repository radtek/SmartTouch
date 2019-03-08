
-- =============================================
-- Author:		<Author,,Vadalisetty Surendra Babu>
-- Create date: <Create Date, ,11-01-2017>
-- Description:	<Description, ,For display phone format>
-- =============================================
CREATE FUNCTION [dbo].[GetPhoneFormart]
(
	@PHONE VARCHAR(50),
    @PhoneType SMALLINT,
    @CountryCode VARCHAR(4) ,
    @Extention VARCHAR(5) 
)
RETURNS VARCHAR(100)
AS
BEGIN
	
	DECLARE @TYPE VARCHAR(50)

	SELECT @TYPE = DropdownValue FROM DropdownValues(NOLOCK) WHERE DropdownValueID=@PhoneType
	
	RETURN  CASE WHEN (@CountryCode IS NOT NULL AND @CountryCode != '')  THEN '+' + @CountryCode + ' ' ELSE '' END
            + @PHONE + CASE WHEN (@Extention IS NOT NULL AND @Extention != '') THEN ' Ext.' + @Extention ELSE '' END +'(' + @TYPE + ')'

END

