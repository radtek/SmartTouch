using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// TagViewModel properties, interface is for unit testing using mock objects.
    /// </summary>
    public interface ITagViewModel
    {
        int TagID { get; set; }
        string TagName { get; set; }
        string Description { get; set; }
        int Count { get; set; }
        int ContactId { get;set; }
        int sourceTagID { get; set; }
        string sourceTagName { get; set; }
        int AccountID { get; set; }
        int CreatedBy { get; set; }
        bool IsSelected { get; set; }
        int OpportunityID { get; set; }
        IEnumerable<TagViewModel> Tags { get; set; }
    }

    public class TagViewModel : ITagViewModel
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public int ContactId { get; set; }
        public int sourceTagID { get; set; }
        public string sourceTagName { get; set; }
        public int AccountID { get; set; }
        public int CreatedBy { get; set; }
        public bool IsSelected { get; set; }
        public int OpportunityID { get; set; }
        public IEnumerable<TagViewModel> Tags { get; set; }
        public bool LeadScoreTag { get; set; }
    }

    public interface ITagsListViewModel
    {
        IEnumerable<TagViewModel> Tags { get; set; }
    }

    public class TagsListViewModel : ITagsListViewModel
    {
        public IEnumerable<TagViewModel> Tags { get; set; }
    }
}
