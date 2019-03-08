using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communications
{
    public class Communication : EntityBase<int>, IAggregateRoot
    {
        public string SecondaryEmails { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string BlogUrl { get; set; }
        public string WebSiteUrl { get; set; }
        public string GooglePlusUrl { get; set; }

        public string FacebookAccessToken { get; set; }
        public string TwitterOAuthToken { get; set; }
        public string TwitterOAuthTokenSecret { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
