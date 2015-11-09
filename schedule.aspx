<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="schedule.aspx.cs"
    Inherits="ManitobaMasterBowlers_com.schedule" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentBlock edit_mce" id="schedulecb"></div>
    <asp:PlaceHolder runat="server" ID="editControls" Visible="false">
        <a id="schedulecb_edit" onclick="javascript: EditContentBlock($('div#schedulecb'));" href="#">Edit</a> 
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <img src="images/schedule_header.gif" alt="schedule" />
    <br />
    <div class="contentBlock edit_mce" id="schedulecb_sidebar"></div>
    <asp:PlaceHolder runat="server" ID="editControls2" Visible="false">
        <a id="schedulecb_sidebar_edit" onclick="javascript: EditContentBlock($('div#schedulecb_sidebar'));" href="#">Edit</a> 
    </asp:PlaceHolder>
</asp:Content>
