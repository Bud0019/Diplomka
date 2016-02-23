<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="UserTransfer_page.aspx.cs" Inherits="LoggedUserSite_UserTransfer_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" Runat="Server">
    <asp:Label ID="Label_from" runat="server" Text="Presun z Asterisku:"></asp:Label>
    <asp:DropDownList ID="DropDownList_from" runat="server" DataSourceID="ObjectDataSource1" AppendDataBoundItems="true" DataTextField="name_Asterisk" DataValueField="name_Asterisk" OnSelectedIndexChanged="DropDownList_from_SelectedIndexChanged" AutoPostBack="True" ValidationGroup="transfer_validation">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="0" />
   </Items>
    </asp:DropDownList>
    <asp:Label ID="Label_to" runat="server" Text="Presun do Asterisku:"></asp:Label>
    <asp:DropDownList ID="DropDownList_to" runat="server" AppendDataBoundItems="true" DataSourceID="ObjectDataSource1" DataTextField="name_Asterisk" DataValueField="name_Asterisk" AutoPostBack="True" ValidationGroup="transfer_validation">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="0" />
   </Items>
    </asp:DropDownList>
    <br />
    <asp:Button ID="Button_transfer" runat="server" Text="Presunuť" OnClick="Button_transfer_Click" ValidationGroup="transfer_validation" />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <br />
    <asp:Label ID="Label_notSelected" runat="server" ForeColor="#CC0000" Text="Vyberte užívateľa." Visible="False"></asp:Label>
    <asp:CompareValidator ID="CompareValidator_checkMatch" runat="server" ErrorMessage="Zdroj a cieľ sa nesmú zhodovať." ControlToCompare="DropDownList_from" ControlToValidate="DropDownList_to" Display="Dynamic" ValidationGroup="transfer_validation" ForeColor="#CC0000" Operator="NotEqual"></asp:CompareValidator>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator_from" runat="server" ErrorMessage="Vyberte zdroj." ControlToValidate="DropDownList_from" Display="Dynamic" ForeColor="#CC0000" InitialValue="0" ValidationGroup="transfer_validation"></asp:RequiredFieldValidator>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator_to" runat="server" ErrorMessage="Vyberte cieľ." ControlToValidate="DropDownList_to" Display="Dynamic" ForeColor="#CC0000" InitialValue="0" ValidationGroup="transfer_validation"></asp:RequiredFieldValidator>
    <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="Vyberte užívateľa." Display="Dynamic" ForeColor="#CC0000"></asp:CustomValidator>
     <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="SelectAsterisksByUser" TypeName="AsteriskRoutingSystem.AsteriskAccessLayer">
        <SelectParameters>
            <asp:SessionParameter Name="userName" SessionField="loggedUser" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:GridView ID="GridView_userTransfer" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" PageSize="8" ShowFooter="True" ShowHeaderWhenEmpty="True" Width="456px" OnPageIndexChanging="GridView_userTransfer_PageIndexChanging" OnSelectedIndexChanged="GridView_userTransfer_SelectedIndexChanged">
         <AlternatingRowStyle BackColor="White" />
         <Columns>
                <asp:CommandField ShowSelectButton="true" SelectText="Vybrať"/>
                <asp:BoundField 
                    DataField="user_number"
                    HeaderText="Užívateľ"
                    />  
               <asp:TemplateField>
          <FooterTemplate>
              <div style="position:relative; float:left" > 
                  </div>                       
          </FooterTemplate>                 
     </asp:TemplateField>            
            </Columns>
      
         <EditRowStyle BackColor="#2461BF" HorizontalAlign="Center" VerticalAlign="Middle" />
         <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" Wrap="True" />
         <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" HorizontalAlign="Center" VerticalAlign="Middle" />
         <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
         <RowStyle BackColor="#EFF3FB" HorizontalAlign="Center" />
         <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
         <SortedAscendingCellStyle BackColor="#F5F7FB" />
         <SortedAscendingHeaderStyle BackColor="#6D95E1" />
         <SortedDescendingCellStyle BackColor="#E9EBEF" />
         <SortedDescendingHeaderStyle BackColor="#4870BE" />
    </asp:GridView>
    <div id="divLogUserTransfer">
        <asp:Label ID="UserTransferLog" runat="server" Text="Log:" Font-Bold="True"></asp:Label>
        <br />
        <asp:TextBox ID="TextBox_log" runat="server" Font-Size="Smaller" Height="261px" ReadOnly="True" TextMode="MultiLine" ValidateRequestMode="Disabled" Width="299px"></asp:TextBox>
    </div>
    </asp:Content>

