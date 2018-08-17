<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="board.aspx.cs"
    Inherits="ManitobaMasterBowlers_com.board" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ListView ID="ListView1" runat="server" DataSourceID="AccessDataSource1">
        <LayoutTemplate>
            <div id="itemPlaceholderContainer" runat="server" style="">
                <span id="itemPlaceholder" runat="server" />
            </div>
            <div style="">
            </div>
        </LayoutTemplate>
        <ItemTemplate>
            <p>
                <strong>
                    <asp:Label ID="PositionLabel" runat="server" Text='<%# Eval("Position") %>' />
                    -
                    <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' /></strong><br>
                <asp:Label ID="PhoneLabel" runat="server" Text='<%# Eval("Phone") %>' />
                •
                <asp:HyperLink ID="EmailLink" runat="server" NavigateUrl='<%# "mailto:" + Eval("Email") %>'><%# Eval("Email") %></asp:HyperLink>
            </p>
        </ItemTemplate>
    </asp:ListView>
    <asp:SqlDataSource ID="AccessDataSource1" runat="server" ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
        SelectCommand="SELECT [Name], [Position], [Phone], [Email] FROM [BoardMembers] ORDER BY [Order]">
    </asp:SqlDataSource>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <img src="images/board_header.gif" alt="board" />
    <br />
    <asp:Login ID="Login1" runat="server" OnAuthenticate="Login1_Authenticate" DisplayRememberMe="false"
        Width="200px" LoginButtonType="Link" UserNameLabelText="User:&nbsp;">
    </asp:Login>
    <asp:LoginStatus ID="LoginStatus1" runat="server" />
</asp:Content>
