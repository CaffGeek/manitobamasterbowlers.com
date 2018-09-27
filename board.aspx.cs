using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace ManitobaMasterBowlers_com
{
    public partial class board : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoginStatus1.Visible = User.Identity.IsAuthenticated;
            Login1.Visible = !User.Identity.IsAuthenticated;
            editControls.Visible = User.Identity.IsAuthenticated;
        }

        protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
        {
            e.Authenticated = FormsAuthentication.Authenticate(Login1.UserName, Login1.Password);
        }
    }
}
