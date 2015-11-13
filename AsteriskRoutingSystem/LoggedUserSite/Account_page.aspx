<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="Account_page.aspx.cs" Inherits="LoggedUserSite_Account_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" Runat="Server">

    <asp:Label ID="Label1" runat="server" Text="Zmena hesla alebo emailu."></asp:Label>
<br />
<asp:ChangePassword ID="ChangePassword1" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" CancelButtonText="Späť" Font-Names="Verdana" Font-Size="0.8em" Width="461px">
    <CancelButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
    <ChangePasswordButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
    <ContinueButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
    <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
    <PasswordHintStyle Font-Italic="True" ForeColor="#507CD1" />
    <TextBoxStyle Font-Size="0.8em" />
    <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
</asp:ChangePassword>
<br />
<asp:Label ID="Label2" runat="server" Text="Nový Email:"></asp:Label>
    <br />
<asp:TextBox ID="TextBox1" runat="server" Width="289px"></asp:TextBox>
    <br />
    <asp:Button ID="Button1" runat="server" Text="Zmeniť email" />
<br />

</asp:Content>

