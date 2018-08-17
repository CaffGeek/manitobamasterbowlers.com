<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="links.aspx.cs" Inherits="ManitobaMasterBowlers_com.links" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Links</h3>
    <asp:ListView ID="ListView1" runat="server" DataSourceID="AccessDataSource1">
        <LayoutTemplate>
            <div ID="itemPlaceholderContainer" runat="server" style="">
                <span ID="itemPlaceholder" runat="server" />
            </div>
            <div style="">
            </div>
        </LayoutTemplate>
        <ItemTemplate>
            <p>
                <asp:HyperLink ID="Link" runat="server" NavigateUrl='<%# Eval("LinkAddress") %>'><%# Eval("LinkDescription")%></asp:HyperLink>
            </p>
        </ItemTemplate>
    </asp:ListView>
    <asp:SqlDataSource ID="AccessDataSource1" runat="server"
            ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
        SelectCommand="SELECT [LinkDescription], [LinkAddress] FROM [LinkTable] ORDER BY [LinkOrder]">
    </asp:SqlDataSource>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <img src="images/links_header.gif" alt="links" />
    <br />
</asp:Content>
