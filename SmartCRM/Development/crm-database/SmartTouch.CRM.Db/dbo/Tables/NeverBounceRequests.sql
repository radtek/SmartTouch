CREATE TABLE [dbo].[NeverBounceRequests] (
    [NeverBounceRequestID] INT            IDENTITY (1, 1) NOT NULL,
    [ReviewedBy]           INT            NULL,
    [ReviewedOn]           DATETIME       NULL,
    [ServiceStatus]        SMALLINT       NOT NULL,
    [AccountID]            INT            NOT NULL,
    [Remarks]              VARCHAR (MAX)  NULL,
    [EmailsCount]          INT            NULL,
    [ScheduledPollingTime] DATETIME       NULL,
    [NeverBounceJobID]     INT            NULL,
    [PollingRemarks]       NVARCHAR (MAX) NULL,
    [PollingStatus]        TINYINT        NULL,
    [CreatedOn]            DATETIME       NOT NULL,
    [CreatedBy]            INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([NeverBounceRequestID] ASC),
	CONSTRAINT [NeverBounceRequests_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [NeverBounceRequests_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [NeverBounceRequests_Users] FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[Users] ([UserID])
   
);
GO

