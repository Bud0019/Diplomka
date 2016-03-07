<%@ Page Title="" Language="C#" MasterPageFile="~/LoggedUserSite/User_MasterPage.master" AutoEventWireup="true" CodeFile="UserTransfer_page.aspx.cs" Inherits="LoggedUserSite_UserTransfer_page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <asp:label id="Label_from" runat="server" text="Presun z Asterisku:"></asp:label>
    <asp:dropdownlist id="DropDownList_from" runat="server" datasourceid="ObjectDataSource2" appenddatabounditems="True" datatextfield="name_Asterisk" datavaluefield="name_Asterisk" onselectedindexchanged="DropDownList_from_SelectedIndexChanged" autopostback="True" validationgroup="transfer_validation">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="0" />
   </Items>
    </asp:dropdownlist>
    <asp:label id="Label_to" runat="server" text="Presun do Asterisku:"></asp:label>
    <asp:dropdownlist id="DropDownList_to" runat="server" appenddatabounditems="True" datasourceid="ObjectDataSource2" datatextfield="name_Asterisk" datavaluefield="name_Asterisk" autopostback="True" validationgroup="transfer_validation">
        <Items>
       <asp:ListItem Text="--Vybrať--" Value="0" />
   </Items>
    </asp:dropdownlist>
    <br />
    <asp:button id="Button_transfer" runat="server" text="Presunuť" onclick="Button_transfer_Click" validationgroup="transfer_validation" />
    <asp:label id="Label_result" runat="server" text="Label" ForeColor="#009933" Visible="False"></asp:label>
    <br />
    <asp:label id="Label_notSelected" runat="server" forecolor="#CC0000" text="Vyberte užívateľa." visible="False"></asp:label>
    <asp:comparevalidator id="CompareValidator_checkMatch" runat="server" errormessage="Zdroj a cieľ sa nesmú zhodovať." controltocompare="DropDownList_from" controltovalidate="DropDownList_to" display="Dynamic" validationgroup="transfer_validation" forecolor="#CC0000" operator="NotEqual"></asp:comparevalidator>
    <asp:requiredfieldvalidator id="RequiredFieldValidator_from" runat="server" errormessage="Vyberte zdroj." controltovalidate="DropDownList_from" display="Dynamic" forecolor="#CC0000" initialvalue="0" validationgroup="transfer_validation"></asp:requiredfieldvalidator>
    <asp:requiredfieldvalidator id="RequiredFieldValidator_to" runat="server" errormessage="Vyberte cieľ." controltovalidate="DropDownList_to" display="Dynamic" forecolor="#CC0000" initialvalue="0" validationgroup="transfer_validation"></asp:requiredfieldvalidator>
    <asp:objectdatasource id="ObjectDataSource1" runat="server" selectmethod="selectTransferedUser" typename="TransferedUserAccessLayer">
        <SelectParameters>
            <asp:ControlParameter ControlID="DropDownList_from" Name="name_user" PropertyName="SelectedValue" Type="String" />
        </SelectParameters>
    </asp:objectdatasource>
    <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="SelectAsterisksByUser" TypeName="AsteriskAccessLayer">
        <SelectParameters>
            <asp:SessionParameter Name="userName" SessionField="loggedUser" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:gridview id="GridView_userTransfer" runat="server" allowpaging="True" autogeneratecolumns="False" cellpadding="4" forecolor="#333333" gridlines="None" pagesize="8" showfooter="True" showheaderwhenempty="True" width="456px" onpageindexchanging="GridView_userTransfer_PageIndexChanging" onselectedindexchanged="GridView_userTransfer_SelectedIndexChanged">
         <AlternatingRowStyle BackColor="White" />
         <Columns>
                <asp:CommandField ShowSelectButton="true" SelectText="Vybrať"/>
                <asp:BoundField 
                    DataField="name_user"
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
    </asp:gridview>
</asp:Content>

