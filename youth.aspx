<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="youth.aspx.cs" Inherits="ManitobaMasterBowlers_com.youth" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentBlock edit_mce" id="youthcb"></div>
    <asp:PlaceHolder runat="server" ID="editControls1" Visible="false">
        <a id="youthcb_edit" onclick="javascript: EditContentBlock($('div#youthcb'));" href="#">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <div class="contentBlock edit_mce" id="youthcb_sidebar"></div>
    <asp:PlaceHolder runat="server" ID="editControls2" Visible="false">
        <a id="youthcb_sidebar_edit" onclick="javascript: EditContentBlock($('div#youthcb_sidebar'));" href="#">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
