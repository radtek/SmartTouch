
namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    public enum MailProvider : byte
    {
        Undefined = 0,
        Smtp = 1,
        SendGrid = 2,
        MailChimp = 3,
        SmartTouch = 4,
        CustomSmartTouch = 5,
        CustomMailChimp = 6
    }
}
