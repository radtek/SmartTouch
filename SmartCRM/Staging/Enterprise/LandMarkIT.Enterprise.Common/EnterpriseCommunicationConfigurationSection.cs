using System;
using System.Configuration;

namespace LandmarkIT.Enterprise.Common
{
    public class EnterpriseCommunicationConfigurationSection : ConfigurationSection
    {
        public static readonly EnterpriseCommunicationConfigurationSection Instance = (EnterpriseCommunicationConfigurationSection)ConfigurationManager.GetSection("enterpriseConfiguration");

        [ConfigurationProperty("enterpriseCommunication")]
        public EnterpriseCommunicationElement CommunicationManager
        {
            get
            {
                return (EnterpriseCommunicationElement)this["enterpriseCommunication"];
            }
            set
            { this["enterpriseCommunication"] = value; }
        }
    }

    public class EnterpriseCommunicationElement : ConfigurationElement
    {
        [ConfigurationProperty("connectionStringName", IsRequired = false)]
        public String ConnectionStringName
        {
            get
            {
                return (String)this["connectionStringName"];
            }
            set
            {
                this["connectionStringName"] = value;
            }
        }

    }
}
