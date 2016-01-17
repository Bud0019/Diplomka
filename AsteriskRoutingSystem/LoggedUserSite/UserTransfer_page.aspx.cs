using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AsteriskRoutingSystem;
using System.Security.Cryptography;
using System.IO;
using System.Text;

public partial class LoggedUserSite_UserTransfer_page : System.Web.UI.Page
{
    TCPConnector tcp = new TCPConnector();
    AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();

    public List<string> usersList
    {
        get
        {
            if (this.ViewState["usersList"] == null)
            {
                this.ViewState["usersList"] = new List<string>();
            }
            return (List<string>)(this.ViewState["usersList"]);
        }
        set { this.ViewState["usersList"] = value; }
    }

    public List<string> usersDetailList
    {
        get
        {
            if (this.ViewState["usersDetailList"] == null)
            {
                this.ViewState["usersDetailList"] = new List<string>();
            }
            return (List<string>)(this.ViewState["usersDetailList"]);
        }
        set { this.ViewState["usersDetailList"] = value; }
    }

    public string fromAddress
    {
        get
        {
            if (ViewState["fromAddress"] != null)
                return (string)(ViewState["fromAddress"]);
            else
                return null;
        }
        set
        {
            ViewState["fromAddress"] = value;
        }
    }

    public string amiLogin
    {
        get
        {
            if (ViewState["AmiLogin"] != null)
                return (string)(ViewState["AmiLogin"]);
            else
                return null;
        }
        set
        {
            ViewState["AmiLogin"] = value;
        }
    }

    public string amiPswd
    {
        get
        {
            if (ViewState["amiPswd"] != null)
                return (string)(ViewState["amiPswd"]);
            else
                return null;
        }
        set
        {
            ViewState["amiPswd"] = value;
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {       
        if (!IsPostBack)
        {
            Session["loggedUser"] = Membership.GetUser().UserName.ToString();         
        }        
        GridView_userTransfer.DataSource = null;          
    }

    protected void DropDownList_from_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(DropDownList_from.SelectedIndex != 0)
        {                                           
            foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
            {
                if (asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                {                   
                    if(tcp.login(asterisk.ip_address, asterisk.login_AMI, tcp.DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        usersList = tcp.getUsersByAsterisk(asterisk.name_Asterisk);
                        GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList();
                        GridView_userTransfer.Visible = true;
                        GridView_userTransfer.DataBind();
                        GridView_userTransfer.PageIndex = 0;
                        tcp.logoff();
                    }
                    else
                    {
                        //nepodarilo sa pripojit k asterisku a zobrazit users
                    }
                }
            }                       
        }
        else
        {
            GridView_userTransfer.Visible = false;           
        }      
    }

    protected void GridView_userTransfer_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList(); ;
        GridView_userTransfer.DataBind();
        GridView_userTransfer.PageIndex = e.NewPageIndex;
        GridView_userTransfer.DataBind();
    }

    protected void Button_transfer_Click(object sender, EventArgs e)
    {
        if(GridView_userTransfer.SelectedIndex == -1 || GridView_userTransfer.Rows.Count == 0)
        {
            Label_notSelected.Visible = true;
        }
        else if(GridView_userTransfer.SelectedIndex != -1)
        {
            Label_notSelected.Visible = false;
            if (Page.IsValid)
            {               
                foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                {
                    if (asterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue))
                    {
                        if (tcp.login(asterisk.ip_address, asterisk.login_AMI, tcp.DecryptAMIPassword(asterisk.password_AMI)))
                        {
                            if(tcp.userTransfer(GridView_userTransfer.SelectedRow.Cells[1].Text, usersDetailList))
                            {
                                TransferedUser transferedUser = new TransferedUser();
                                transferedUser.transferedUser = GridView_userTransfer.SelectedRow.Cells[1].Text;
                                transferedUser.originalContext = tcp.originalContext;
                                transferedUser.originalAsterisk = DropDownList_from.SelectedValue;
                                transferedUser.currentAsterisk = DropDownList_to.SelectedValue;
                                if(asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).Count == 0)
                                {
                                    asteriskAccessLayer.insertTransferedUser(transferedUser);
                                    if(tcp.firstTransferUser(transferedUser.transferedUser, transferedUser.originalAsterisk))
                                    {
                                        tcp.logoff();
                                        if(tcp.login(fromAddress, amiLogin, amiPswd))
                                        {
                                            if (tcp.changeInOriginal(transferedUser.transferedUser, transferedUser.originalContext, transferedUser.currentAsterisk) && tcp.deleteFromOriginal(transferedUser.transferedUser))
                                            {
                                                tcp.logoff();
                                                foreach (AsteriskRoutingSystem.Asterisk otherAsterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                                                {
                                                    if (!asterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue) || asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                                                    {  
                                                        if(tcp.login(otherAsterisk.ip_address, otherAsterisk.login_AMI, tcp.DecryptAMIPassword(otherAsterisk.password_AMI)))
                                                        {
                                                            if(tcp.addToOthersDialPlan(transferedUser.transferedUser, transferedUser.currentAsterisk, transferedUser.originalAsterisk))
                                                            {
                                                                //pridanie k dialplanu ostatnych asteriskov
                                                            }
                                                            else
                                                            {
                                                                //chyba
                                                            }
                                                            tcp.logoff();
                                                        }
                                                        else
                                                        {
                                                            //chyba
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //nepodarilo sa preniest
                                            }
                                        }
                                        else
                                        {
                                            //nepodarilo sa preniest
                                        }                                    
                                    }
                                    else
                                    {
                                        //nepodarilo sa preniest uzivatela
                                    }
                                }
                                else if (asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).ElementAt(0).originalAsterisk.Equals(DropDownList_to.SelectedValue))
                                {
                                    TransferedUser tu = asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).ElementAt(0);
                                    if (tcp.returnBackToOriginal(tu.transferedUser, tu.originalContext, tu.currentAsterisk) &&  tcp.returnOriginalContext(tu.transferedUser, tu.originalContext))
                                    {
                                        tcp.logoff();
                                        if(tcp.login(fromAddress, amiLogin, amiPswd))
                                        {
                                            if (tcp.deleteTransferFromRemote(tu.transferedUser, tu.originalAsterisk) && tcp.deleteFromOriginal(tu.transferedUser))
                                            {
                                                foreach (AsteriskRoutingSystem.Asterisk otherAsterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                                                {
                                                    if (!asterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue) || asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                                                    {
                                                        if (tcp.login(otherAsterisk.ip_address, otherAsterisk.login_AMI, tcp.DecryptAMIPassword(otherAsterisk.password_AMI)))
                                                        {
                                                            if (tcp.deleteFromOthers(tu.transferedUser, tu.originalAsterisk, tu.currentAsterisk))
                                                            {
                                                                //pridanie k dialplanu ostatnych asteriskov
                                                            }
                                                            else
                                                            {
                                                                //chyba
                                                            }
                                                            tcp.logoff();
                                                        }
                                                        else
                                                        {
                                                            //chyba
                                                        }
                                                    }
                                                }
                                                asteriskAccessLayer.deleteTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text);                                                                                                                                  
                                            }
                                            else
                                            {
                                                //chyba
                                            }
                                        }
                                        else
                                        {
                                            //chyba
                                        }
                                       
                                    }
                                    else
                                    {
                                        //chyba
                                    }                                                                                
                                }
                                else
                                {
                                    asteriskAccessLayer.updateTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text, DropDownList_to.SelectedValue);
                                    //logika pri prenose 
                                }                           
                            }
                            else
                            {
                                //nepodarilo sa preniest uzivatela
                            }
                        }
                        else
                        {
                            //nepodarilo sa pripojit k asterisku a zobrazit users
                        }
                        break;
                    }
                }
            }
        }        
    }

    protected void GridView_userTransfer_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
        {
            if (asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
            {
                fromAddress = asterisk.ip_address;
                amiLogin = asterisk.login_AMI;
                amiPswd = tcp.DecryptAMIPassword(asterisk.password_AMI);
                if (tcp.login(asterisk.ip_address, asterisk.login_AMI, tcp.DecryptAMIPassword(asterisk.password_AMI)))
                {
                    usersDetailList = tcp.getUserDetail(GridView_userTransfer.SelectedRow.Cells[1].Text);
                    tcp.logoff();
                }
                else
                {
                    //nepodarilo sa pripojit k asterisku a zobrazit users
                }
            }
        }
        
    }
}