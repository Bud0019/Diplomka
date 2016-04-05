<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="Error_page.aspx.cs" Inherits="LoggedUserSite_Error_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <h1>Upps, nastala neočakávana chyba !</h1>
    <asp:button runat="server" text="Domov" id="button_errorBack" postbackurl="~/LoggedUserSite/User_page.aspx" />
</asp:Content>

