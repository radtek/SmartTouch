﻿CREATE TABLE [dbo].[BouncedEmailData] (
    [BouncedEmailDataID] INT            IDENTITY (1, 1) NOT NULL,
    [Email]              VARCHAR (256)  NOT NULL,
    [AccountID]          INT            NOT NULL,
    [StatusID]           SMALLINT       NOT NULL,
    [SentOn]             DATETIME       NOT NULL,
    [ContactID]          INT            NOT NULL,
    [BouncedReason]      NVARCHAR (MAX) NULL
);


GO
CREATE NONCLUSTERED INDEX [BouncedEmailData_All_Columns]
    ON [dbo].[BouncedEmailData]([Email] ASC, [AccountID] ASC, [StatusID] ASC, [SentOn] ASC, [ContactID] ASC)
    INCLUDE([BouncedEmailDataID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [BouncedEmailData_SentOn]
    ON [dbo].[BouncedEmailData]([SentOn] ASC)
    INCLUDE([StatusID], [BouncedEmailDataID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [BouncedEmailData_AccountID]
    ON [dbo].[BouncedEmailData]([AccountID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [BouncedEmailData_SentOn_AccountID]
    ON [dbo].[BouncedEmailData]([SentOn] ASC)
    INCLUDE([AccountID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20170510-110053]
    ON [dbo].[BouncedEmailData]([AccountID] ASC, [StatusID] ASC, [SentOn] ASC)
    INCLUDE([Email]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);
