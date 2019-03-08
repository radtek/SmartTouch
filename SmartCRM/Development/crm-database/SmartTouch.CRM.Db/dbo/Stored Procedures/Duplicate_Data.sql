CREATE PROCEDURE [dbo].[Duplicate_Data]
AS

BEGIN
  DECLARE @body nvarchar(MAX),
          @subject varchar(100),
          @DuplicatecCount int, 
    @ContactID INT, 
    --@body  NVARCHAR(MAX)
    @Step1 Nvarchar(MAX),
    @Step2 Nvarchar(MAX),
    @Step3 Nvarchar(MAX)
   DECLARE @NewLine AS CHAR(2) = CHAR(13) + CHAR(10)  
Create table #Table (ID  INT IDENTITY(1,1),AccountID int ,AccountName varchar(100),ObjectName VARCHAR (100), ContactID INT,DuplicatecCount INT,DuplicatecCountSTATUS TINYINT)
  /* For duplicate ContactEmails */
 ;
  WITH TempEmp (AccountID,AccountName, EMAIL, ContactEmailID, ContactID, DuplicatecCount)
  AS (SELECT
    CE.Accountid,
	AccountName,
    EMAIL,
    ContactEmailID,
    ContactID,
    ROW_NUMBER() OVER (PARTITION BY AccountName,CE.AccountID, EMAIL, ContactID ORDER BY ContactEmailID ASC) AS DuplicatecCount
  FROM ContactEmails ce WITH (NOLOCK)
  inner join Accounts a ON A.Accountid =ce.Accountid
  WHERE CE.IsDeleted = 0 and ce.IsPrimary = 1
  )
   INSERT INTO #Table(AccountID,AccountName,ObjectName,ContactID,DuplicatecCount,DuplicatecCountSTATUS)
SELECT AccountID,AccountName,ObjectName = 'ContactEmails',ContactID ,DuplicatecCount,DuplicatecCountSTATUS = (CASE  WHEN DuplicatecCount > 1 THEN 1 ELSE 0 end )
  FROM TempEmp
  WHERE DuplicatecCount > 1

 --/ For duplicate ContactPhoneNumbers /


  SET @DuplicatecCount = 0
  set @ContactID = 0
 
  BEGIN
    ; WITH DupContactPhoneNumbers (Accountid,AccountName,ContactPhoneNumberID,ContactID,PhoneNumber,IsPrimary, DuplicateCount)
  AS 
  (
  SELECT
	CP.Accountid,
	AccountName,
	ContactPhoneNumberID,
    ContactID,
    PhoneNumber,
	IsPrimary,
    ROW_NUMBER() OVER (PARTITION BY CP.Accountid,AccountName,ContactID, PhoneNumber,IsPrimary ORDER BY ContactPhoneNumberID) AS DuplicateCount
  FROM SmartCRM.dbo.ContactPhoneNumbers cp WITH (NOLOCK) 
  inner join Accounts a ON A.Accountid = cp.Accountid
  WHERE CP.IsDeleted = 0  and CP.IsPrimary = 1
  )
   INSERT INTO #Table(Accountid,AccountName,ObjectName,ContactID,DuplicatecCount,DuplicatecCountSTATUS)
SELECT AccountID,AccountName,ObjectName = 'ContactPhoneNumbers',ContactID ,DuplicateCount,DuplicatecCountSTATUS = (CASE  WHEN DuplicateCount > 1 THEN 1 ELSE 0 end )
  FROM DupContactPhoneNumbers
  WHERE DuplicateCount > 1
END

--  --/ For duplicate ContactPhoneNumbers /


  SET @DuplicatecCount = 0
  set @ContactID = 0
  BEGIN
    ;
    WITH DupLeadSource (Accountid,AccountName,ContactID, ContactLeadSourceMapID, DuplicateCount)
    AS (SELECT 
	C.Accountid,
	AccountName,
      CL.ContactID,
      ContactLeadSourceMapID,
      ROW_NUMBER() OVER (PARTITION BY C.Accountid,AccountName,CL.ContactID ORDER BY ContactLeadSourceMapID) AS DuplicateCount
    FROM SmartCRM.dbo.ContactLeadSourceMap cl
	inner join Contacts c on c.ContactID = CL.ContactID
	INNER JOIN Accounts A ON A.AccountID = C.AccountID
    WHERE IsPrimaryLeadSource = 1 and c.IsDeleted = 0)
   INSERT INTO #Table(Accountid,AccountName,ObjectName,ContactID,DuplicatecCount,DuplicatecCountSTATUS)
SELECT AccountID,AccountName,ObjectName = 'ContactLeadSourceMap',ContactID ,DuplicateCount,DuplicatecCountSTATUS = (CASE  WHEN DuplicateCount > 1 THEN 1 ELSE 0 end )
  FROM DupLeadSource
  WHERE DuplicateCount > 1
 END

 DECLARE @bodyMsg nvarchar(max)
DECLARE @tableHTML nvarchar(max)

SET @subject = 'Query Results in HTML with CSS'


SET @tableHTML = 
N'<style type="text/css">
#box-table
{
font-family: "Lucida Sans Unicode", "Lucida Grande", Sans-Serif;
font-size: 12px;
text-align: center;
border-collapse: collapse;
border-top: 7px solid #9baff1;
border-bottom: 7px solid #9baff1;
}
#box-table th
{
font-size: 13px;
font-weight: normal;
background: #b9c9fe;
border-right: 2px solid #9baff1;
border-left: 2px solid #9baff1;
border-bottom: 2px solid #9baff1;
color: #039;
}
#box-table td
{
border-right: 1px solid #aabcfe;
border-left: 1px solid #aabcfe;
border-bottom: 1px solid #aabcfe;
color: #669;
}
tr:nth-child(odd)	{ background-color:#eee; }
tr:nth-child(even)	{ background-color:#fff; }	
</style>'+	
N'<H3><font color="Red">All Rows From ContactEmails,ContactPhoneNumbers,ContactLeadSourceMap Tables </H3>' +
N'<table id="box-table" >' +
N'<tr><font color="Green"><th>ID</th>
<th>AccountID</th>  
<th>AccountName</th>
<th>ContactID</th>
<th>ContactEmails</th>
<th>ContactPhoneNumbers</th>
<th>ContactLeadSourceMap</th>
</tr>' +
CAST ( ( 
SELECT top 50  td = CAST([ID] AS VARCHAR(100)),'  ',
td =  AccountID,'  ',
td =  AccountName,'  ',
td =  ContactID,'  ',
td = (case when ObjectName = 'ContactEmails' then 1 else 0 end),'  ',
td =(case when ObjectName = 'ContactPhoneNumbers' then 1 else 0 end)  ,'  ',
td = (case when ObjectName = 'ContactLeadSourceMap' then 1 else 0 end) ,'  '
 FROM 	#Table
ORDER BY [ID]
FOR XML PATH('tr'), TYPE 
) AS NVARCHAR(MAX) ) +
N'</table>' 


  IF (Select COUNT(*) From #Table) > 1
 begin


    EXEC msdb..sp_send_dbmail @profile_name = 'ST-Stage-Notification',
                              @recipients = 'santosh.srinivas@landmarkit.in;ravindra.challagandla@landmarkit.in;manohar.pathapati@landmarkit.in;saikrishna.mandali@landmarkit.in',
                              --  @recipients = 'manohar.pathapati@landmarkit.in;santosh.srinivas@landmarkit.in;haripratap.elduri@landmarkit.in;ravindra.challagandla@landmarkit.in',
                              --   @recipients = 'manohar.pathapati@landmarkit.in;narendra.mangala@landmarkit.in;prabhakara.challuri@landmarkit.in;haripratap.elduri@landmarkit.in;pallavi.kovvuri@landmarkit.in;ravi.nukamreddy@landmarkit.in;dba@landmarkit.in;sysadmin@landmarkit.in',

                              @body =  @tableHTML,
                              @subject = 'Idintified  duplicate  FROM Stage server',
                              @body_format = 'HTML'
                           

  END
  IF (Select COUNT(*) From #Table) < 1
  BEGIN
    PRINT 'There are no Duplicate data found'
  END


END

--EXEC [dbo].[Duplicate_Data]
GO

