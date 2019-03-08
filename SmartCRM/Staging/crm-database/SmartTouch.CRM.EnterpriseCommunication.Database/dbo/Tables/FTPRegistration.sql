CREATE TABLE [dbo].[FTPRegistration] (
    [FTPRegistrationID] INT              IDENTITY (1, 1) NOT NULL,
    [Guid]              UNIQUEIDENTIFIER NOT NULL,
    [Host]              NVARCHAR (200)   NOT NULL,
    [UserName]          NVARCHAR (100)   NULL,
    [Password]          NVARCHAR (100)   NULL,
    [Port]              INT              NULL,
    [EnableSSL]         BIT              NULL,
    CONSTRAINT [PK_FTPRegistration] PRIMARY KEY CLUSTERED ([FTPRegistrationID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_FTPRegistration_FTPRegistration] FOREIGN KEY ([FTPRegistrationID]) REFERENCES [dbo].[FTPRegistration] ([FTPRegistrationID])
);

