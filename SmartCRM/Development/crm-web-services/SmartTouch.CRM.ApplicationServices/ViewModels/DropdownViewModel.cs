using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Dropdowns;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
      
    public interface IDropdownsViewModel
    {
        byte DropdownID { get; set; }
        string Dropdownname { get; set; }
        IEnumerable<DropdownValueViewModel> DropdownValuesList { get; set; }
        int AccountID { get; set; }
    }

    public class DropdownViewModel : IDropdownsViewModel
    {
        public DropdownViewModel() 
        { 
            
        }
        public byte DropdownID { get; set; }
        public string Dropdownname { get; set; }
        public IEnumerable<DropdownValueViewModel> DropdownValuesList { get; set; }
        public string dropdownValuesToString { get; set; }
        public int AccountID { get; set; }
       
    }
}
