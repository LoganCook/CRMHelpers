using System.Collections.Generic;
using System.Xml;
using Xunit;
using Client;

namespace tests
{
    public class FetchXmlTest
    {
        [Fact]
        public void CanProduceXml()
        {
            const string result = @"<fetch version='1.0' mapping='logical'><entity name='test'><attribute name='name' /><attribute name='description' /></entity></fetch>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            var xml = new FetchXML("test");
            FetchElement entity = xml.EntityElement;
            entity.AddField("name")
                .AddField("description");

            Assert.Equal("?fetchXml=" + doc.OuterXml, xml.ToQueryString());
        }

        [Fact]
        public void CanCreateWithAttributes()
        {
            var attributes = new Dictionary<string, string>
            {
                { "name", "salesorderdetail" },
                { "from", "salesorderid" },
                { "to", "salesorderid" }
            };
            FetchElement element = new FetchElement("link-entity", attributes);
            Assert.Equal(@"<link-entity name=""salesorderdetail"" from=""salesorderid"" to=""salesorderid"" />", element.DOC.OuterXml);
        }

        [Fact]
        public void CanCreateFragment()
        {
            FetchElement frgment = new FetchElement("link-entity", "test");
            Assert.Equal(@"<link-entity name=""test"" />", frgment.DOC.OuterXml);
        }

        [Fact]
        public void CanAddFragment()
        {
            const string result = @"<fetch version='1.0' mapping='logical'><entity name='test'><link-entity name='test' /></entity></fetch>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            var xml = new FetchXML("test");
            FetchElement frgment = new FetchElement("link-entity", "test");
            xml.EntityElement.AddFragment(frgment);
            Assert.Equal("?fetchXml=" + doc.OuterXml, xml.ToQueryString());
        }
    }
}
