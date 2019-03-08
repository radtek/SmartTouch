CREATE TABLE [dbo].[NeverBounceEmailStatus] (
    [NeverBounceEmailStatusID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]                INT      NOT NULL,
    [EmailStatus]              SMALLINT NOT NULL,
    [NeverBounceRequestID]     INT      NOT NULL,
    [CreatedOn]                DATETIME NOT NULL,
    [UpdatedOn]                DATETIME NULL,
    [ContactEmailID]           INT      NULL,
    PRIMARY KEY CLUSTERED ([NeverBounceEmailStatusID] ASC),
    FOREIGN KEY ([NeverBounceRequestID]) REFERENCES [dbo].[NeverBounceRequests] ([NeverBounceRequestID])
);

