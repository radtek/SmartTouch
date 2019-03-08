
CREATE TABLE [dbo].[FacebookLeadgen](
	[FacebookLeadGenID] [int] IDENTITY(1,1) NOT NULL,
	[AdGroupID] [bigint] NOT NULL CONSTRAINT [df_adgroupid]  DEFAULT ((0)),
	[AdID] [bigint] NOT NULL CONSTRAINT [df_adid]  DEFAULT ((0)),
	[LeadGenID] [bigint] NOT NULL CONSTRAINT [df_leadgenid]  DEFAULT ((0)),
	[PageID] [bigint] NOT NULL CONSTRAINT [df_pageid]  DEFAULT ((0)),
	[FormID] [bigint] NOT NULL CONSTRAINT [df_formid]  DEFAULT ((0)),
	[IsProcessed] [bit] NOT NULL DEFAULT ((0)),
	[CreatedDate] [datetime] NOT NULL DEFAULT (getutcdate()),
	[RawData] [varchar](max) NULL,
	[Remarks] [varchar](1000) NULL,
	 CONSTRAINT [PK_FacebookLeadgen] PRIMARY KEY CLUSTERED ([FacebookLeadGenID] ASC) WITH (FILLFACTOR = 90) ON [PRIMARY]);

GO



