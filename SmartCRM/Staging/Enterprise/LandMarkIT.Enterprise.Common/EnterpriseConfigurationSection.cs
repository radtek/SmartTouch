using System;
using System.Configuration;

namespace LandmarkIT.Enterprise.Common
{
    public class EnterpriseConfigurationSection : ConfigurationSection
    {
        public static EnterpriseConfigurationSection Instance = (EnterpriseConfigurationSection)ConfigurationManager.GetSection("enterpriseConfiguration");

        [ConfigurationProperty("communicationManager")]
        public CommunicationManagerElement CommunicationManager
        {
            get
            {
                return (CommunicationManagerElement)this["communicationManager"];
            }
            set
            { this["communicationManager"] = value; }
        }
    }

    public class CommunicationManagerElement : ConfigurationElement
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
