
CREATE PROC [dbo].[GetLastSentCampaign]
AS
BEGIN
	SELECT TOP 1 C.name,SP.providername as VMTA,C.processeddate as LastCampaignSentDate FROM campaigns (NOLOCK) C
JOIN serviceproviders (NOLOCK) SP ON SP.serviceproviderid=C.serviceproviderid
WHERE C.campaignstatusid=105 AND C.ISDELETED=0 ORDER BY C.processeddate DESC
END