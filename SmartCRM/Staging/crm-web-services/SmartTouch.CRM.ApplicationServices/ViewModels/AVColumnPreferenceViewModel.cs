using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class AVColumnPreferenceViewModel
    {
        public int AVColumnPreferenceID { get; set; }
        public int EntityID { get; set; }
        public byte EntityType { get; set; }
        public int FieldID { get; set; }
        public byte FieldType { get; set; }
        public byte ShowingType { get; set; }

        public IEnumerable<int> Fields { get; set; }
    }
}
