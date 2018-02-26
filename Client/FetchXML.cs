using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Client
{
    // see https://msdn.microsoft.com/en-us/library/gg328332.aspx
    /// <summary>
    /// Class to create FetchXML entity element for acting with Dynamics CRM
    /// </summary>
    class FetchXML
    {
        // The meaningful root element of a FetchXml <fetch><entity /></fetch>
        public FetchElement EntityElement { get; }

        public FetchXML(string entityName)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement fetchElement = doc.CreateElement("fetch");
            fetchElement.SetAttribute("version", "1.0");
            fetchElement.SetAttribute("mapping", "logical");
            doc.AppendChild(fetchElement);

            XmlElement entityElement = doc.CreateElement("entity");
            fetchElement.AppendChild(entityElement);
            entityElement.SetAttribute("name", entityName);
            EntityElement = new FetchElement(entityElement);
        }

        /// <summary>
        /// Create a free FetchElement
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public FetchElement CreateElement(string element)
        {
            XmlElement newElement = EntityElement.DOC.CreateElement(element);
            return new FetchElement(newElement);
        }

        /// <summary>
        /// Return ?fetchXml=the content of xml created for query
        /// </summary>
        /// <returns></returns>
        public string ToQueryString()
        {
            return "?fetchXml=" + EntityElement.DOC.OuterXml;
        }
    }

    /// <summary>
    /// Class to represent and manipulate elements in FetchXml
    /// </summary>
    class FetchElement
    {
        // use to verify alias when in development
        static Regex rgx = new Regex("^[A-Za-z_][a-zA-Z0-9_]{0,}$");

        XmlElement Current;
        public XmlDocument DOC { get; }

        public FetchElement(XmlElement element)
        {
            Current = element;
            DOC = Current.OwnerDocument;
        }

        /// <summary>
        /// Create a new child element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public FetchElement AddElement(string element)
        {
            XmlElement newElement = DOC.CreateElement(element);
            Current.AppendChild(newElement);
            return new FetchElement(newElement);
        }

        public void SetAttribute(string name, string value)
        {
            Current.SetAttribute(name, value);
        }

        #region shortcuts
        /// <summary>
        /// Create an attribute element for returning field value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="optionals">e.g. alias, aggregate</param>
        /// <returns></returns>
        public FetchElement AddField(string name, Dictionary<string, string> optionals = null)
        {
            FetchElement field = AddElement("attribute");
            field.SetAttribute("name", name);
            if (optionals != null)
            {
                foreach (KeyValuePair<string, string> item in optionals)
                {
                    // alias: Only characters within the ranges [A-Z], [a-z] or [0-9] or _ are allowed.
                    // The first character may only be in the ranges [A-Z], [a-z] or _.
                    if (item.Key == "alias")
                        Debug.Assert(rgx.Match(item.Value).Success);
                    field.SetAttribute(item.Key, item.Value);
                }
            }
            return this;
        }

        /// <summary>
        /// Create an attribute element for returning field value as alias
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        public FetchElement AddField(string name, string alias)
        {
            Dictionary<string, string> option = new Dictionary<string, string>
            {
                ["alias"] = alias
            };
            return AddField(name, option);
        }

        public FetchElement AddFilter(string type = "and")
        {
            FetchElement filterElement = AddElement("filter");
            filterElement.SetAttribute("type", type);
            return filterElement;
        }

        /// <summary>
        /// Add a condition element to Current element - a filter element
        /// </summary>
        /// <param name="target">Field on which a filter applies to </param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns>Current element for chaining</returns>
        public FetchElement AddCondition(string target, string op, string value = null)
        {
            FetchElement condElement = AddElement("condition");
            condElement.SetAttribute("attribute", target);
            condElement.SetAttribute("operator", op);
            if (!string.IsNullOrEmpty(value))
                condElement.SetAttribute("value", value);
            return this;
        }

        /// <summary>
        /// Add a link entity to current element
        /// </summary>
        /// <param name="toEntity">Another entity this entity links to </param>
        /// <param name="sourceAttribute"></param>
        /// <param name="targetAttribute"></param>
        /// <param name="optionals">can be anything from the list: link-type (inner, outer), alias, intersect, visible etc</param>
        /// <returns></returns>
        public FetchElement AddLinkEntity(string toEntity, string sourceAttribute, string targetAttribute, Dictionary<string, string> optionals = null)
        {
            FetchElement linkingElement = AddElement("link-entity");
            linkingElement.SetAttribute("name", toEntity);
            linkingElement.SetAttribute("from", sourceAttribute);
            linkingElement.SetAttribute("to", targetAttribute);
            if (optionals != null)
            {
                foreach (KeyValuePair<string, string> item in optionals)
                {
                    linkingElement.SetAttribute(item.Key, item.Value);
                }
            }
            return linkingElement;
        }
        #endregion
    }
}
