
CREATE PROCEDURE [dbo].[Get_User_Created_Actions_List]
	-- Add the parameters for the stored procedure here
	@AccountId int,
	@UserID int
AS
BEGIN
	--;WITH cte AS
	--(
	--	SELECT  CA.ActionID, Count(CA.ContactID) as NoOfContacts, MAX (CA.ContactActionMapID) as MaxContactActionMapID
	--	FROM ContactActionMap CA  (nolock)
	--	join Contacts(NOLOCK) C on CA.ContactID = C.ContactID and C.IsDeleted = 0 and C.AccountID = @AccountId
	--	group by CA.ActionID
	--)
	--select A.ActionID, A.ActionDetails, A.ActionDate, 
 --   case when c.contacttype = 1 then 
	--    COALESCE(NULLIF(C.FirstName,'')+' '+ NULLIF(C.LastName,''), NULLIF(C.Company,''),NULLIF(ce.Email,'')) 
	--	    + (case when cte.NoOfContacts > 1 then '...'  else '' end)
	--    else COALESCE(NULLIF(C.Company,''),NULLIF(ce.Email,'')) 
	--	    + (case when cte.NoOfContacts > 1 then '...'  else '' end)
	--    end as FirstName,
 --   C.ContactID, ca.IsCompleted,
	--d.DropDownValue as ActionTypeValue, c.ContactType, c.Company, ce.Email,a.CreatedOn,a.ActionEndTime--,
	----STUFF(
 ----                                           (SELECT ',  '+ U.FirstName +' ' + U.LastName FROM UserActionMap UTM (NOLOCK)
 ----                                               INNER JOIN Users U (NOLOCK) ON U.UserID = UTM.UserID
 ----                                           WHERE UTM.ActionID = A.ActionID FOR XML PATH('')),
 ----                                           1,
 ----                                           1,
 ----                                           '') as UserName
 --   from Actions A (nolock)
 --   join cte on A.ActionID = cte.ActionID 
 --   join ContactActionMap ca (nolock) on ca.ContactActionMapID = cte.MaxContactActionMapID
 --   join Contacts C (nolock) on ca.ContactID = C.ContactID
	--left outer join ContactEmails (nolock) ce on ce.ContactID = c.ContactID and ce.IsPrimary = 1 and ce.IsDeleted = 0
	--join UserActionmap uam (nolock) on uam.actionid = a.actionid  
	--join DropdownValues d (nolock) on d.DropdownValueID = A.ActionType
 --   where A.AccountID = @AccountId and userid = @UserID and C.AccountID=@AccountId
 ;WITH cte AS
                (
                    SELECT  CA.ActionID, Count(CA.ContactID) as NoOfContacts, MAX (CA.ContactActionMapID) as MaxContactActionMapID
                    FROM ContactActionMap CA  (nolock)
                    join Contacts(NOLOCK) C on CA.ContactID = C.ContactID and C.IsDeleted = 0 and C.AccountID = @AccountId
                    group by CA.ActionID
                )
                select A.ActionID, A.ActionDetails, A.ActionDate, 
                case when c.contacttype = 1 then 
	                COALESCE(NULLIF(C.FirstName,'')+' '+ NULLIF(C.LastName,''), NULLIF(C.Company,''),NULLIF(ce.Email,'')) 
		                + (case when cte.NoOfContacts > 1 then '...'  else '' end)
	                else COALESCE(NULLIF(C.Company,''),NULLIF(ce.Email,'')) 
		                + (case when cte.NoOfContacts > 1 then '...'  else '' end)
	                end as FirstName,
               C.ContactID, ca.IsCompleted,
				 d.DropDownValue as ActionTypeValue, c.ContactType, c.Company, ce.Email,a.CreatedOn,a.ActionEndTime
				into #tempActions
                from Actions A (nolock)
                join cte on A.ActionID = cte.ActionID 
                join ContactActionMap ca (nolock) on ca.ContactActionMapID = cte.MaxContactActionMapID
                join Contacts C (nolock) on ca.ContactID = C.ContactID
				left outer join ContactEmails (nolock) ce on ce.ContactID = c.ContactID and ce.IsPrimary = 1 and ce.IsDeleted = 0
				join UserActionmap uam (nolock) on uam.actionid = a.actionid  
				join DropdownValues d (nolock) on d.DropdownValueID = A.ActionType
                where A.AccountID = @AccountId and userid = @UserID and C.AccountID=@AccountId
				order by a.actionid desc


				declare @count int 
                declare @counter int = 1
				                                        
				select @count = count(1) from #tempActions
				create table #assignedTo (actionId int, assignedTo varchar(max))
				while @counter<= @count
					begin
						declare @actionId int
						select top(@counter) @actionId = actionId from #tempActions
						order by actionid desc

						declare @assignedTo varchar(max)
						
						select @assignedTo = coalesce(@assignedTo+',','') + u.FirstName + ' ' + u.LastName
						from UserActionMap (nolock) uam
						inner join Users u (nolock) on u.userid = uam.userid
						where uam.actionid = @actionId

						insert into #assignedTo
						select @actionId, @assignedTo

						set @counter = @counter + 1
						set @assignedTo = null
					end

					select ta.*, ast.assignedTo as UserName from #tempActions ta 
					inner join #assignedTo ast on ast.actionId = ta.actionId
END
GO


