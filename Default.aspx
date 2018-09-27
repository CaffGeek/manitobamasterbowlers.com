<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ManitobaMasterBowlers_com._Default"
    MasterPageFile="~/Site1.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContent">
    <%=System.Configuration.ConfigurationManager.ConnectionStrings["Masters_ConnectionString"].ConnectionString%>
    <div class="contentBlock edit_mce" id="homepage"></div>
    <asp:PlaceHolder runat="server" ID="editControls" Visible="false">
        <a id="homepage_edit" onclick="javascript: EditContentBlock($('div#homepage'));" href="#">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="SidebarContent">
    <img src="images/announce.gif" alt="announcements" />
    <br />
    <div id="announcements"></div>
    <asp:PlaceHolder runat="server" ID="editControls1" Visible="false">
        <a id="announcement_edit" href="admin_announcements.aspx">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
