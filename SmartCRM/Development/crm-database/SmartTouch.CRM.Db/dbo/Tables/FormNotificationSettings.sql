
CREATE TABLE [dbo].[FormNotificationSettings](
	[FormNotificationSettingID] [int] IDENTITY(1,1) NOT NULL,
	[FormID] [int] NOT NULL,
CONSTRAINT [PK_FormNotificationSettings] PRIMARY KEY CLUSTERED ([FormNotificationSettingID] ASC) WITH (FILLFACTOR = 90) ON [Primary],
 CONSTRAINT [FK_FormNotificationSettings_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID]));
GO





