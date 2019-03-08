
CREATE procedure [dbo].[GetWorkflowCampaignActionLinks]
@AccountId int

AS
select distinct CM.Name + ' < ' + cl.url  + ' > (' +convert(varchar(10), cl.LinkIndex) + ')' LinkText, CM.Name as CampaignName, CM.CampaignId, CL.CampaignLinkId, cl.LinkIndex 
FROM WorkflowActions WA (NOLOCK)--CM.CampaignID, WCAL.LinkID, 
		INNER JOIN Workflows W (NOLOCK) ON W.WorkflowID = WA.WorkflowID
		INNER JOIN WorkflowCampaignActions WCA (NOLOCK) ON WA.WorkflowActionID = WCA.WorkflowActionID
		INNER JOIN Campaigns (NOLOCK) CM ON CM.CampaignID = WCA.CampaignID
		INNER JOIN WorkflowCampaignActionLinks (NOLOCK) WCAL ON  WCAL.ParentWorkflowActionID = WCA.WorkflowCampaignActionID
		INNER JOIN CampaignLinks(NOLOCK) CL ON CL.CampaignID = CM.CAMPAIGNID
		WHERE CM.IsDeleted = 0 AND CM.CampaignStatusID = 107 AND CM.AccountID = @AccountId AND WA.IsDeleted = 0 AND W.IsDeleted = 0


order by 
	CampaignName , cl.LinkIndex 

--Need to add a filter to exclude deleted workflows
--EXEC GetWorkflowCampaignActionLinks 4218