using System;

namespace ManitobaMasterBowlers_com
{
    public partial class _template : System.Web.UI.Page
    {
        public string FileName {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(System.Web.HttpContext.Current.Request.Url.AbsolutePath);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            editControls1.Visible = User.Identity.IsAuthenticated;
            editControls2.Visible = User.Identity.IsAuthenticated;
        }
    }
}
