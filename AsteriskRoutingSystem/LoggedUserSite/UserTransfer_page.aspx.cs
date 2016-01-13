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
                                if (tcp.addTransferedToDialPlan(GridView_userTransfer.SelectedRow.Cells[1].Text))
                                {
                                    tcp.logoff();
                                    foreach (AsteriskRoutingSystem.Asterisk asteriskFrom in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                                    {
                                        if (asteriskFrom.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                                        {
                                            if (tcp.login(asteriskFrom.ip_address, asteriskFrom.login_AMI, tcp.DecryptAMIPassword(asteriskFrom.password_AMI)))
                                            {
                                                if (tcp.deleteTransferedFromOriginal(GridView_userTransfer.SelectedRow.Cells[1].Text))
                                                {
                                                    //uspesny transfer
                                                    //databind
                                                }
                                                tcp.logoff();
                                            }
                                            else
                                            {
                                                //nepodarilo sa pripojit k asterisku a zobrazit users
                                            }
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //nepodarilo sa vytvorit zaznam v dialplane
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