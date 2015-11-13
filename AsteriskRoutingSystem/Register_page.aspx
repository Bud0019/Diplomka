<%@ Page Title="" Language="C#" MasterPageFile="~/Main_MasterPage.master" AutoEventWireup="true" CodeFile="Register_page.aspx.cs" Inherits="Register_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <asp:CreateUserWizard ID="CreateUserWizard1" runat="server" DisplayCancelButton="True" CancelButtonText="Spať" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" CancelDestinationPageUrl="~/Login_page.aspx" CompleteSuccessText="Váš účet bol úspešne vytvorený." ConfirmPasswordCompareErrorMessage="Potvrdzovacie heslo sa musí zhodovať s heslom." ConfirmPasswordLabelText="Potvrdiť heslo:" ConfirmPasswordRequiredErrorMessage="Zadajte potvrdzovacie heslo." ContinueButtonText="Pokračovať" ContinueDestinationPageUrl="~/Login_page.aspx" CreateUserButtonText="Vytvoriť účet" DuplicateEmailErrorMessage="Emailová adresa už existuje. Zadajte iný email." DuplicateUserNameErrorMessage="Užívateľské meno už existuje. Zadajte iné meno." EmailRegularExpression="^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$" EmailRegularExpressionErrorMessage="Nesprávný formát emailu." EmailRequiredErrorMessage="Zadajte Email." InvalidEmailErrorMessage="Nesprávny formát emailu." InvalidPasswordErrorMessage="Minimálna dĺžka hesla je 5 znakov." PasswordLabelText="Heslo:" PasswordRequiredErrorMessage="Zadajte heslo." UnknownErrorMessage="Váš účet nebol vytvorený. Skúste to znova." UserNameLabelText="Užívateľské meno:" UserNameRequiredErrorMessage="Zadajte užívateľské meno. ">
        <ContinueButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" ForeColor="#284E98" />
        <CreateUserButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" ForeColor="#284E98" />
        <TitleTextStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
        <WizardSteps>
            <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server" Title="Nový účet">
            </asp:CreateUserWizardStep>
            <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
            </asp:CompleteWizardStep>
        </WizardSteps>
        <HeaderStyle BackColor="#284E98" BorderColor="#EFF3FB" BorderStyle="Solid" BorderWidth="2px" Font-Bold="True" Font-Size="0.9em" ForeColor="White" HorizontalAlign="Center" />
        <NavigationButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" ForeColor="#284E98" />
        <SideBarButtonStyle BackColor="#507CD1" Font-Names="Verdana" ForeColor="White" />
        <SideBarStyle BackColor="#507CD1" Font-Size="0.9em" VerticalAlign="Top" />
        <StepStyle Font-Size="0.8em" />
    </asp:CreateUserWizard>
</asp:Content>

