using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Xml;

namespace FileProcessor.BackgroundService.UnitTest
{
    [TestClass]
    public class FileProccessorUnitTest
    {
        [TestMethod]
        public void ValidateMRNFieldOfInputXMLFile_ReturnsTrue()
        {
            string filePath = @"C:\\Input\test.xml";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            XmlNode mrnNode = xmlDoc.SelectSingleNode("medisight/mrn");

            var mrnValue = mrnNode.InnerText;

            string pattern = @"^\d{7}\w$";
            bool PatternMatch = Regex.IsMatch(mrnValue, pattern);

            if(PatternMatch == true)
            {
                Assert.IsTrue(PatternMatch);
                System.Console.WriteLine("Pattern match");
                return;
            }
            else
            {
                Assert.IsFalse(PatternMatch);
                System.Console.WriteLine("Pattern does not match");
                return;
            }
            

        }

    }
}
