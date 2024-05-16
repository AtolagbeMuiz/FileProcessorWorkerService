using System;
using System.Collections.Generic;
using System.Text;

namespace FileProcessor.BackgroundService
{
    public class XMLObject
    {
        public string documentType { get; set; }
        public string mrn { get; set; }

        //[System.Xml.Serialization.XmlElementAttribute(DataType = "dateTime", ElementName = "encounterDate")]

        public string encounterDate { get; set; }
    }
}
