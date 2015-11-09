using System;

namespace ManitobaMasterBowlers_com
{
    public partial class SiteMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            newPageControls.Visible = User.Identity.IsAuthenticated;
        }
    }
}
