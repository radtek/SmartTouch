CREATE TABLE [dbo].[FormNotificationSettings] (
    [FormNotificationSettingID] INT IDENTITY (1, 1) NOT NULL,
    [FormID]                    INT NOT NULL,
    CONSTRAINT [PK_FormNotificationSettings] PRIMARY KEY CLUSTERED ([FormNotificationSettingID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_FormNotificationSettings_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID])
);

