CREATE TABLE [dbo].[FTPRegistration] (
[FTPRegistrationID] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Host] [nvarchar](200) NOT NULL,
	[UserName] [nvarchar](100) NULL,
	[Password] [nvarchar](100) NULL,
	[Port] [int] NULL,
	[EnableSSL] [bit] NULL,
    CONSTRAINT [PK_FTPRegistration] PRIMARY KEY CLUSTERED ([FTPRegistrationID] ASC),
    CONSTRAINT [FK_FTPRegistration_FTPRegistration] FOREIGN KEY ([FTPRegistrationID]) REFERENCES [dbo].[FTPRegistration] ([FTPRegistrationID])
);

