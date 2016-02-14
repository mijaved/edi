﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//XML
using System.Xml;
using System.Xml.Serialization;

namespace AIMS_BD_IATI.Library.Parser.ParserIATIv2
{
    public class ParserIATIv2 : IParserIATI
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParserIATIv2() 
        {
 
        }

        /// <summary>
        /// Implements ParseXML
        /// </summary>
        /// <returns></returns>
        public IXmlResult ParseIATIXML(string url)
        {
            IXmlResult xmlResult;

            var serializer = new XmlSerializer(typeof(XmlResultv2), new XmlRootAttribute("result"));

            // Create an XmlNamespaceManager to resolve namespaces.
            NameTable nameTable = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nameTable);
            nsmgr.AddNamespace("iati-extra", "");

            // Create an XmlParserContext.  The XmlParserContext contains all the information
            // required to parse the XML fragment, including the entity information and the
            // XmlNamespaceManager to use for namespace resolution.
            XmlParserContext xmlParserContext = new XmlParserContext(nameTable, nsmgr, null, XmlSpace.None);

            // Create the reader.
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.NameTable = nameTable;

            using (var Reader = XmlReader.Create(url, xmlReaderSettings, xmlParserContext))
            {
                xmlResult = (XmlResultv2)serializer.Deserialize(Reader);
            }

            return xmlResult;
        }
    }
}
