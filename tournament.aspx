<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="tournament.aspx.cs" Inherits="ManitobaMasterBowlers_com.tournament" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="font-weight: bold;">
        Year 
        <asp:DropDownList runat="server" ID="Year" AutoPostBack="true" 
            DataSourceID="dsSeason" DataTextField="SeasonDesc" 
            DataValueField="SeasonCode"></asp:DropDownList>
        <asp:SqlDataSource ID="dsSeason" runat="server" 
            ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
            SelectCommand="SELECT [SeasonCode], [SeasonDesc], [Id] FROM [SeasonTable] ORDER BY [SeasonDesc] DESC">
        </asp:SqlDataSource>
        Tournament  
        <asp:DropDownList runat="server" ID="Tournament" AutoPostBack="true" 
            DataSourceID="dsTournament" DataTextField="TournamentLocation" 
            DataValueField="Id" OnDataBound="Tournament_DataBound"></asp:DropDownList>
        <asp:SqlDataSource ID="dsTournament" runat="server" 
                ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
                SelectCommand="SELECT [Id], [TournamentNumber], [TournamentLocation] FROM [TournamentTable] WHERE (([SeasonCode] = @SeasonCode) AND ([Division] = @Division)) ORDER BY TournamentNumber">
            <SelectParameters>
                <asp:ControlParameter ControlID="Year" Name="SeasonCode" PropertyName="SelectedValue" Type="String" DefaultValue="2008" />
                <asp:QueryStringParameter QueryStringField="Division" Name="Division" DefaultValue="Tournament" Type="String" />
            </SelectParameters>
        </asp:SqlDataSource>
        Gender  
        <asp:DropDownList runat="server" ID="Gender" AutoPostBack="true">
            <asp:ListItem Selected="True" Text="Both" Value="%"></asp:ListItem>
            <asp:ListItem Selected="False" Text="Ladies" Value="L"></asp:ListItem>
            <asp:ListItem Selected="False" Text="Men" Value="M"></asp:ListItem>
        </asp:DropDownList>
        <asp:Button runat="server" ID="go" Text="Go" />
        <asp:ListView ID="TournamentResults" runat="server" DataSourceID="dsTournamentResults">
            <LayoutTemplate>
                <table ID="itemPlaceholderContainer" runat="server" border="0" style="">
                    <tr runat="server" style="">
                        <th runat="server">Name</th>
                        <th runat="server">1</th>
                        <th runat="server">2</th>
                        <th runat="server">3</th>
                        <th runat="server">4</th>
                        <th runat="server">5</th>
                        <th runat="server">6</th>
                        <th runat="server">7</th>
                        <th runat="server">8</th>
                        <th runat="server">Total</th>
                    </tr>
                    <tr ID="itemPlaceholder" runat="server">
                    </tr>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr style="">
                    <td>
                        <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            &nbsp;avg: <asp:Label ID="BowlerAverageLabel" runat="server" Text='<%# Eval("BowlerAverage") %>' />
                        </asp:Panel>
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("POA1") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game1Label" runat="server" Text='<%# Eval("Game1") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("POA2") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game2Label" runat="server" Text='<%# Eval("Game2") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label3" runat="server" Text='<%# Eval("POA3") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game3Label" runat="server" Text='<%# Eval("Game3") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("POA4") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game4Label" runat="server" Text='<%# Eval("Game4") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label8" runat="server" Text='<%# Eval("POA5") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game5Label" runat="server" Text='<%# Eval("Game5") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("POA6") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game6Label" runat="server" Text='<%# Eval("Game6") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label6" runat="server" Text='<%# Eval("POA7") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game7Label" runat="server" Text='<%# Eval("Game7") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("POA8") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game8Label" runat="server" Text='<%# Eval("Game8") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="TotalPOALabel" runat="server" Text='<%# Eval("TotalPOA") %>' />
                        </asp:Panel>
                        <asp:Label ID="TotalScratchLabel" runat="server" Text='<%# Eval("TotalScratch") %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                
            </EmptyDataTemplate>
        </asp:ListView>
        <asp:SqlDataSource ID="dsTournamentResults" runat="server" 
            ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
            SelectCommandType="StoredProcedure"
            SelectCommand="TournamentAllResults">
            <SelectParameters>
                <asp:QueryStringParameter DefaultValue="Tournament" Name="Division" 
                    QueryStringField="Division" Type="String" />
                <asp:ControlParameter ControlID="Gender" DefaultValue="%" Name="Gender" 
                    PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="Year" DefaultValue="2008" 
                    Name="SeasonYear" PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="Tournament" DefaultValue="" 
                    Name="TournamentId" PropertyName="SelectedValue" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:ListView ID="TournamentSummary" runat="server" 
            DataSourceID="dsTournamentSummary">
            <LayoutTemplate>
                <table ID="itemPlaceholderContainer" runat="server" border="0" style="text-align:right;">
                    <tr runat="server" style="text-align:center;">
                        <th style="width:30%;" runat="server">Name</th>
                        <th style="width:10%;" runat="server">1</th>
                        <th style="width:10%;" runat="server">2</th>
                        <th style="width:10%;" runat="server">3</th>
                        <th style="width:10%;" runat="server">4</th>
                        <th style="width:10%;" runat="server">5</th>
                        <th style="width:10%;" runat="server">6</th>
                        <th style="width:10%;" runat="server">Total</th>
                    </tr>
                    <tr ID="itemPlaceholder" runat="server">
                    </tr>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr style="">
                    <td style="text-align:left;">
                        <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("Tournament1POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game1Label" runat="server" Text='<%# Eval("Tournament1Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("Tournament2POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game2Label" runat="server" Text='<%# Eval("Tournament2Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label3" runat="server" Text='<%# Eval("Tournament3POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game3Label" runat="server" Text='<%# Eval("Tournament3Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("Tournament4POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game4Label" runat="server" Text='<%# Eval("Tournament4Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label8" runat="server" Text='<%# Eval("Tournament5POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game5Label" runat="server" Text='<%# Eval("Tournament5Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("Tournament6POA") %>' />
                        </asp:Panel>
                        <asp:Label ID="Game6Label" runat="server" Text='<%# Eval("Tournament6Scratch") %>' />
                    </td>
                    <td>
                        <asp:Panel runat="server" Visible="<%# isPOA %>" style="display:block;">
                            <asp:Label ID="TotalPOALabel" runat="server" Text='<%# Eval("TotalPOA") %>' />
                        </asp:Panel>
                        <asp:Label ID="TotalScratchLabel" runat="server" Text='<%# Eval("TotalScratch") %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                
            </EmptyDataTemplate>
        </asp:ListView>
        <asp:SqlDataSource ID="dsTournamentSummary" runat="server" 
            ConnectionString="Data Source=VCNSQL81.webhost4life.com;Initial Catalog=Masters_85900;Persist Security Info=True;User ID=gszpak_85900;Password=prince"
            SelectCommandType="StoredProcedure"
            SelectCommand="Top4TotalResults">
            <SelectParameters>
                <asp:QueryStringParameter DefaultValue="Tournament" Name="Division" 
                    QueryStringField="Division" Type="String" />
                <asp:ControlParameter ControlID="Year" DefaultValue="2008" Name="Year" 
                    PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="Gender" DefaultValue="B" Name="Gender" 
                    PropertyName="SelectedValue" Type="String" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
    <asp:Panel runat="server" ID="ErrorPopup" CssClass="errorPopup" Visible="false">
    </asp:Panel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContent" runat="server">
    <asp:Image runat="server" ID="SidebarHeader" />
    <br />
    <asp:PlaceHolder ID="Sidebar" runat="server">
    </asp:PlaceHolder>
</asp:Content>
