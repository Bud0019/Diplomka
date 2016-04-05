<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="AsterisksManagement_page.aspx.cs" 
    Inherits="LoggedUserSite_AsterisksMnt_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <div id="AsteriskManagmentDiv">
        <asp:label id="Label_AsteriskManagment" runat="server" text="Správa Asteriskov" font-bold="True"></asp:label>
        <asp:gridview id="GridView_Asterisks" runat="server" allowpaging="True" autogeneratecolumns="False" cellpadding="4" onselectedindexchanged="OnSelectedIndexChanged" 
            datasourceid="Asterisk_ObjectDataSource" forecolor="#333333" gridlines="None" showheaderwhenempty="True" datakeynames="id_Asterisk,tls_certDestination" pagesize="8" width="423px">
            <AlternatingRowStyle BackColor="White" />
            <Columns>
                <asp:CommandField ShowSelectButton="true" SelectText="Upraviť" />
                <asp:BoundField
                    DataField="name_Asterisk"
                    HeaderText="Názov" />
                <asp:BoundField
                    DataField="prefix_Asterisk"
                    HeaderText="Prefix" />
                <asp:BoundField
                    DataField="ip_address"
                    HeaderText="IP adresa" />
                <asp:BoundField
                    DataField="login_AMI"
                    HeaderText="AMI login" />
                <asp:BoundField
                    DataField="tls_enabled"
                    HeaderText="tls" />
                <asp:BoundField
                    DataField="tls_certDestination"
                    HeaderText="certDestination"
                    Visible="false" />
            </Columns>
            <EditRowStyle BackColor="#2461BF" />
            <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#EFF3FB" />
            <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#F5F7FB" />
            <SortedAscendingHeaderStyle BackColor="#6D95E1" />
            <SortedDescendingCellStyle BackColor="#E9EBEF" />
            <SortedDescendingHeaderStyle BackColor="#4870BE" />
        </asp:gridview>

        <asp:objectdatasource id="Asterisk_ObjectDataSource" runat="server" selectmethod="SelectAsterisksByUser" typename="AsteriskAccessLayer">
            <SelectParameters>
                <asp:SessionParameter DefaultValue="" Name="userName" SessionField="loggedUser" Type="String" />
            </SelectParameters>
        </asp:objectdatasource>
    </div>
    <div id="manageAsteriskDiv">
        <asp:label id="Label_addAsterisk" runat="server" text="Pridať Asterisk" font-bold="True"></asp:label>
        <br />
        <asp:label id="Label_name" runat="server" text="Názov:"></asp:label>
        <br />
        <asp:textbox id="TextBox_name" runat="server" validationgroup="vg_addAsterisk" maxlength="11"></asp:textbox>
        <asp:requiredfieldvalidator id="RequiredFieldValidator_name" runat="server" controltovalidate="TextBox_name" display="Dynamic" errormessage="Musíte zadať názov." forecolor="#CC0000" validationgroup="vg_addAsterisk"></asp:requiredfieldvalidator>
        <br />
        <asp:label id="Label_ipAddress" runat="server" text="IP adresa:"></asp:label>
        <br />
        <asp:textbox id="TextBox_ipAddress" runat="server" validationgroup="vg_addAsterisk"></asp:textbox>
        <asp:requiredfieldvalidator id="RequiredFieldValidator_ipAddress" runat="server" controltovalidate="TextBox_ipAddress" errormessage="Musíte zadať ip adresu." display="Dynamic" forecolor="#CC0000" validationgroup="vg_addAsterisk"></asp:requiredfieldvalidator>
        <asp:regularexpressionvalidator id="RegularExpressionValidator_ipAddress" runat="server" controltovalidate="TextBox_ipAddress" errormessage="Neplatný tvar IP adresy." display="Dynamic" forecolor="#CC0000" validationexpression="^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$" validationgroup="vg_addAsterisk"></asp:regularexpressionvalidator>
        <br />
        <asp:label id="Label_prefix" runat="server" text="Prefix Asterisku:"></asp:label>
        <br />
        <asp:textbox id="TextBox_prefix" runat="server" maxlength="9" textmode="Number" validationgroup="vg_addAsterisk"></asp:textbox>
        <asp:requiredfieldvalidator id="RequiredFieldValidator1" runat="server" display="Dynamic" errormessage="Musíte zadať prefix Asterisku." forecolor="#CC0000" validationgroup="vg_addAsterisk" controltovalidate="TextBox_prefix"></asp:requiredfieldvalidator>
        <br />
        <asp:label id="Label_login" runat="server" text="Login:"></asp:label>
        <br />
        <asp:textbox id="TextBox_login" runat="server" enabletheming="True" validationgroup="vg_addAsterisk" maxlength="15"></asp:textbox>
        <asp:requiredfieldvalidator id="RequiredFieldValidator_login" runat="server" controltovalidate="TextBox_login" errormessage="Musíte zadať login." display="Dynamic" forecolor="#CC0000" validationgroup="vg_addAsterisk"></asp:requiredfieldvalidator>
        <br />
        <asp:label id="Label_password" runat="server" text="Heslo:"></asp:label>
        <br />
        <asp:textbox id="TextBox_password" runat="server" textmode="Password" validationgroup="vg_addAsterisk"></asp:textbox>
        <asp:requiredfieldvalidator id="RequiredFieldValidator_password" runat="server" controltovalidate="TextBox_password" errormessage="Musíte zadať heslo." display="Dynamic" forecolor="#CC0000" validationgroup="vg_addAsterisk"></asp:requiredfieldvalidator>
        <br />
        <asp:label id="Label_TLS" runat="server" text="Povoliť TLS:"></asp:label>
        <asp:checkbox id="CheckBox_TLS" runat="server" />
        <br />
        <asp:label id="Label_certificate" runat="server" text="Umiestnenie certifikátu:"></asp:label>
        <br />
        <asp:textbox id="TextBox_certDestination" runat="server"></asp:textbox>
        <br />

        <asp:button id="Button_confirm" runat="server" text="Pridať" validationgroup="vg_addAsterisk" onclick="Button_confirm_Click" />

        <asp:button id="Button_edit" runat="server" text="Upraviť" validationgroup="vg_addAsterisk" visible="False" onclick="Button_edit_Click" />

        <asp:button id="Button_delete" runat="server" onclick="Button_delete_Click" text="Zmazať" visible="False" />

        <asp:button id="Button_cancel" runat="server" onclick="Button_cancel_Click" text="Zrušiť" visible="False" />

        <br />
        <asp:label id="response_label" runat="server" backcolor="White" forecolor="#009933" text="Label" visible="False"></asp:label>
        <br />

        <asp:label id="Label_deletePermanently" runat="server" text="Odstrániť aj tak?(Vymazanie chybného Asterisku sa preskočí)" forecolor="#CC0000"></asp:label>
        <br />
        <asp:button id="Button_confirmDelete" runat="server" text="Áno" onclick="Button_confirmDelete_Click" />
        <asp:button id="Button_denyDelete" runat="server" text="Nie" onclick="Button_denyDelete_Click" />
    </div>
</asp:Content>

