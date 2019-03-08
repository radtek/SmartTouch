CREATE TYPE [dbo].[MailQueue1] AS TABLE (
    [TokenGuid]            UNIQUEIDENTIFIER NULL,
    [RequestGuid]          UNIQUEIDENTIFIER NULL,
    [From]                 VARCHAR (500)    NULL,
    [PriorityID]           TINYINT          NULL,
    [ScheduledTime]        DATETIME         NULL,
    [QueueTime]            DATETIME         NULL,
    [DisplayName]          NVARCHAR (500)   NULL,
    [ReplyTo]              VARCHAR (MAX)    NULL,
    [To]                   VARCHAR (MAX)    NULL,
    [CC]                   VARCHAR (MAX)    NULL,
    [BCC]                  VARCHAR (MAX)    NULL,
    [Subject]              NVARCHAR (MAX)   NULL,
    [Body]                 NVARCHAR (MAX)   NULL,
    [IsBodyHtml]           BIT              NULL,
    [StatusID]             TINYINT          NULL,
    [ServiceResponse]      VARCHAR (MAX)    NULL,
    [ServiceProviderEmail] VARCHAR (MAX)    NULL);

