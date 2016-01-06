<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="UserTransfer_page.aspx.cs" Inherits="LoggedUserSite_UserTransfer_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" Runat="Server">
    <asp:Label ID="Label_from" runat="server" Text="Presun z Asterisku:"></asp:Label>
    <asp:DropDownList ID="DropDownList_from" runat="server" DataSourceID="ObjectDataSource1" AppendDataBoundItems="true" DataTextField="name_Asterisk" DataValueField="name_Asterisk" OnSelectedIndexChanged="DropDownList_from_SelectedIndexChanged" AutoPostBack="True">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="" />
   </Items>
    </asp:DropDownList>
    <asp:Label ID="Label_to" runat="server" Text="Presun do Asterisku:"></asp:Label>
    <asp:DropDownList ID="DropDownList_to" runat="server" AppendDataBoundItems="true" DataSourceID="ObjectDataSource1" DataTextField="name_Asterisk" DataValueField="name_Asterisk">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="" />
   </Items>
    </asp:DropDownList>
     <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="SelectAsterisksByUser" TypeName="AsteriskRoutingSystem.AsteriskAccessLayer">
        <SelectParameters>
            <asp:SessionParameter Name="userName" SessionField="loggedUser" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:GridView ID="GridView_userTransfer" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" PageSize="8" ShowFooter="True" ShowHeaderWhenEmpty="True" Width="481px">
         <AlternatingRowStyle BackColor="White" />
         <Columns>
                <asp:CommandField ShowSelectButton="true" SelectText="Vybrať"/>
                <asp:BoundField 
                    DataField="user_number"
                    HeaderText="Užívateľ"
                    />  
               <asp:TemplateField>
          <FooterTemplate>
              
              <asp:Label runat="server" Text="Vyhľadať:" ID="Label_search"></asp:Label>
              <asp:TextBox ID="TextBox_search" runat="server" TextMode="Number" />
              <asp:ImageButton runat="server" ID="imageButton_search" Height="19px" ImageUrl="~/App_Themes/Style/Images/searchButton.png" style="margin-right: 0px" Width="27px"></asp:ImageButton>                          
          </FooterTemplate>                 
     </asp:TemplateField>            
            </Columns>
      
         <EditRowStyle BackColor="#2461BF" />
         <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
         <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
         <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
         <RowStyle BackColor="#EFF3FB" />
         <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
         <SortedAscendingCellStyle BackColor="#F5F7FB" />
         <SortedAscendingHeaderStyle BackColor="#6D95E1" />
         <SortedDescendingCellStyle BackColor="#E9EBEF" />
         <SortedDescendingHeaderStyle BackColor="#4870BE" />
    </asp:GridView>

    <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="getUsersByAsterisk" TypeName="TCPConnector">
        <SelectParameters>
            <asp:ControlParameter ControlID="DropDownList_from" Name="asteriskID" PropertyName="SelectedValue" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

</asp:Content>

