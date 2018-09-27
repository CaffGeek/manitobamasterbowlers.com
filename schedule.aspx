<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="schedule.aspx.cs"
    Inherits="ManitobaMasterBowlers_com.schedule" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <iframe src="https://calendar.google.com/calendar/embed?showTitle=0&amp;showNav=0&amp;showDate=0&amp;showTabs=0&amp;showCalendars=0&amp;showTz=0&amp;mode=AGENDA&amp;height=600&amp;wkst=1&amp;bgcolor=%23FFFFFF&amp;src=890j7vinhjiqert62c6cg0to3g%40group.calendar.google.com&amp;color=%235F6B02&amp;ctz=America%2FChicago"
        style="border-width:0" 
        width="565" 
        height="500" 
        frameborder="0" 
        scrolling="no"></iframe>
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
