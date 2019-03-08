using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaign
{
    public interface ICampaignRepository : IRepository<Campaign, int>
    {
        IEnumerable<Campaign> SearchCampaign(string name);
        IEnumerable<Campaign> FindByContact(int contactId);
        IEnumerable<Campaign> FindByCampaignDate(DateTime campaignDate);
        IEnumerable<Campaign> FindByEmail(string email);

        Campaign GetCampaignByID(int campaignId);
        int GetContactCampaignMapId(int campaignId, int contactId);
        int CampaignContactsCount(int campaignId);
        void DeleteCampaign(int campaignId);
        
    }
}
