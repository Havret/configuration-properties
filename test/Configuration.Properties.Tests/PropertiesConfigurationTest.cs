using System.IO;
using System.Text;
using NUnit.Framework;

namespace Configuration.Properties.Tests
{
    [TestFixture]
    public class PropertiesConfigurationTest
    {
        [Test]
        public void LoadKeyValuePairsFromValidPropertiesFile()
        {
            var properties = @"
            DefaultConnection.ConnectionString=TestConnectionString
            DefaultConnection.Provider=SqlClient
            Data.Inventory.ConnectionString=AnotherTestConnectionString
            Data.Inventory.Provider=MySql
            ";

            var propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual("TestConnectionString", GetValue(propertiesConfigSrc, "DefaultConnection:ConnectionString"));
            Assert.AreEqual("SqlClient", GetValue(propertiesConfigSrc, "DEFAULTCONNECTION:PROVIDER"));
            Assert.AreEqual("AnotherTestConnectionString", GetValue(propertiesConfigSrc, "Data:Inventory:CONNECTIONSTRING"));
            Assert.AreEqual("MySql", GetValue(propertiesConfigSrc, "Data:Inventory:Provider"));
        }

        [Test]
        public void LoadMethodCanHandleEmptyValue()
        {
            var properties = @"DefaultKey=";
            PropertiesConfigurationProvider propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual(string.Empty, GetValue(propertiesConfigSrc, "DefaultKey"));
        }

        [Test]
        public void LoadKeyValuePairsFromValidPropertiesFileWithQuotedValues()
        {
            var properties = "DefaultConnection.ConnectionString=\"TestConnectionString\"\n" +
                             "DefaultConnection.Provider=\"SqlClient\"\n" +
                             "Data.Inventory.ConnectionString=\"AnotherTestConnectionString\"\n" +
                             "Provider=\"MySql\"";

            var propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual("TestConnectionString", GetValue(propertiesConfigSrc, "DefaultConnection:ConnectionString"));
            Assert.AreEqual("SqlClient", GetValue(propertiesConfigSrc, "DefaultConnection:Provider"));
            Assert.AreEqual("AnotherTestConnectionString", GetValue(propertiesConfigSrc, "Data:Inventory:ConnectionString"));
            Assert.AreEqual("MySql", GetValue(propertiesConfigSrc, "Provider"));
        }

        [Test]
        public void DoubleQuoteIsPartOfValueIfNotPaired()
        {
            var properties = "DefaultConnection=\"TestConnectionString\n" +
                             "Provider=SqlClient\"";

            var propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual("\"TestConnectionString", GetValue(propertiesConfigSrc, "DefaultConnection"));
            Assert.AreEqual("SqlClient\"", GetValue(propertiesConfigSrc, "Provider"));
        }

        [Test]
        public void DoubleQuoteIsPartOfValueIfAppearInTheMiddleOfValue()
        {
            var properties = "DefaultConnection=Test\"Connection\"String\n" +
                             "Provider=Sql\"Client";

            var propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual("Test\"Connection\"String", GetValue(propertiesConfigSrc, "DefaultConnection"));
            Assert.AreEqual("Sql\"Client", GetValue(propertiesConfigSrc, "Provider"));
        }

        [Test]
        public void SupportAndIgnoreComments()
        {
            var properties = @"
            # Comments
            ConnectionString=TestConnectionString
            Provider=SqlClient
            ";

            var propertiesConfigSrc = new PropertiesConfigurationProvider(new PropertiesConfigurationSource());

            propertiesConfigSrc.Load(StringToStream(properties));

            Assert.AreEqual("TestConnectionString", GetValue(propertiesConfigSrc, "ConnectionString"));
            Assert.AreEqual("SqlClient", GetValue(propertiesConfigSrc, "Provider"));
        }

        private static Stream StringToStream(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(bytes);
        }

        private string GetValue(PropertiesConfigurationProvider provider, string key)
        {
            Assert.IsTrue(provider.TryGet(key, out var value));
            return value;
        }
    }
}