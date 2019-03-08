CREATE TABLE [dbo].[DropdownValues] (
    [DropdownValueID]     SMALLINT     IDENTITY (1, 1) NOT NULL,
    [DropdownID]          TINYINT      NOT NULL,
    [AccountID]           INT          NULL,
    [DropdownValue]       VARCHAR (50) NOT NULL,
    [IsDefault]           BIT          NOT NULL,
    [SortID]              SMALLINT     NULL,
    [IsActive]            BIT          NOT NULL,
    [DropdownValueTypeID] SMALLINT     NOT NULL,
    [IsDeleted]           BIT          CONSTRAINT [DF_DropdownValues_IsDeleted] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DropdownValues] PRIMARY KEY CLUSTERED ([DropdownValueID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_DropdownValues_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_DropdownValues_Dropdowns] FOREIGN KEY ([DropdownID]) REFERENCES [dbo].[Dropdowns] ([DropdownID]),
    CONSTRAINT [FK_DropdownValues_DropdownValueTypes] FOREIGN KEY ([DropdownValueTypeID]) REFERENCES [dbo].[DropdownValueTypes] ([DropdownValueTypeID])
);


GO
CREATE NONCLUSTERED INDEX [missing_index_90]
    ON [dbo].[DropdownValues]([DropdownID] ASC, [AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([DropdownValueID], [DropdownValue], [IsDefault], [SortID], [IsActive], [DropdownValueTypeID]) WITH (FILLFACTOR = 90);
GO

CREATE NONCLUSTERED INDEX [IX_DropdownValues_missing_257] ON [dbo].[DropdownValues]
(
	[AccountID] ASC,
	[IsDeleted] ASC,
	[DropdownValueTypeID] ASC
)
INCLUDE ( 	[DropdownValueID],
	[DropdownID],
	[DropdownValue],
	[IsDefault],
	[SortID],
	[IsActive]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


CREATE NONCLUSTERED INDEX [IX_DropdownValues_missing_251] ON [dbo].[DropdownValues]
(
	[IsDeleted] ASC,
	[DropdownValueTypeID] ASC
)
INCLUDE ( 	[DropdownValueID],
	[DropdownID],
	[AccountID],
	[DropdownValue],
	[IsDefault],
	[SortID],
	[IsActive]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_DropdownValues_missing_232] ON [dbo].[DropdownValues]
(
	[DropdownValueTypeID] ASC
)
INCLUDE ( 	[DropdownValueID],
	[DropdownID],
	[AccountID],
	[DropdownValue],
	[IsDefault],
	[SortID],
	[IsActive],
	[IsDeleted]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [NonClusteredIndex-Dorpdownvalueidtype] ON [dbo].[DropdownValues]
(
	[DropdownValueTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
