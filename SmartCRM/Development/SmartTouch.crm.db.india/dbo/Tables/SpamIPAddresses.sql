﻿CREATE TABLE [dbo].[SpamIPAddresses](
	[SpamIPAddressID] [int] IDENTITY(1,1) NOT NULL,
	[IsSpam] [bit] NOT NULL,
	[AccountID] [int] NULL,
	[IPAddress] [binary](4) NULL,
 CONSTRAINT [PK_SpamIPAddresses] PRIMARY KEY CLUSTERED 
(
	[SpamIPAddressID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
