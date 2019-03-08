CREATE TABLE [dbo].[Tours_Audit] (
    [AuditId]            BIGINT           IDENTITY (1, 1) NOT NULL,
    [TourID]             INT              NOT NULL,
    [TourDetails]        NVARCHAR (1000)  NULL,
    [CommunityID]        SMALLINT         NOT NULL,
    [TourDate]           DATETIME         NOT NULL,
    [TourType]           SMALLINT         NOT NULL,
    [ReminderDate]       DATETIME         NULL,
    [CreatedBy]          INT              NOT NULL,
    [CreatedOn]          DATETIME         NOT NULL,
    [NotificationStatus] TINYINT          NULL,
    [RemindbyText]       BIT              NULL,
    [RemindbyEmail]      BIT              NULL,
    [RemindbyPopup]      BIT              NULL,
    [AuditAction]        CHAR (1)         NOT NULL,
    [AuditDate]          DATETIME         CONSTRAINT [DF_Tours_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]          VARCHAR (50)     CONSTRAINT [DF_Tours_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]           VARCHAR (128)    CONSTRAINT [DF_Tours_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]        BIT              NOT NULL,
    [EmailRequestGuid]   UNIQUEIDENTIFIER NULL,
    [TextRequestGuid]    UNIQUEIDENTIFIER NULL,
    [AccountID]          INT              NULL,
    [LastUpdatedBy]      INT              NULL,
    [LastUpdatedOn]      DATETIME         NULL,
    CONSTRAINT [PK_Tours_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);


GO
CREATE NONCLUSTERED INDEX [IX_Tours_Audit_AccountID]
    ON [dbo].[Tours_Audit]([AccountID] ASC)
    INCLUDE([TourID], [TourDetails], [AuditAction], [AuditDate], [LastUpdatedBy]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tours_Audit_TourID]
    ON [dbo].[Tours_Audit]([TourID] ASC)
    INCLUDE([AuditId]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Tours_Audit_TourID_AccountID]
    ON [dbo].[Tours_Audit]([TourID] ASC, [AccountID] ASC)
    INCLUDE([TourDetails], [AuditAction], [AuditDate], [LastUpdatedBy]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];

