using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Collections;

namespace ManitobaMasterBowlers_com
{
    public abstract class Upload
    {
        #region Private Properties

        private XElement uploadXML { get; set; }

        private XElement fileinfo { get; set; }

        private XElement errors { get; set; }

        private bool hasErrors
        {
            get
            {
                return this.errors != null &&
                        (from error in this.errors.Elements("Error")
                         where !string.IsNullOrEmpty(error.Value)
                         select error
                        ).Count() > 0;
            }
        }

        private string uploadName
        {
            get
            {
                // Return the UploadNameAttribute
                foreach (object attribute in this.GetType().GetCustomAttributes(true))
                {
                    if (attribute is UploadNameAttribute)
                    {
                        return attribute.ToString();
                    }
                }

                // Else
                return this.GetType().Name;
            }
        }

        private string filepath
        {
            get
            {
                return fileinfo.Element("File").Element("Name").Value;
            }
        }

        private byte[] filebytes
        {
            get
            {
                return Convert.FromBase64String(fileinfo.Element("File").Element("Data").Value);
            }
        }

        #endregion

        #region Protected Properties

        protected string Filename
        {
            get
            {
                return Path.GetFileName(this.filepath);
            }
        }

        protected string CachedFileName
        {
            get
            {
                return string.Format("{0}.{1}",
                    DateTime.Now.ToString("yyyyMMdd.hhmmss"),
                    this.Filename
                );
            }
        }

        protected int TournamentId
        {
            get
            {
                int i = 0;
                int.TryParse(fileinfo.Element("File").Element("TournamentId").Value, out i);
                return i;
            }
        }

        #endregion

        #region Public Functions (Process)

        /// <summary>
        /// Sends the file to Sharepoint.
        /// Attempts to validate the file, and if successful, it is sent to the AS400.
        /// </summary>
        /// <returns>
        /// XElement containing errors encountered during processing
        /// </returns>
        public string Process(XElement fileinfo)
        {
            // Ensure a clean slate
            this.uploadXML = null;
            this.fileinfo = fileinfo;

            // Transform the Upload into a workable XML format
            try
            {
                this.uploadXML = this.BuildUploadXML();

                // All files are first sent to cache
                this.SaveToCache();

                if (this.uploadXML.Element("Errors") != null && this.uploadXML.Element("Errors").Elements("Error").Count() > 0)
                {
                    this.LogError(this.uploadXML.Element("Errors"));
                    this.uploadXML.Element("Errors").Remove();
                }
            }
            catch (Exception x)
            {
                this.LogError("Excel Format appears invalid, ensure you are using the latest template.", x);
            }

            // If the file passed format validation, loop again, validating data
            if (!this.hasErrors)
            {
                foreach (XElement item in this.uploadXML.Elements().Where(e => e.Name != "Errors"))
                {
                    try
                    {
                        // Validate
                        XElement validationErrors = this.Validate(item);

                        // Adjust File Processing State
                        if (validationErrors == null)
                        {
                            item.SetAttributeValue("status", "success");
                        }
                        else
                        {
                            this.LogError(validationErrors);
                            item.SetAttributeValue("status", "failure");
                        }
                    }
                    catch (Exception x)
                    {
                        item.SetAttributeValue("status", "failure");
                        x.Data.Add("Item XML", item.ToString());
                        this.LogError("Could not Validate item", x);
                    }
                }
            }

            // If the file COMPLETELY passed validation, and made it to the control tables, commit it as a whole
            if (!this.hasErrors)
            {
                try
                {
                    this.Commit(this.uploadXML);
                }
                catch (Exception x)
                {
                    x.Data.Add("Item XML", this.uploadXML.ToString());
                    this.LogError("Could not save results.", x);
                }
            }

            // Set Uplaod Status and save File History
            this.uploadXML.SetAttributeValue("status", this.hasErrors ? "failure" : "success");

            // Store errors with History for debug purposes
            if (this.errors != null)
            {
                this.uploadXML.AddFirst(this.errors);
            }

            // Throw the first error as an exception if one exists
            if (this.errors != null)
            {
                if ((from err in this.errors.Elements("Error").Where(e => !string.IsNullOrEmpty(e.Value)) select err).Count() > 0)
                {
                    return this.errors.ToString();
                }
            }

            // Success!
            return string.Empty;
        }

        #endregion

        #region Protected Abstract Functions (BuildUploadXML, Validate, Commit)

        /// <summary>
        ///		Using this.GetWorksheetXML to retrieve and convert worksheets from the excel file and convert
        ///		them into XML, this method will unify the worksheets into a single xml document, which 
        ///		is returned as an XElement.
        ///		Any errors are included in Errors/Error nodes directly below the root node.
        /// </summary>
        /// <returns>
        ///		An XElement representation of the uploaded Excel file.
        ///		Any errors are included in Errors/Error nodes directly below the root node.
        /// </returns>
        protected abstract XElement BuildUploadXML();

        /// <summary>
        ///		Performs validation on the item
        /// </summary>
        /// <returns>
        ///		An XElement of any encountered errors (Errors/Error nodes), or null if successful.
        /// </returns>
        protected abstract XElement Validate(XElement item);

        /// <summary>
        ///		Calls a stored procedure that writes the item to the local tables (AS400 copies)
        /// </summary>
        protected abstract void Commit(XElement item);

        #endregion

        #region Protected Functions (GetWorksheetXML, ValidateXSD)

        protected XElement GetWorksheetXML(string sheetName, string xsltPath)
        {
            // Create working file
            string workingFilePath = Path.GetTempFileName();
            File.WriteAllBytes(workingFilePath, this.filebytes);

            // Read the worksheet
            DataSet myDataSet = new DataSet();
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + workingFilePath + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";

            // Fill the Dataset
            OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "]", strConn);
            myCommand.Fill(myDataSet);

            // Clean up
            File.Delete(workingFilePath);

            // Return transformed XML
            return XElement.Parse(Utility.Transform(myDataSet.GetXml(), xsltPath, null));
        }

        protected XElement GetWorksheetXML(int sheetNumber, string xsltPath)
        {
            // Create working file
            string workingFilePath = Path.GetTempFileName();
            File.WriteAllBytes(workingFilePath, this.filebytes);

            // Read the worksheet
            DataSet myDataSet = new DataSet();
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + workingFilePath + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";

            // Fill the Dataset
            OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [" + GetExcelSheetNames(strConn)[sheetNumber] + "]", strConn);
            myCommand.Fill(myDataSet);

            // Clean up
            File.Delete(workingFilePath);

            // Return transformed XML
            return XElement.Parse(Utility.Transform(myDataSet.GetXml(), xsltPath, null));
        }

        protected XElement ValidateXSD(XElement item, string xsdPath)
        {
            XElement errors = new XElement("Errors");

            if (File.Exists(xsdPath))
            {
                // Reference: http://msdn.microsoft.com/en-us/library/bb358456.aspx
                XDocument xsd = XDocument.Load(xsdPath);
                XDocument xml = XDocument.Parse(item.ToString());
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", XmlReader.Create(new StringReader(xsd.ToString())));
                xml.Document.Validate(schemas,
                    // Validation Event/Error Handling
                    (sender, e) =>
                    {
                        string message = e.Message;

                        message = message.Replace(
                            "element is invalid - The value '' is invalid according to its datatype 'requiredString' - The actual length is less than the MinLength value.",
                            "cannot be blank.");

                        errors.Add(new XElement("Error", message));
                    }
                );
            }

            // If there were errors return them, otherwise return null
            return errors.Elements().Count() > 0 ? errors : null;
        }

        #endregion

        #region Private Functions (LogError, EmailError, SaveToCache)

        private XElement LogError(string error, Exception x)
        {
            // Log and Email Exceptions
            if (x != null)
            {
                this.EmailError(error, x);
            }

            if (this.errors == null)
            {
                this.errors = new XElement("Errors");
            }

            // Log Friendly Error
            this.errors.Add(new XElement("Error", error));

            // Return error object
            return this.errors;
        }

        private XElement LogError(XElement errors)
        {
            // Log Friendly Error
            if (errors != null)
            {
                if (this.errors == null)
                {
                    this.errors = new XElement("Errors");
                }

                this.errors.Add(errors.Elements("Error"));
            }

            // Return error object
            return this.errors;
        }

        private void EmailError(string error, Exception x)
        {
            // Send any attached Data along with error
            XElement errorText = new XElement("xml", new XElement("messages", error));

            foreach (string key in x.Data.Keys)
            {
                errorText.Add(
                    new XElement("dataitem",
                        new XAttribute("key", key),
                        new XElement("value", x.Data[key].ToString())
                    )
                );
            }

            //TODO: LogException(System.Environment.MachineName, "NR5X_UploadService", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, errorText.ToString(), 100, x.ToString(), true);
        }

        private ArrayList GetExcelSheetNames(string strConn)
        {
            OleDbConnection objConn = null;
            DataTable dt = null;

            try
            {
                objConn = new OleDbConnection(strConn);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                ArrayList excelSheets = new ArrayList();
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets.Add(row["TABLE_NAME"].ToString());
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        private void SaveToCache()
        {
            byte[] fileBytes = Convert.FromBase64String(this.fileinfo.Element("File").Element("Data").Value);
            string filename = HttpContext.Current.Server.MapPath(@"~/uploads/results/") + this.CachedFileName;
            File.WriteAllBytes(filename, filebytes);
        }

        #endregion
    }

    #region UploadNameAttribute Class

    [AttributeUsage(AttributeTargets.Class)]
    public class UploadNameAttribute : Attribute
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public UploadNameAttribute(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    #endregion

    #region UploadException Class
    [global::System.Serializable]
    public class UploadException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UploadException() { }

        public UploadException(string message) : base(message) { }

        public UploadException(string message, Exception inner) : base(message, inner) { }

        protected UploadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

    }
    #endregion
}