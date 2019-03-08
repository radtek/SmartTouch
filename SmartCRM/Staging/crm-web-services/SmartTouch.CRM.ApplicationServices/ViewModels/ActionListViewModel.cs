using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ActionListEntry
    {
        public int ActionId { get; set; }
        public string ActionDetail { get; set; }
        public DateTime RemindOn { get; set; }
    }

    public interface IActionListviewModel
    {
        IEnumerable<ActionListEntry> Actions { get; set; }
    }
    public class ActionListViewModel : IActionListviewModel
    {
        public IEnumerable<ActionListEntry> Actions { get; set; }
    }
}
