CREATE TABLE [dbo].[SpamValidators](
	[SpamValidatorID] [tinyint] IDENTITY(1,1) NOT NULL,
	[Validator] [varchar](30) NOT NULL,
	[Value] [varchar](max) NULL,
	[AccountID] [int] NULL,
	[RunValidation] [bit] NOT NULL,
	[Order] [int] NOT NULL,
 CONSTRAINT [PK_SpamValidators] PRIMARY KEY CLUSTERED 
(
	[SpamValidatorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]