using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web.Services;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Web.Script.Services;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using ManitobaMasterBowlers_com;
using System.Configuration;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ToolboxItem(false)]
[System.Web.Script.Services.ScriptService]
public class Masters : System.Web.Services.WebService
{

    SqlConnection _connection;
    SqlConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                ConnectionStringSettings connection = ConfigurationManager.ConnectionStrings["Masters_ConnectionString"];
                _connection = new SqlConnection(connection.ConnectionString);
            }
            return _connection;
        }
    }

    #region ContentBlocks
    [WebMethod]
    public string LoadContentBlock(string id)
    {
        string html = string.Empty;
        try
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();
            SqlCommand readCommand = new SqlCommand(@"SELECT TOP 1 ContentHTML FROM ContentBlocks WHERE ContentBlock = @id and DeleteDate IS NULL ORDER BY CreateDate DESC", Connection);
            readCommand.Parameters.AddWithValue("id", id);
            html = readCommand.ExecuteScalar() as string;
        }
        catch (Exception x)
        {
            return x.Message;
        }
        finally
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }
        return html ?? string.Empty;
    }

    [WebMethod]
    public string SaveContentBlock(string id, string html)
    {
        try
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();
            SqlCommand saveCommand = new SqlCommand(@"INSERT INTO ContentBlocks (ContentBlock, ContentHTML) VALUES (@id, @html)", Connection);
            saveCommand.Parameters.AddWithValue("id", id);
            saveCommand.Parameters.AddWithValue("html", html);
            //TODO: saveCommand.Parameters.AddWithValue("date", DateTime.Now);
            saveCommand.ExecuteNonQuery();
        }
        catch (Exception x)
        {
            return x.Message;
        }
        finally
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }
        return html;
    }
    #endregion

    #region Announcements
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
    public XElement LoadAnnouncements()
    {
        XElement output;
        try
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();

            SqlCommand readCommand = new SqlCommand("LoadAnnouncements", Connection);
            readCommand.CommandType = CommandType.StoredProcedure;
            XmlReader reader = readCommand.ExecuteXmlReader();
            reader.Read();

            string result = string.Empty; ;
            while (reader.ReadState != ReadState.EndOfFile)
            {
                result += reader.ReadOuterXml();
            }
            reader.Close();

            result = result.Replace("&amp;nbsp;", " ");
            output = XElement.Parse(HttpUtility.HtmlDecode(result));
        }
        catch (Exception x)
        {
            throw;
        }
        finally
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }
        return output;
    }

    [WebMethod]
    public bool SaveAnnouncement(string input)
    {
        bool output = false;

        try
        {
            XElement inputXml = XElement.Parse(input);

            if (Connection.State == ConnectionState.Closed) Connection.Open();
            
            //Save Announcement
            SqlCommand saveCommand = new SqlCommand(@"SaveAnnouncement", Connection);
            saveCommand.CommandType = CommandType.StoredProcedure;
            saveCommand.Parameters.Add(new SqlParameter("input", inputXml.ToString()));

            saveCommand.ExecuteNonQuery();

            output = true;
        }
        catch
        {
            throw;
        }
        finally
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }

        return output;
    }

    [WebMethod]
    public bool DeleteAnnouncement(string input)
    {
        bool output = false;

        try
        {
            XElement inputXml = XElement.Parse(input);

            if (Connection.State == ConnectionState.Closed) Connection.Open();

            //Save Announcement
            SqlCommand saveCommand = new SqlCommand(@"DeleteAnnouncement", Connection);
            saveCommand.CommandType = CommandType.StoredProcedure;
            saveCommand.Parameters.Add(new SqlParameter("input", inputXml.ToString()));

            saveCommand.ExecuteNonQuery();

            output = true;
        }
        catch
        {
            throw;
        }
        finally
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }

        return output;
    }
    #endregion

    #region Pages
    [WebMethod]
    public string[] GetPages(string inclusionRegexString, string exclusionRegexString)
    {
        Regex inclusionRegex = new Regex(inclusionRegexString ?? ".*", RegexOptions.IgnoreCase);
        Regex exclusionRegex = new Regex(exclusionRegexString ?? "", RegexOptions.IgnoreCase);

        string[] pages;
        string currentPath = HttpContext.Current.Server.MapPath("~");

        pages = (from f in Directory.GetFiles(currentPath)
                 where inclusionRegex.IsMatch(f) && !exclusionRegex.IsMatch(f)
                 select f.Replace(currentPath, "")
                ).OrderBy(s => s).ToArray();

        return pages;
    }

    [WebMethod]
    public bool CreatePage(string input)
    {
        bool output = false;

        try
        {
            XElement inputXml = XElement.Parse(input);
            string pageName = CreateValidFileName(inputXml.Element("PageName").Value) + ".aspx";

            //Copy template to New Page
            File.Copy(HttpContext.Current.Server.MapPath("_template.aspx"), HttpContext.Current.Server.MapPath(pageName));

            output = true;
        }
        catch
        {
            throw;
        }

        return output;
    }

    private string CreateValidFileName(string title)
    {
        string validFileName = title.Trim().Replace(" ", "");

        foreach (char invalChar in Path.GetInvalidFileNameChars())
        {
            validFileName = validFileName.Replace(invalChar.ToString(), "");
        }

        foreach (char invalChar in Path.GetInvalidPathChars())
        {
            validFileName = validFileName.Replace(invalChar.ToString(), "");
        }

        if (validFileName.Length > 160) //safe value threshold is 260
            validFileName = validFileName.Remove(156);

        return validFileName;
    } 
    #endregion

    #region Uploads

    [WebMethod()]
    public string UploadTeachingResultsFile(string filterXml)
    {
        return UploadFile(new UploadTeaching(), filterXml);
    }

    private string UploadFile(Upload upload, string filterXml)
    {
        // Process throws an exception if there is an error.
        upload.Process(XElement.Parse(filterXml));

        // Success!
        return string.Empty;
    }

    private string ExecStoredProc(string storedProc, string filterXml)
    {
        string procResult = string.Empty;

        using (Persistence oDB = new Persistence())
        {
            Persistence.ParameterCollection parameters = new Persistence.ParameterCollection();
            parameters.Add("filterXml");
            parameters[0].Value = filterXml;

            oDB.Execute("Masters_ConnectionString", storedProc, parameters, out procResult);
        }

        return procResult;
    }

    #endregion
}
