CREATE TABLE [dbo].[ImportContactData] (
    [ImportContactDataID]      INT              IDENTITY (1, 1) NOT NULL,
    [FirstName]                NVARCHAR (4000)  NULL,
    [LastName]                 NVARCHAR (4000)  NULL,
    [CompanyName]              NVARCHAR (4000)  NULL,
    [Title]                    NVARCHAR (300)   NULL,
    [LeadSource]               NVARCHAR (100)   NULL,
    [LifecycleStage]           NVARCHAR (50)    NULL,
    [PartnerType]              NVARCHAR (50)    NULL,
    [DoNotEmail]               BIT              NULL,
    [HomePhone]                NVARCHAR (20)    NULL,
    [MobilePhone]              NVARCHAR (20)    NULL,
    [WorkPhone]                NVARCHAR (20)    NULL,
    [AccountID]                INT              NULL,
    [PrimaryEmail]             NVARCHAR (256)   NULL,
    [SecondaryEmails]          NVARCHAR (4000)  NULL,
    [FacebookUrl]              NVARCHAR (4000)  NULL,
    [TwitterUrl]               NVARCHAR (4000)  NULL,
    [GooglePlusUrl]            NVARCHAR (4000)  NULL,
    [LinkedInUrl]              NVARCHAR (4000)  NULL,
    [BlogUrl]                  NVARCHAR (4000)  NULL,
    [WebSiteUrl]               NVARCHAR (4000)  NULL,
    [AddressLine1]             NVARCHAR (95)    NULL,
    [AddressLine2]             NVARCHAR (95)    NULL,
    [City]                     NVARCHAR (35)    NULL,
    [State]                    NVARCHAR (65)    NULL,
    [Country]                  NVARCHAR (65)    NULL,
    [ZipCode]                  NVARCHAR (11)    NULL,
    [CustomFieldsData]         NVARCHAR (4000)  NULL,
    [ContactID]                INT              NULL,
    [ContactStatusID]          TINYINT          NULL,
    [ReferenceID]              UNIQUEIDENTIFIER NULL,
    [ContactTypeID]            TINYINT          NULL,
    [OwnerID]                  INT              NULL,
    [JobID]                    INT              NULL,
    [LeadSourceID]             SMALLINT         NULL,
    [LifecycleStageID]         SMALLINT         NULL,
    [PartnerTypeID]            SMALLINT         NULL,
    [LoopID]                   INT              NULL,
    [CompanyID]                INT              NULL,
    [PhoneData]                NVARCHAR (4000)  NULL,
    [CommunicationID]          INT              NULL,
    [EmailExists]              BIT              NULL,
    [IsBuilderNumberPass]      BIT              NULL,
    [LeadAdapterSubmittedData] NVARCHAR (4000)  NULL,
    [LeadAdapterRowData]       NVARCHAR (4000)  NULL,
    [ValidEmail]               BIT              NULL,
    [OrginalRefId]             VARCHAR (50)     NULL,
    [IsDuplicate]              BIT              NOT NULL,
    [IsCommunityNumberPass]    BIT              NULL,
    CONSTRAINT [PK_ImportContactData] PRIMARY KEY CLUSTERED ([ImportContactDataID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_missing_371]
    ON [dbo].[ImportContactData]([AccountID] ASC, [JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [IsBuilderNumberPass] ASC, [IsCommunityNumberPass] ASC)
    INCLUDE([ImportContactDataID], [OrginalRefId]);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_missing_385]
    ON [dbo].[ImportContactData]([AccountID] ASC, [JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [ContactStatusID] ASC, [IsBuilderNumberPass] ASC, [IsCommunityNumberPass] ASC)
    INCLUDE([OrginalRefId]);

