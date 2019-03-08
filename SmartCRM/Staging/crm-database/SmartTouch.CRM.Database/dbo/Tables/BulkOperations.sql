﻿CREATE TABLE [dbo].[BulkOperations] (
    [BulkOperationID]        INT            IDENTITY (1, 1) NOT NULL,
    [OperationType]          INT            NOT NULL,
    [OperationID]            INT            NOT NULL,
    [SearchCriteria]         VARCHAR (MAX)  NULL,
    [SearchDefinitionID]     INT            NULL,
    [AdvancedSearchCriteria] VARCHAR (MAX)  NULL,
    [AccountID]              INT            NOT NULL,
    [UserID]                 INT            NOT NULL,
    [RoleID]                 SMALLINT       NOT NULL,
    [AccountPrimaryEmail]    NVARCHAR (MAX) NULL,
    [AccountDomain]          NVARCHAR (MAX) NULL,
    [RelationType]           INT            NULL,
    [ExportSelectedFields]   NVARCHAR (MAX) NULL,
    [ExportType]             INT            NULL,
    [DateFormat]             NVARCHAR (50)  NULL,
    [TimeZone]               NVARCHAR (50)  NULL,
    [ExportFileKey]          NVARCHAR (50)  NULL,
    [ExportFileName]         NVARCHAR (50)  NULL,
    [UserEmailID]            NVARCHAR (50)  NULL,
    [CreatedOn]              DATETIME       NULL,
    [Status]                 TINYINT        DEFAULT ((1)) NULL,
    [ActionCompleted]        BIT            NULL,
    CONSTRAINT [PK_BulkOperations] PRIMARY KEY CLUSTERED ([BulkOperationID] ASC) WITH (FILLFACTOR = 90)
);
