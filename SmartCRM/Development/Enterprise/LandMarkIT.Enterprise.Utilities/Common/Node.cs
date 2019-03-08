using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System;
using System.Collections;

namespace LandmarkIT.Enterprise.Utilities.Common
{
    public class Node
    {
        public Node(XElement Element)
        {
            this.Name = Element.Name.LocalName;
            this._current = Element;
            if (!Element.HasAttributes && !Element.HasElements)
            {
                this.Value = Element.Value;
            }

            this._childNodes.AddRange(Element.Elements()
                                        .Select(e => new Node(e)));
            this._attributes.AddRange(
                        Element.Attributes()
                            .Select(a => new KeyValuePair<string, string>(a.Name.LocalName, a.Value)));
        }
        
        public string Name { get; private set; }
        public string Value { get; private set; }
        private List<KeyValuePair<string, string>> _attributes = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        private List<Node> _childNodes = new List<Node>();
        public List<Node> ChildNodes
        {
            get
            {
                return _childNodes;
            }

        }
        public IEnumerable<Node> this[string node]
        {
            get
            {
                return ChildNodes.Where(c => c.Name.ToLower() == node.ToLower());
            }
        }
        private XElement _current { get; set; }
        public XElement Current
        {
            get
            {
                return _current;
            }
        }
        public NodeAttributes GetAllAttributesAndElements()
        {
            NodeAttributes attributes = new NodeAttributes();
            Action<Node> getAttributes = null;

            getAttributes = (n) =>
            {
                if (n.Attributes.Any()) n.Attributes.ForEach(f => attributes.Add(new NodeAttribute(n.Name, f.Key, f.Value)));
                else if (!n.ChildNodes.Any()) attributes.Add(new NodeAttribute(n.Name, n.Name, n.Value));
                foreach (var cNode in n.ChildNodes) getAttributes(cNode);
            };

            getAttributes(this);
            return attributes;
        }

    }

    public class NodeAttribute
    {
        private string _nodeName = string.Empty;
        private string _name = string.Empty;
        private string _value = string.Empty;
        public NodeAttribute()
        {

        }
        public NodeAttribute(string nodeName, string name, string value)
        {
            _nodeName = nodeName;
            _name = name;
            _value = value;
        }
        public string Name { get { return _name; } }
        public string Value { get { return _value; } }
        public string NodeName { get { return _nodeName; } }
    }

    public class NodeAttributes : List<NodeAttribute>
    {
        public NodeAttribute this[string name]
        {
            get
            {
                var attribute= this.Where(n => n.Name.ToLower() == name.ToLower()).FirstOrDefault();
                return attribute ?? new NodeAttribute();
            }
        }
    }
}
