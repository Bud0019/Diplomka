<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="AsterisksManagment_page.aspx.cs" Inherits="LoggedUserSite_AsterisksMnt_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        #TextArea1 {
            height: 0px;
            width: 251px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" Runat="Server">
    
    <div id="AsteriskManagmentDiv">  
    <asp:Label ID="Label_AsteriskManagment" runat="server" Text="Správa Asteriskov" Font-Bold="True"></asp:Label>
        <asp:GridView ID="GridView_Asterisks" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" OnSelectedIndexChanged="OnSelectedIndexChanged" DataSourceID="Asterisk_ObjectDataSource" ForeColor="#333333" GridLines="None" ShowHeaderWhenEmpty="True" DataKeyNames="id_Asterisk, tls_certDestination" PageSize="8" Width="423px">
            <AlternatingRowStyle BackColor="White" />
            <Columns>
                <asp:CommandField ShowSelectButton="true" SelectText="Upraviť"/>
                <asp:BoundField 
                    DataField="name_Asterisk"
                    HeaderText="Názov"
                    />
                <asp:BoundField 
                    DataField="prefix_Asterisk"
                    HeaderText="Prefix"
                    />
                <asp:BoundField 
                    DataField="ip_address"
                    HeaderText="IP adresa"
                    />
                <asp:BoundField 
                    DataField="login_AMI"
                    HeaderText="AMI login"
                    /> 
                <asp:BoundField 
                    DataField="tls_enabled"
                    HeaderText="tls"                   
                    />  
                <asp:BoundField 
                    DataField="tls_certDestination"
                    HeaderText="certDestination"
                    Visible ="false"                   
                    />                 
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
        </asp:GridView>
        <asp:ObjectDataSource ID="Asterisk_ObjectDataSource" runat="server" SelectMethod="SelectAsterisksByUser" TypeName="AsteriskRoutingSystem.AsteriskAccessLayer">
            <SelectParameters>
                <asp:SessionParameter DefaultValue="" Name="userName" SessionField="loggedUser" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>      
    </div>
    <div id="manageAsteriskDiv">    
    <asp:Label ID="Label_addAsterisk" runat="server" Text="Pridať Asterisk" Font-Bold="True"></asp:Label>
        <br />
    <asp:Label ID="Label_name" runat="server" Text="Názov:"></asp:Label>
        <br />
    <asp:TextBox ID="TextBox_name" runat="server" ValidationGroup="vg_addAsterisk" MaxLength="11"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator_name" runat="server" ControlToValidate="TextBox_name" Display="Dynamic" ErrorMessage="Musíte zadať názov." ForeColor="#CC0000" ValidationGroup="vg_addAsterisk"></asp:RequiredFieldValidator>
        <br />
    <asp:Label ID="Label_ipAddress" runat="server" Text="IP adresa:"></asp:Label>
        <br />
    <asp:TextBox ID="TextBox_ipAddress" runat="server" ValidationGroup="vg_addAsterisk"></asp:TextBox>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator_ipAddress" runat="server" ControlToValidate="TextBox_ipAddress" ErrorMessage="Musíte zadať ip adresu." Display="Dynamic" ForeColor="#CC0000" ValidationGroup="vg_addAsterisk"></asp:RequiredFieldValidator>
    <asp:RegularExpressionValidator ID="RegularExpressionValidator_ipAddress" runat="server" ControlToValidate="TextBox_ipAddress" ErrorMessage="Neplatný tvar IP adresy." Display="Dynamic" ForeColor="#CC0000" ValidationExpression="^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$" ValidationGroup="vg_addAsterisk"></asp:RegularExpressionValidator>
    <br />
    <asp:Label ID="Label_prefix" runat="server" Text="Prefix Asterisku:"></asp:Label>
    <br />
    <asp:TextBox ID="TextBox_prefix" runat="server" MaxLength="9" TextMode="Number" ValidationGroup="vg_addAsterisk"></asp:TextBox>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Display="Dynamic" ErrorMessage="Musíte zadať prefix Asterisku." ForeColor="#CC0000" ValidationGroup="vg_addAsterisk" ControlToValidate="TextBox_prefix"></asp:RequiredFieldValidator>
    <br />
    <asp:Label ID="Label_login" runat="server" Text="Login:"></asp:Label>
        <br />
    <asp:TextBox ID="TextBox_login" runat="server" EnableTheming="True" ValidationGroup="vg_addAsterisk" MaxLength="15"></asp:TextBox>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator_login" runat="server" ControlToValidate="TextBox_login" ErrorMessage="Musíte zadať login." Display="Dynamic" ForeColor="#CC0000" ValidationGroup="vg_addAsterisk"></asp:RequiredFieldValidator>
    <br />
    <asp:Label ID="Label_password" runat="server" Text="Heslo:"></asp:Label>
        <br />
    <asp:TextBox ID="TextBox_password" runat="server" TextMode="Password" ValidationGroup="vg_addAsterisk"></asp:TextBox>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator_password" runat="server" ControlToValidate="TextBox_password" ErrorMessage="Musíte zadať heslo." Display="Dynamic" ForeColor="#CC0000" ValidationGroup="vg_addAsterisk"></asp:RequiredFieldValidator>
        <br />
        <asp:Label ID="Label_TLS" runat="server" Text="Povoliť TLS:"></asp:Label>
        <asp:CheckBox ID="CheckBox_TLS" runat="server" />
        <br />
        <asp:Label ID="Label_certificate" runat="server" Text="Umiestnenie certifikátu:"></asp:Label>
        <br />
        <asp:TextBox ID="TextBox_certDestination" runat="server"></asp:TextBox>
    <br />
        
    <asp:Button ID="Button_confirm" runat="server" Text="Pridať" ValidationGroup="vg_addAsterisk" OnClick="Button_confirm_Click" />
       
        <asp:Button ID="Button_edit" runat="server" Text="Upraviť" ValidationGroup="vg_addAsterisk" Visible="False" OnClick="Button_edit_Click" />
       
    <asp:Button ID="Button_delete" runat="server" OnClick="Button_delete_Click" Text="Zmazať" Visible="False" />
       
    <asp:Button ID="Button_cancel" runat="server" OnClick="Button_cancel_Click" Text="Zrušiť" Visible="False" />
       
    </div>
     <div id="logDiv">
        <asp:label runat="server" text="Log:" ID="Label_log" Font-Bold="True"></asp:label>
        <br />
        <asp:textbox runat="server" ID="TextBox_log"  Height="267px" TextMode="MultiLine" Width="293px" ClientIDMode="Static" Font-Size="Smaller" ReadOnly="True" ValidateRequestMode="Disabled"></asp:textbox>
        <br />
        <asp:button runat="server" text="Vyčistiť" ID="button_clear" OnClick="button_clear_Click" />
    </div>
   </asp:Content>

