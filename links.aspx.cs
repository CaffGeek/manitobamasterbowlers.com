using System;
using System.Configuration;

namespace ManitobaMasterBowlers_com
{
    public partial class links : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Masters_ConnectionString"].ConnectionString;
            AccessDataSource1.ConnectionString = connectionString;
        }
    }
}
