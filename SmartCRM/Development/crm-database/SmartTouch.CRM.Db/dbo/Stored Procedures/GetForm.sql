
 CREATE PROCEDURE [dbo].[GetForm]
			@FormID int
       AS
       BEGIN

       	SET NOCOUNT ON;
        
		DECLARE @AllSubmissions INT = 0 
		DECLARE	@UniqueSubmissions INT = 0
		DECLARE @ContactIDS TABLE
		(
			 ContactID INT
	    )

		INSERT INTO @ContactIDS
		SELECT FS.ContactID FROM FormSubmissions (NOLOCK) FS INNER JOIN Contacts(NOLOCK) C ON C.ContactID = FS.ContactID WHERE FormID = @FormID AND C.IsDeleted = 0

		SELECT @AllSubmissions = (SELECT COUNT(ContactID) FROM @ContactIDS)
		SELECT @UniqueSubmissions= (SELECT COUNT(DISTINCT ContactID) FROM @ContactIDS)
		
		SELECT FormID, Name, Acknowledgement, AcknowledgementType, HTMLContent, F.Status, F.AccountID, F.CreatedBy, F.CreatedOn, LastModifiedBy, LastModifiedOn, F.IsDeleted, Submissions, 
		LeadSourceID, @AllSubmissions AllSubmissions, @UniqueSubmissions UniqueSubmissions, IsAPIForm FROM Forms (NOLOCK) F 
		JOIN Accounts (NOLOCK) A ON F.AccountID = A.AccountID WHERE FormID = @FormID AND A.IsDeleted = 0 AND A.Status = 1

		SELECT FormTagID, FormID, TagID FROM FormTags (NOLOCK) 
			WHERE FormID = @FormID


		SELECT DropdownValueID, DropdownID, dv.AccountID, DropdownValue, IsDefault, SortID, IsActive, DropdownValueTypeID, dv.IsDeleted FROM Dropdownvalues (NOLOCK) dv
			INNER JOIN Forms (nolock) f on dv.DropdownValueID = f.LeadSourceID
			WHERE f.FormID = @FormID
       
		SELECT FF.FormFieldID,FF.FormID,FF.FieldID,FF.DisplayName,FF.IsMandatory,FF.SortID,FF.IsDeleted,FF.IsHidden
				,F.FieldInputTypeID ,F.FieldID ,F.Title,F.ValidationMessage,F.ParentID,F.AccountID,F.FieldCode,F.CustomFieldSectionID,F.SortID,F.StatusID,F.IsLeadAdapterField,F.LeadAdapterType
				FROM Fields (NOLOCK) F
						INNER JOIN FormFields (NOLOCK) FF ON F.FieldID = FF.FieldID
						WHERE ff.FormID = @FormID

			SELECT VT.TagID, TagName, [Description], AccountID, CreatedBy, [Count], IsDeleted FROM vTags (NOLOCK) VT
			INNER JOIN FormTags (NOLOCK) FT ON FT.TagID = VT.TagID
			WHERE FT.FormID = @FormID
			
	   END
	   
	   
-- EXEC GETFORM 56421