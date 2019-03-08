CREATE TABLE [dbo].[NeverBounceMappings] (
    [NeverBounceMappingID]  INT     IDENTITY (1, 1) NOT NULL,
    [NeverBounceRequestID]  INT     NULL,
    [EntityID]              INT     NOT NULL,
    [NeverBounceEntityType] TINYINT NOT NULL,
    PRIMARY KEY CLUSTERED ([NeverBounceMappingID] ASC),
    FOREIGN KEY ([NeverBounceRequestID]) REFERENCES [dbo].[NeverBounceRequests] ([NeverBounceRequestID])
);

