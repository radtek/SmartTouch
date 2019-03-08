CREATE TABLE [dbo].[TEMP] (
    [AccountName]           NVARCHAR (100) NULL,
    [SenderReputationCount] INT            NULL,
    [CampaignsSent]         INT            NULL,
    [Recipients]            INT            NULL,
    [Sent]                  INT            NULL,
    [Delivered]             INT            NULL,
    [Bounced]               VARCHAR (252)  NULL,
    [Opened]                VARCHAR (302)  NULL,
    [Clicked]               VARCHAR (302)  NULL,
    [TagsAll]               INT            NULL,
    [TagsActive]            INT            NULL,
    [SSAll]                 INT            NULL,
    [SSActive]              INT            NULL
);

