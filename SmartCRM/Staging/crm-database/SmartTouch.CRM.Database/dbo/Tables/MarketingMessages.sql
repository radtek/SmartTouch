CREATE TABLE [dbo].[MarketingMessages] (
    [MarketingMessageID]    INT           IDENTITY (1, 1) NOT NULL,
    [MarketingMessageTitle] VARCHAR (128) NULL,
    [TimeInterval]          TINYINT       NULL,
    [Status]                SMALLINT      NULL,
    [SelectedBy]            TINYINT       NOT NULL,
    [IsDeleted]             BIT           NOT NULL,
    [CreatedBy]             INT           NOT NULL,
    [LastUpdatedBy]         INT           NOT NULL,
    [CreatedDate]           DATETIME      NOT NULL,
    [LastUpdatedDate]       DATETIME      NOT NULL,
    [ScheduleFrom]          DATETIME      NULL,
    [ScheduleTo]            DATETIME      NULL,
    CONSTRAINT [PK_MarketingMessages] PRIMARY KEY CLUSTERED ([MarketingMessageID] ASC) WITH (FILLFACTOR = 90)
);

