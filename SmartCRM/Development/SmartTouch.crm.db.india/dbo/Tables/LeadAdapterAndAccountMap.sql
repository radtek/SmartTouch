﻿CREATE TABLE [dbo].[LeadAdapterAndAccountMap] (
    [LeadAdapterAndAccountMapID] INT              IDENTITY (1, 1) NOT NULL,
    [LeadAdapterTypeID]          TINYINT          NOT NULL,
    [AccountID]                  INT              NOT NULL,
    [BuilderNumber]              NVARCHAR (MAX)   NOT NULL,
    [ArchivePath]                NVARCHAR (200)   NULL,
    [LocalFilePath]              NVARCHAR (200)   NULL,
    [CreatedBy]                  INT              NOT NULL,
    [CreatedDateTime]            DATETIME         NOT NULL,
    [ModifiedBy]                 INT              NULL,
    [ModifiedDateTime]           DATETIME         NULL,
    [RequestGuid]                UNIQUEIDENTIFIER NOT NULL,
    [IsDelete]                   BIT              NULL,
    [LeadAdapterErrorStatusID]   TINYINT          NULL,
    [LastProcessed]              DATETIME         NULL,
    [LeadAdapterServiceStatusID] SMALLINT         NULL,
    [LeadSourceType]             SMALLINT         NULL,
    [CommunityNumber]            NVARCHAR (MAX)   DEFAULT ('') NULL,
    CONSTRAINT [PK_LeadAdapterAndAccountMap] PRIMARY KEY CLUSTERED ([LeadAdapterAndAccountMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_DropdownValues] FOREIGN KEY ([LeadSourceType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_LeadAdapterErrorStatus] FOREIGN KEY ([LeadAdapterErrorStatusID]) REFERENCES [dbo].[LeadAdapterErrorStatus] ([LeadAdapterErrorStatusID]),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_Statuses] FOREIGN KEY ([LeadAdapterServiceStatusID]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_LeadAdapterAndAccountMap_Users1] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);

