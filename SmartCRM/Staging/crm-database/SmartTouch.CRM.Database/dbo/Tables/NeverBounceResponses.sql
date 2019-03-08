CREATE TABLE [dbo].[NeverBounceResponses] (
    [NeverBounceResponseID] INT           IDENTITY (1, 1) NOT NULL,
    [NeverBounceRequestID]  INT           NOT NULL,
    [ResponseJSON]          VARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([NeverBounceResponseID] ASC),
    FOREIGN KEY ([NeverBounceRequestID]) REFERENCES [dbo].[NeverBounceRequests] ([NeverBounceRequestID])
);

