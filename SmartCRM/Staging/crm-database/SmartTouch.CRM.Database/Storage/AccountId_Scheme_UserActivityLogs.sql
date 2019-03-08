CREATE PARTITION SCHEME [AccountId_Scheme_UserActivityLogs]
    AS PARTITION [AccountIdList]
    TO ([Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit], [Audit]);

