using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface INoteViewModel
    {
        int NoteId { get; set; }
        int? AccountId { get; set; }
        string NoteDetails { get; set; }
        IList<ContactEntry> Contacts { get; set; }
        int OppurtunityId { get; set; }
        //int tagId { get; set; }
        IList<TagViewModel> TagsList { get; set; }
        short NoteCategory { get; set; }
        IEnumerable<dynamic> NoteCategories { get; set; }
        int CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
    }

    public class NoteViewModel : INoteViewModel
    {
        public int NoteId { get; set; }
        /// <summary>
        /// Note Details
        /// </summary>
        public string NoteDetails { get; set; }
        public int? AccountId { get; set; }
        //public int tagId { get; set; }
        /// <summary>
        /// Associated Contacts
        /// </summary>
        public IList<ContactEntry> Contacts { get; set; }
        public int OppurtunityId { get; set; }

        /// <summary>
        /// Associated Tags
        /// </summary>
        public IList<TagViewModel> TagsList { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool SelectAll { get; set; }
        public bool AddToContactSummary { get; set; }
        /// <summary>
        /// Note category
        /// </summary>
        public virtual short NoteCategory { get; set; }
        public IEnumerable<dynamic> NoteCategories { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
    }

    public class PartialViewModel
    {
        public NoteViewModel noteViewModel{ get; set; }
        public TourViewModel tourViewModel{ get; set; }
        public RelationshipViewModel relationshipViewModel{ get; set; }
    }
}
