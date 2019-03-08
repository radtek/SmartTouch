CREATE TABLE [dbo].[UserSocialMediaPosts] (
    [UserSocialMediaPostID] INT            IDENTITY (1, 1) NOT NULL,
    [CampaignID]            INT            NULL,
    [UserID]                INT            NOT NULL,
    [Post]                  VARCHAR (MAX)  NOT NULL,
    [AttachmentPath]        VARCHAR (1024) NULL,
    [CommunicationType]     VARCHAR (10)   NOT NULL,
    CONSTRAINT [PK_UserSocialMediaPosts] PRIMARY KEY CLUSTERED ([UserSocialMediaPostID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_UserSocialMediaPosts_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_UserSocialMediaPosts_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

