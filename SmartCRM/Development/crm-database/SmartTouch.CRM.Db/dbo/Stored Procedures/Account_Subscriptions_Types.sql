
CREATE PROCEDURE  [dbo].[Account_Subscriptions_Types] 
	-- Add the parameters for the stored procedure here

AS
BEGIN
	select 0 as SubscriptionID,'Accounts' as SubscriptionName , 0 'isSubscription'
	union
	select subscriptionid as SubscriptionID , subscriptionname as SubscriptionName , 1 from DBO.subscriptions (nolock) where subscriptionid IN (2,3)
END


/*

   EXEC Account_Subscriptions_Types
  
*/





GO


