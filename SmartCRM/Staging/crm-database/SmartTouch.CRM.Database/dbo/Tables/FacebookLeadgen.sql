CREATE TABLE [dbo].[FacebookLeadgen] (
    [FacebookLeadGenID] INT            IDENTITY (1, 1) NOT NULL,
    [AdGroupID]         BIGINT         CONSTRAINT [df_adgroupid] DEFAULT ((0)) NOT NULL,
    [AdID]              BIGINT         CONSTRAINT [df_adid] DEFAULT ((0)) NOT NULL,
    [LeadGenID]         BIGINT         CONSTRAINT [df_leadgenid] DEFAULT ((0)) NOT NULL,
    [PageID]            BIGINT         CONSTRAINT [df_pageid] DEFAULT ((0)) NOT NULL,
    [FormID]            BIGINT         CONSTRAINT [df_formid] DEFAULT ((0)) NOT NULL,
    [IsProcessed]       BIT            DEFAULT ((0)) NOT NULL,
    [CreatedDate]       DATETIME       DEFAULT (getutcdate()) NOT NULL,
    [RawData]           VARCHAR (MAX)  NULL,
    [Remarks]           VARCHAR (1000) NULL,
    CONSTRAINT [PK_FacebookLeadgen] PRIMARY KEY CLUSTERED ([FacebookLeadGenID] ASC) WITH (FILLFACTOR = 90)
);

