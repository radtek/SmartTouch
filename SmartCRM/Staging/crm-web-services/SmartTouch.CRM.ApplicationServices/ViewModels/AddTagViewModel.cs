using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IAddTagViewModel
    {
        int AddTagID { get; set; }
        IEnumerable<ContactEntry> Contacts { get; set; }
        IEnumerable<TagViewModel> TagsList { get; set; }
        IEnumerable<TagViewModel> PopularTags { get; set; }
        IEnumerable<TagViewModel> RecentTags { get; set; }
        IEnumerable<OpportunitiesList> Opportunities { get; set; }
    }

    public class AddTagViewModel : IAddTagViewModel
    {
        public int AddTagID { get; set; }
        public IEnumerable<ContactEntry> Contacts { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
        public IEnumerable<OpportunitiesList> Opportunities { get; set; }
        public bool SelectAll { get; set; }
    }

    //public class AddTagViewModel
    //{
    //    public int TagID { get; set; }
    //    public string TagName { get; set; }
    //    public string Description { get; set; }
    //    public int Count { get; set; }
    //    public int ContactId { get; set; }
    //    public int AccountId { get; set; }
    //    public bool IsChecked { get; set; }
    //}
}
