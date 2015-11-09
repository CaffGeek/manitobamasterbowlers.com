<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="admin_announcements.aspx.cs"
    Inherits="ManitobaMasterBowlers_com.admin_announcements" ValidateRequest="false" %>
    
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContent">
    <asp:ScriptManagerProxy ID="ScriptManager1" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/js/admin_announcements.js" />
        </Scripts>
    </asp:ScriptManagerProxy>
    
    <div id="status"></div>

    <span class="EditAnnouncement">
        <textarea id="Announcement" class="Announcement" cols="50"></textarea>
        <br />
        <span class="Id" style="display:nonee;"></span>
        Start: <input type="text" class="StartDate" />
        Stop: <input type="text" class="EndDate" />
        <span onclick="javascript:Save_Click()" class="StyledButton Orange Save" title="Save" style="text-align: left;">Save</span>
        <span onclick="javascript:Clear_Click()" class="StyledButton Green Cancel" title="Clear" style="text-align: left;">Clear</span>
    </span>
    <hr />
    <div class="FixedHeaders" style="height: 300px;">
        <table id="dataTable" class="TableGrey" cellspacing="0">
            <col style="width: 80%;"/>
            <col style="width: 20%;"/>
            <thead>
                <tr>
                    <th colspan="2"><span id="Span1" style="display:none;">Loading...</span></th>
                </tr>
            </thead>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="SidebarContent" runat="server">
    <img src="images/announce.gif" alt="announcements" />
    <br />
    <div id="announcements"></div>
</asp:Content>