CREATE TABLE [dbo].[OpportunityActionMap_Audit] (
    [AuditId]                BIGINT        IDENTITY (1, 1) NOT NULL,
    [OpportunityActionMapID] INT           NOT NULL,
    [OpportunityID]          INT           NOT NULL,
    [ActionID]               INT           NOT NULL,
    [IsCompleted]            BIT           NULL,
    [AuditAction]            CHAR (1)      NOT NULL,
    [AuditDate]              DATETIME      CONSTRAINT [DF_OpportunityActionMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]              VARCHAR (50)  CONSTRAINT [DF_OpportunityActionMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]               VARCHAR (128) CONSTRAINT [DF_OpportunityActionMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]            BIT           NOT NULL,
    [IsBuyer]                BIT           CONSTRAINT [IsBuyer_Audit_Defualt] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_OpportunityActionMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);

