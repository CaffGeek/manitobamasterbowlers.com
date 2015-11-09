<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SiteMap.aspx.cs" Inherits="ManitobaMasterBowlers_com.SiteMap"
    MasterPageFile="~/Site1.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContent">
    <asp:ScriptManagerProxy ID="ScriptManager1" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/js/sitemap.js" />
        </Scripts>
    </asp:ScriptManagerProxy>
    
    <h3>Sitemap</h3>
    <ul class="ExistingPages">
        
    </ul>    
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="SidebarContent">
    <asp:PlaceHolder runat="server" ID="newPageControls" Visible="false">
        <h3>Create a new page</h3>
        <div class="NewPage">
            <p>Name: <input type="text" id="PageName" /></p>
            <span id="Create" class="StyledButton Green NewPage" onclick="javascript:Create_Click(this);">Create</span>
            <p>Example: budspud</p>
            <p>You do not need to enter an extension (.aspx) it will be added automatically.</p>
        </div>
    </asp:PlaceHolder>
</asp:Content>