CREATE TABLE [dbo].[LeadScoreConditionValues] (
    [LeadScoreConditionValueID] INT            IDENTITY (1, 1) NOT NULL,
    [LeadScoreRuleID]           INT            NOT NULL,
    [ValueType]                 SMALLINT       NOT NULL,
    [Value]                     VARCHAR (1000) NULL,
    CONSTRAINT [PK_LeadScoreConditionValues] PRIMARY KEY CLUSTERED ([LeadScoreConditionValueID] ASC) WITH (FILLFACTOR = 90)
);

