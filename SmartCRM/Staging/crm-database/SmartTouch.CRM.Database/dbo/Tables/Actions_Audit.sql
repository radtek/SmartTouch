CREATE TABLE [dbo].[Actions_Audit] (
    [AuditId]            BIGINT           IDENTITY (1, 1) NOT NULL,
    [ActionID]           INT              NOT NULL,
    [ActionDetails]      NVARCHAR (1000)  NOT NULL,
    [RemindOn]           DATETIME         NULL,
    [CreatedBy]          INT              NOT NULL,
    [CreatedOn]          DATETIME         NOT NULL,
    [NotificationStatus] TINYINT          NULL,
    [RemindbyText]       BIT              NULL,
    [RemindbyEmail]      BIT              NULL,
    [RemindbyPopup]      BIT              NULL,
    [AuditAction]        CHAR (1)         NOT NULL,
    [AuditDate]          DATETIME         CONSTRAINT [DF_Actions_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]          VARCHAR (50)     CONSTRAINT [DF_Actions_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]           VARCHAR (128)    CONSTRAINT [DF_Actions_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]        BIT              NOT NULL,
    [EmailRequestGuid]   UNIQUEIDENTIFIER NULL,
    [TextRequestGuid]    UNIQUEIDENTIFIER NULL,
    [AccountID]          INT              NULL,
    [LastUpdatedBy]      INT              NULL,
    [LastUpdatedOn]      DATETIME         NULL,
    [ActionType]         SMALLINT         NULL,
    [ActionDate]         DATETIME         NULL,
    [ActionStartTime]    DATETIME         NULL,
    [ActionEndTime]      DATETIME         NULL,
    CONSTRAINT [PK_Actions_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_Actions_Audit_ActionID_ActionDetails_CreatedBy]
    ON [dbo].[Actions_Audit]([ActionID] ASC)
    INCLUDE([ActionDetails], [CreatedBy], [AuditAction], [AuditDate], [AuditStatus]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

