using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace ManitobaMasterBowlers_com
{
    public partial class tournament : System.Web.UI.Page
    {
        #region isPOA
        public bool isPOA
        {
            get
            {
                return Division != "Tournament";
            }
        }
        #endregion

        #region TournamentId
        public int TournamentId
        {
            get
            {
                int i = 0;
                int.TryParse(Tournament.SelectedValue, out i);
                return i;
            }
        }
        #endregion

        #region Division
        public string Division
        {
            get
            {
                return HttpContext.Current.Request.QueryString["Division"] ?? "Tournament";
            }
        }
        #endregion

        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Masters_ConnectionString"].ConnectionString;
            dsTournamentResults.ConnectionString = connectionString;
            dsTournament.ConnectionString = connectionString;
            dsSeason.ConnectionString = connectionString;
            dsTournamentSummary.ConnectionString = connectionString;


            SidebarHeader.ImageUrl = "images/" + Division + "_header.gif";
            SidebarHeader.ToolTip = Division;

            string newCommand = TournamentId > 0
                ? dsTournamentResults.SelectCommand
                : dsTournamentSummary.SelectCommand;

            if (isPOA)
            {
                TournamentResults.Sort("TotalPOA", SortDirection.Descending);
                TournamentSummary.Sort("TotalPOA", SortDirection.Descending);
            }
            else
            {
                TournamentResults.Sort("TotalScratch", SortDirection.Descending);
                TournamentSummary.Sort("TotalScratch", SortDirection.Descending);
            }

            TournamentResults.Visible = TournamentId > 0;
            TournamentSummary.Visible = !TournamentResults.Visible;

            //Add controls for sidebar content and editing
            PlaceHolder ph =
                new PlaceHolder()
                {
                    ID = "editControls",
                    Visible = User.Identity.IsAuthenticated
                };

            if (User.Identity.IsAuthenticated && TournamentId > 0)
            {
                ph.Controls.Add(new Label() { ID = "uploadTournament", Text = "Upload Results" });
                ph.Controls.Add(new LiteralControl(@" - <a href=""uploads/" + Division + @"Template.xls"">Template</a>"));
                ph.Controls.Add(new LiteralControl("<br/>"));
                ph.Controls.Add(new FileUpload() { ID = "UploadFile" });
                ph.Controls.Add(new LiteralControl("<br/>"));
                Button uploadButton = new Button() { Text = "Upload" };
                uploadButton.Click += UploadFile_Click;
                ph.Controls.Add(uploadButton);
                ph.Controls.Add(new LiteralControl("<hr/>"));
            }

            ph.Controls.Add(new LiteralControl(@"<a id=""" + Division + @"cb_sidebar_edit"" onclick=""javascript: EditContentBlock($('div#" + Division + @"cb_sidebar'));"" href=""#"">Edit</a>"));           
            Sidebar.Controls.Add(ph);
            Sidebar.Controls.Add(new LiteralControl(@"<div class=""contentBlock edit_mce"" id=""" + Division + @"cb_sidebar""></div>"));
        }
        #endregion

        #region Tournament_DataBound
        protected void Tournament_DataBound(object sender, EventArgs e)
        {
           Tournament.Items.Insert(0, new ListItem("Summary", "0"));
        }
        #endregion

        #region UploadFile_Click
        protected void UploadFile_Click(object sender, EventArgs e)
        {
            FileUpload uploadFile = Sidebar.FindControl("UploadFile") as FileUpload;

            XElement filter = new XElement("Filter",
                                new XElement("File",
                                    new XElement("Name", uploadFile.PostedFile.FileName),
                                    new XElement("TournamentId", TournamentId),
                                    new XElement("Data", Convert.ToBase64String(uploadFile.FileBytes))
                                )
                            );
            
            //Could change to use a dynamic invoke...we'll see
            string result = string.Empty;
            switch (Division.ToUpper())
            {
                case "TEACHING":
                    result = new UploadTeaching().Process(filter);
                    break;
                case "TOURNAMENT":
                    result = new UploadTournament().Process(filter);
                    break;
                case "SENIOR":
                    result = new UploadSenior().Process(filter);
                    break;
            }

            // All didn't go well
            ErrorPopup.Visible = false;
            if (!string.IsNullOrEmpty(result))
            {
                XElement errors = XElement.Parse(result);

                ErrorPopup.Controls.Clear();
                ErrorPopup.Visible = true;
                ErrorPopup.Controls.Add(new LinkButton() { CssClass = "closer", Text = "X", OnClientClick = "$('.errorPopup').hide(); return false;" });
                ErrorPopup.Controls.Add(new LiteralControl(Utility.Transform(errors.ToString(), HttpContext.Current.Server.MapPath(@"~\Xslt\UploadErrors.xslt"))));
            }
        }
        #endregion
    }
}
