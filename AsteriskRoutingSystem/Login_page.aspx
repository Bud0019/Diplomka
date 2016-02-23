<%@ Page Title="" Language="C#" MasterPageFile="~/Main_MasterPage.master" AutoEventWireup="true" CodeFile="Login_page.aspx.cs" Inherits="Login_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <asp:Login ID="Login" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#333333" CreateUserText="Registrovať" DisplayRememberMe="False" FailureText="Prihlásenie zlyhalo. Skúste to znova." Height="179px" LoginButtonText="Prihlásiť" PasswordLabelText="Heslo:" PasswordRequiredErrorMessage="Musíte zadať heslo." TextLayout="TextOnTop" TitleText="Zadajte meno a heslo" UserNameLabelText="Užívateľské meno:" UserNameRequiredErrorMessage="Musíte zadať meno." Width="228px" CreateUserUrl="~/Register_page.aspx" OnAuthenticate="Login_Authenticate" DestinationPageUrl="~/LoggedUserSite/User_page.aspx">
        <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
        <LoginButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
        <TextBoxStyle Font-Size="0.8em" />
        <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
    </asp:Login>
</asp:Content>

