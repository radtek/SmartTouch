﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class AVColumnPreferencesDb
    {
        [Key]
        public int AVColumnPreferenceID { get; set; }
        public int EntityID { get; set; }
        public byte EntityType { get; set; }
        public int FieldID { get; set; }
        public byte FieldType { get; set; }
        public byte ShowingType { get; set; }
    }
}