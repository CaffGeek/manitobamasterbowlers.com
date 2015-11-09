using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

namespace ManitobaMasterBowlers_com
{
    public partial class board : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoginStatus1.Visible = User.Identity.IsAuthenticated;
            Login1.Visible = !User.Identity.IsAuthenticated;
        }

        protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
        {
            e.Authenticated = FormsAuthentication.Authenticate(Login1.UserName, Login1.Password);
        }
    }
}
