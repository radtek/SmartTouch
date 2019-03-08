
CREATE TABLE [dbo].[AccountSettings](
	[AccountSettingsID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[StatusID] [smallint] NULL,
	[viewName] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountSettingsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
    
GO




