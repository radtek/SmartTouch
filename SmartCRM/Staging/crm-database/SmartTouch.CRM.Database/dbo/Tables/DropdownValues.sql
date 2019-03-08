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
CREATE NONCLUSTERED INDEX [IX_DropdownValues_missing_257]
    ON [dbo].[DropdownValues]([AccountID] ASC, [IsDeleted] ASC, [DropdownValueTypeID] ASC)
    INCLUDE([DropdownValueID], [DropdownID], [DropdownValue], [IsDefault], [SortID], [IsActive]);


GO
CREATE NONCLUSTERED INDEX [IX_DropdownValues_missing_232]
    ON [dbo].[DropdownValues]([DropdownValueTypeID] ASC)
    INCLUDE([DropdownValueID], [DropdownID], [AccountID], [DropdownValue], [IsDefault], [SortID], [IsActive], [IsDeleted]);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20171109-125747]
    ON [dbo].[DropdownValues]([DropdownID] ASC, [AccountID] ASC, [DropdownValue] ASC, [IsActive] ASC, [IsDeleted] ASC)
    INCLUDE([DropdownValueID]);


GO
CREATE STATISTICS [_dta_stat_2133582639_2_3_9_7]
    ON [dbo].[DropdownValues]([DropdownID], [AccountID], [IsDeleted], [IsActive]);

