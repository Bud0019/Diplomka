<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="Account_page.aspx.cs" Inherits="LoggedUserSite_Account_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    
    <asp:Label ID="Label1" runat="server" text="Zmena hesla alebo emailu."></asp:Label>
    <br />
    <asp:ChangePassword ID="ChangePassword1" runat="server" backcolor="#EFF3FB" bordercolor="#B5C7DE" borderpadding="4" borderstyle="Solid" borderwidth="1px" cancelbuttontext="Späť" font-names="Verdana" font-size="0.8em" width="461px">
        <CancelButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
        <ChangePasswordButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
        <ContinueButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
        <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
        <PasswordHintStyle Font-Italic="True" ForeColor="#507CD1" />
        <TextBoxStyle Font-Size="0.8em" />
        <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
    </asp:ChangePassword>
    <asp:Label ID="Label2" runat="server" text="Nový Email:"></asp:Label>
    <br />
    <asp:TextBox ID="TextBox1" runat="server" width="289px"></asp:TextBox>
    <br />
    <asp:Button ID="Button1" runat="server" text="Zmeniť email" />
    
</asp:Content>

