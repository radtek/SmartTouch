﻿CREATE TABLE [dbo].[SpamKeyWords](
	[SpamKeyWordID] [int] IDENTITY(1,1) NOT NULL,
	[Value] [varchar](max) NOT NULL,
 CONSTRAINT [PK_SpamKeyWords] PRIMARY KEY CLUSTERED 
(
	[SpamKeyWordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]