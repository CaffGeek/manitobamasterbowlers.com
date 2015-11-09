using System;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace ManitobaMasterBowlers_com
{
    public class Utility
    {

        #region Constructor
        public Utility()
        {
            //try
            //{
            //    dataUtility = new DataUtility(ConfigurationManager.ConnectionStrings[1].Name);
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Utility.Constructor. " + ex.Message);
            //}
        }
        #endregion

        #region Xml Methords
        /// <summary>
        /// Validates <paramref name="xmlString"/> using the XSD schema stored at <paramref name="xsdPath"/>.
        /// </summary>
        /// <param name="xmlString">The Xml string to validate.</param>
        /// <param name="xsdPath">The path of the XSD schema file.</param>
        /// <returns>True when <paramref name="xmlString"/> is valid according to xsd schema at <paramref name="xsdPath"/> or false otherwise.</returns>
        /// <exception cref="System.NullReferenceException">Thrown when <paramref name="xmlString"/> or <paramref name="xsdPath"/> is null.</exception>
        public bool Validate(string xmlString, string xsdPath)
        {
            if (string.IsNullOrEmpty(xmlString))
                throw new System.ArgumentNullException("Xml string is null or empty.");
            if (string.IsNullOrEmpty(xsdPath))
                throw new System.ArgumentNullException("Xsd path is null or empty.");

            xsdPath = System.AppDomain.CurrentDomain.BaseDirectory + xsdPath;
            System.Xml.XmlDocument document = new XmlDocument();
            document.LoadXml(xmlString);
            System.Xml.XmlDocument schema = new XmlDocument();

            schema.Load(xsdPath);
            using (System.Xml.XmlNodeReader nrDocument = new System.Xml.XmlNodeReader(document))
            {
                using (System.Xml.XmlNodeReader nrSchema = new System.Xml.XmlNodeReader(schema))
                {
                    System.Xml.XmlReaderSettings rsDocument = new System.Xml.XmlReaderSettings();
                    rsDocument.Schemas.Add(null, nrSchema);
                    rsDocument.ValidationType = System.Xml.ValidationType.Schema;
                    rsDocument.ValidationEventHandler +=
                     new System.Xml.Schema.ValidationEventHandler(ValidationEventHandler);

                    using (System.Xml.XmlReader rDocument = System.Xml.XmlReader.Create(nrDocument, rsDocument))
                    {
                        try
                        {
                            while (rDocument.Read()) ;
                        }
                        catch (System.Xml.Schema.XmlSchemaException eCurrent)
                        {
                            throw new System.Exception("Xml inputed is invalid. " + eCurrent.Message);
                        }

                        rDocument.Close();
                    }

                    nrSchema.Close();
                }

                nrDocument.Close();
            }

            return true;
        }

        /// <summary>
        /// Occurs when the reader encounters validation errors.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ValidationEventHandler(object sender,
         System.Xml.Schema.ValidationEventArgs e)
        {

            if (e.Severity == System.Xml.Schema.XmlSeverityType.Error)
                throw e.Exception;
        }
        public static string Transform(string sXml, string sXslPath)
        {
            XmlDocument doc = new XmlDocument();
            XmlWriter xmlWriter;
            StringBuilder sBuilder = new StringBuilder();
            try
            {
                doc.LoadXml(sXml);
                System.Xml.Xsl.XslCompiledTransform xsl = new XslCompiledTransform();
                xsl.Load(sXslPath);
                xmlWriter = System.Xml.XmlWriter.Create(sBuilder, xsl.OutputSettings);
                xsl.Transform(doc.CreateNavigator(), xmlWriter);
                return sBuilder.ToString();
            }
            catch (Exception eTranx)
            {
                throw new Exception("Transform." + eTranx.Message);
            }
        }
        public static string Transform(string xmlString1, string sXslPath, System.Xml.Xsl.XsltArgumentList arguments)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString1);

                System.Xml.Xsl.XslCompiledTransform xsl = new System.Xml.Xsl.XslCompiledTransform();
                xsl.Load(sXslPath);

                System.IO.StringWriter swDocument
                    = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(swDocument);
                xsl.Transform((doc.CreateNavigator()), arguments, xtw);
                xtw.Flush();
                return swDocument.ToString();
            }
            catch (Exception eTranx)
            {
                throw new Exception("Transform." + eTranx.Message);
            }
        }
        #region FormatXml
        /// <summary>
        /// Formats an XML String for human readability.
        /// </summary>
        /// <param name="xmlSring">The xml string to format.</param>
        /// <returns>The human readable xml string.</returns>
        public static string FormatXml(string xmlString)
        {
            if (xmlString == null || xmlString == string.Empty)
                throw new System.Exception("Xml string is invalid.");
            System.Xml.XmlDocument document = new XmlDocument();
            document.LoadXml(xmlString);

            using (System.IO.StringWriter swDocument = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            {
                System.Xml.XmlTextWriter twDocument = new System.Xml.XmlTextWriter(swDocument);
                try
                {
                    twDocument.Formatting = System.Xml.Formatting.Indented;
                    twDocument.Indentation = 2;
                    document.WriteTo(twDocument);
                }
                finally
                {
                    twDocument.Close();
                }

                return swDocument.ToString();
            }
        }
        #endregion
        public static XmlNode addXmlNode(XmlNode baseNode, string name, string value)
        {
            XmlNode node = null;
            XmlElement e = baseNode.OwnerDocument.CreateElement(name);
            e.InnerText = value;
            node = baseNode.AppendChild(e);
            return node;
        }
        public static XmlNode setTreeViewXmlNode(XmlNode baseNode, string name, string value)
        {
            XmlNode node = baseNode.SelectSingleNode(name + "[@name=" + value + "]");
            if (node == null)
            {
                XmlElement e = baseNode.OwnerDocument.CreateElement(name);
                node = baseNode.AppendChild(e);
            }
            setXmlAttr(node, "name", value);

            return node;
        }
        public static XmlNode setXmlNode(XmlNode baseNode, string name, string value)
        {
            XmlNode node = baseNode.SelectSingleNode(name);
            if (node == null)
            {
                XmlElement e = baseNode.OwnerDocument.CreateElement(name);
                node = baseNode.AppendChild(e);
            }
            node.InnerText = value;

            return node;
        }

        public static void setXmlAttr(XmlNode baseNode, string name, string value)
        {
            XmlAttribute attr = baseNode.Attributes[name];
            if (attr == null)
            {
                attr = baseNode.OwnerDocument.CreateAttribute(name);
                baseNode.Attributes.Append(attr);
            }
            attr.Value = value;
        }

        #endregion

    }
}
