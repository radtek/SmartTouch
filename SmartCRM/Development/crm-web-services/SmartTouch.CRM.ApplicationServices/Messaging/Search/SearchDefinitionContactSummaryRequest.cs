using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class CampaignRecipientsSummaryRequest : ServiceRequestBase
    {
        public TagViewModel ContactTag { get; set; }
        public AdvancedSearchViewModel SearchDefinition { get; set; }
        public bool IncludeSearchResults { get; set; }
        public IEnumerable<AdvancedSearchViewModel> AllSearchDefinitions { get; set; }
        public IEnumerable<TagViewModel> AllContactTags { get; set; }
        public long ToTagStatus { get; set; }
    }



    public class CampaignRecipientsSummaryResponse : ServiceResponseBase
    {
        public long CountBySearchDefinition{ get; set; }
        public long CountsByAllSearchDefinitions { get; set; }
        public IEnumerable<int> AllUniqueContactIds { get; set; }
    }


}
