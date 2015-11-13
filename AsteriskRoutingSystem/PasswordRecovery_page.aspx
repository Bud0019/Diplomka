<%@ Page Title="" Language="C#" MasterPageFile="~/Main_MasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecovery_page.aspx.cs" Inherits="PasswordRecovery_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" OnSendingMail="PasswordRecovery1_SendingMail" QuestionLabelText="">
        <SubmitButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
        <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
        <SuccessTextStyle Font-Bold="True" ForeColor="#507CD1" />
        <TextBoxStyle Font-Size="0.8em" />
        <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
    </asp:PasswordRecovery>
</asp:Content>

