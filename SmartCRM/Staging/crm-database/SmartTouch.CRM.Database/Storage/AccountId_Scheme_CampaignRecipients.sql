﻿CREATE PARTITION SCHEME [AccountId_Scheme_CampaignRecipients]
    AS PARTITION [AccountIdList]
    TO ([CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup], [CampaignRecipientsGroup]);
