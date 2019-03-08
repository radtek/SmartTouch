using System;

namespace LandmarkIT.Enterprise.CommunicationManager.Responses
{
    public class RegistrationResponse
    {
        public Guid Token { get; set; }
        public virtual Exception ExceptionMessage { get; set; }
    }
}
