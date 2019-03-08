using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ITagTypesViewModel
    {
        int TagTypeID { get; set; }
        string TagTypeName { get; set; }
    }

    public class TagTypesViewModel : ITagTypesViewModel
    {
        public int TagTypeID { get; set; }
        public string TagTypeName { get; set; }
    }
}
