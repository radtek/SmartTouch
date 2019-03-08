using LandmarkIT.Enterprise.Utilities.Common;
using System.Xml.Linq;

namespace LandmarkIT.Enterprise.Utilities.Excel
{
    public class ReadXML
    {
        private string _path { get; set; }
        public ReadXML(string path)
        {
            _path = path;
        }
        public Node Read()
        {
            XElement rootElement = XElement.Load(_path);
            return new Node(rootElement);
        }
    }
}
