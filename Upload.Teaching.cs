using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;

namespace ManitobaMasterBowlers_com
{
    public class UploadTeaching : Upload
    {

        protected override XElement BuildUploadXML()
        {
            // Get and transform this effectively does basic format validation
            XElement results = this.GetWorksheetXML(0, HttpContext.Current.Server.MapPath(@"~\Xslt\Teaching\TeachingResults.xslt"));

            // Unify and return
            XsltArgumentList xal = new XsltArgumentList();

            // Set some other parameters
            xal.AddParam("fileName", "", this.Filename);
            xal.AddParam("tournamentId", "", this.TournamentId);

            XElement transformed = XElement.Parse(Utility.Transform(results.ToString(), HttpContext.Current.Server.MapPath(@"~\Xslt\Teaching\Teaching.xslt"), xal));
            return transformed;
        }

        protected override XElement Validate(XElement item)
        {
            // Validate with XSD
            XElement xsdErrors = this.ValidateXSD(item, HttpContext.Current.Server.MapPath(@"~\Xslt\Teaching\Teaching.xsd"));
            if (xsdErrors != null && xsdErrors.Elements("Error").Count() > 0)
            {
                return xsdErrors;
            }

            // Validate with Stored Proc
            string result = string.Empty;
            using (Persistence oDB = new Persistence())
            {
                Persistence.ParameterCollection parameters = new Persistence.ParameterCollection();
                parameters.Add("InputXml");
                parameters[0].Value = item.ToString();
                oDB.Execute("Masters_ConnectionString", "ValidateTeachingData", parameters, out result);
            }

            if (!string.IsNullOrEmpty(result))
            {
                XElement spErrors = XElement.Parse(result);
                spErrors.Elements("Error").Where(e => string.IsNullOrEmpty(e.Value)).Remove();

                if (spErrors.Elements("Error").Count() > 0)
                {
                    return spErrors;
                }
            }

            // SUCCESS!
            return null;

        }

        protected override void Commit(XElement item)
        {
            // Save it
            using (Persistence oDB = new Persistence())
            {
                Persistence.ParameterCollection parameters = new Persistence.ParameterCollection();
                parameters.Add("InputXml");
                parameters[0].Value = item.ToString();
                oDB.Execute("Masters_ConnectionString", "SaveTeachingData", parameters);
            }
        }

    }
}
