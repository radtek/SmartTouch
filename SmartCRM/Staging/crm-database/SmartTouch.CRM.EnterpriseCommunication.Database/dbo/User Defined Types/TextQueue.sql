CREATE TYPE [dbo].[TextQueue] AS TABLE (
    [TextResponseID]  INT              NULL,
    [From]            VARCHAR (75)     NULL,
    [To]              VARCHAR (75)     NULL,
    [SenderID]        VARCHAR (75)     NULL,
    [Message]         VARCHAR (200)    NULL,
    [Status]          BIT              NULL,
    [ServiceResponse] VARCHAR (MAX)    NULL,
    [RequestGuid]     UNIQUEIDENTIFIER NULL,
    [TokenGuid]       UNIQUEIDENTIFIER NULL,
    [ScheduledTime]   DATETIME         NULL);

