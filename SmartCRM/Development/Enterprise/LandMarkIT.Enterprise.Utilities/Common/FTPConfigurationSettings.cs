using System.Configuration;

namespace LandmarkIT.Enterprise.Utilities.Common
{
    public class FTPElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }
        [ConfigurationProperty("username", IsRequired = true)]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }
    }
    [ConfigurationCollection(typeof(FTPElement))]
    public class FTPElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FTPElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FTPElement)element).Name;
        }
    }
    public class FTPPathRetrieverSection : ConfigurationSection
    {
        [ConfigurationProperty("paths", IsDefaultCollection = true)]
        public FTPElementCollection FTPElements
        {
            get { return (FTPElementCollection)this["paths"]; }
            set { this["paths"] = value; }
        }
    }
}
