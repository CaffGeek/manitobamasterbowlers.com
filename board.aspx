<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="board.aspx.cs"
    Inherits="ManitobaMasterBowlers_com.board" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentBlock edit_mce" id="board"></div>
    <asp:PlaceHolder runat="server" ID="editControls" Visible="false">
        <a id="homepage_edit" onclick="javascript: EditContentBlock($('div#homepage'));" href="#">Edit</a>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <img src="images/board_header.gif" alt="board" />
    <br />
    <asp:Login ID="Login1" runat="server" OnAuthenticate="Login1_Authenticate" DisplayRememberMe="false"
        Width="200px" LoginButtonType="Link" UserNameLabelText="User:&nbsp;">
    </asp:Login>
    <asp:LoginStatus ID="LoginStatus1" runat="server" />
</asp:Content>
