CREATE TABLE [dbo].[SpamValidators] (
    [SpamValidatorID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Validator]       VARCHAR (30)  NOT NULL,
    [Value]           VARCHAR (MAX) NULL,
    [AccountID]       INT           NULL,
    [RunValidation]   BIT           NOT NULL,
    [Order]           INT           NOT NULL,
    CONSTRAINT [PK_SpamValidators] PRIMARY KEY CLUSTERED ([SpamValidatorID] ASC) WITH (FILLFACTOR = 90)
);

