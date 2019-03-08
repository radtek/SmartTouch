CREATE TABLE [dbo].[Accounts] (
    [AccountID]             INT            IDENTITY (1, 1) NOT NULL,
    [AccountName]           NVARCHAR (75)  NOT NULL,
    [FirstName]             NVARCHAR (75)  NOT NULL,
    [LastName]              NVARCHAR (75)  NOT NULL,
    [Company]               NVARCHAR (75)  NULL,
    [DomainURL]             VARCHAR (100)  NULL,
    [PrimaryEmail]          VARCHAR (256)  NOT NULL,
    [HomePhone]             VARCHAR (20)   NULL,
    [WorkPhone]             VARCHAR (20)   NULL,
    [MobilePhone]           VARCHAR (50)   NULL,
    [PrivacyPolicy]         VARCHAR (MAX)  NULL,
    [CommunicationID]       INT            NULL,
    [SubscriptionID]        TINYINT        NOT NULL,
    [DateFormatID]          TINYINT        NOT NULL,
    [CurrencyID]            TINYINT        NOT NULL,
    [CountryID]             NCHAR (2)      NOT NULL,
    [TimeZone]              VARCHAR (32)   NOT NULL,
    [Status]                TINYINT        NOT NULL,
    [IsDeleted]             BIT            CONSTRAINT [DF_Accounts_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedBy]             INT            NOT NULL,
    [CreatedOn]             DATETIME       NOT NULL,
    [ModifiedBy]            INT            NULL,
    [ModifiedOn]            DATETIME       NULL,
    [GoogleDriveClientID]   NVARCHAR (200) NULL,
    [GoogleDriveAPIKey]     NVARCHAR (200) NULL,
    [OpportunityCustomers]  TINYINT        NULL,
    [DropboxAppKey]         NVARCHAR (200) NULL,
    [LogoImageID]           INT            NULL,
    [FacebookAPPID]         VARCHAR (MAX)  NULL,
    [FacebookAPPSecret]     VARCHAR (MAX)  NULL,
    [TwitterAPIKey]         VARCHAR (MAX)  NULL,
    [TwitterAPISecret]      VARCHAR (MAX)  NULL,
    [StatusMessage]         NVARCHAR (MAX) NULL,
    [SenderReputationCount] FLOAT (53)     NULL,
    [HelpURL]               VARCHAR (500)  NOT NULL,
    [ShowTC]                BIT            NOT NULL,
    [TC]                    NVARCHAR (MAX) NULL,
    [Disclaimer]            BIT            NULL,
    [ContactsCount]         INT            CONSTRAINT [DF_Accounts_ContactsCount] DEFAULT ((0)) NOT NULL,
    [EmailsCount]           INT            CONSTRAINT [DF_Accounts_EmailsCount] DEFAULT ((0)) NOT NULL,
    [LitmusAPIKey]          VARCHAR (50)   NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([AccountID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Accounts_Communications] FOREIGN KEY ([CommunicationID]) REFERENCES [dbo].[Communications] ([CommunicationID]),
    CONSTRAINT [FK_Accounts_Images] FOREIGN KEY ([LogoImageID]) REFERENCES [dbo].[Images] ([ImageID]),
    CONSTRAINT [FK_Accounts_Subscriptions] FOREIGN KEY ([SubscriptionID]) REFERENCES [dbo].[Subscriptions] ([SubscriptionID])
);


GO

CREATE TRIGGER [dbo].[tr_Accounts_Delete] ON [dbo].[Accounts] FOR DELETE AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate])
	 SELECT AccountID, ModifiedBy, 1, 4, GETUTCDATE() FROM deleted
GO

CREATE TRIGGER [dbo].[tr_Accounts_Insert] ON [dbo].[Accounts] FOR 
INSERT AS 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate], AccountID, EntityName)
	 SELECT AccountID, CreatedBy, 1, 1, GETUTCDATE(), AccountID, AccountName FROM Inserted
GO


CREATE  TRIGGER [dbo].[tr_Accounts_Update] ON [dbo].[Accounts] AFTER UPDATE
AS
  BEGIN
     

      IF ( update(ContactsCount) or UPDATE  (EmailsCount))

	  BEGIN  
	    SELECT 1
	  END 

	  ELSE  
	   BEGIN 
 INSERT INTO dbo.UserActivityLogs([EntityID],[UserID],[ModuleID],[UserActivityID],[LogDate],AccountID, EntityName)
	 SELECT AccountID, ModifiedBy, 1, CASE WHEN IsDeleted = 0 THEN 3 
 										   WHEN IsDeleted = 1 THEN 4 END, GETUTCDATE(),AccountID, AccountName FROM Inserted
		END  
		END 