<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="_template.aspx.cs" Inherits="ManitobaMasterBowlers_com._template" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentBlock edit_mce" id="<%= this.FileName %>"></div>
    <asp:PlaceHolder runat="server" ID="editControls1" Visible="false">
        <a id="<%= this.FileName %>_edit" onclick="javascript: EditContentBlock($('div').filter('#<%= this.FileName %>'));" href="#">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <div class="contentBlock edit_mce" id="<%= this.FileName %>_sidebar"></div>
    <asp:PlaceHolder runat="server" ID="editControls2" Visible="false">
        <a id="<%= this.FileName %>_sidebar_edit" onclick="javascript: EditContentBlock($('div#<%= this.FileName %>_sidebar'));" href="#">Edit</a> 
    </asp:PlaceHolder>
</asp:Content>
