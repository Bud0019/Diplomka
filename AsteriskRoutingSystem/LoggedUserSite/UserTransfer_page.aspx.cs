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
    AMIConnector amiConnector = new AMIConnector();
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
        if (DropDownList_from.SelectedIndex != 0)
        {
            foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
            {
                if (asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                {
                    if (amiConnector.login(asterisk.ip_address, asterisk.login_AMI, amiConnector.DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        usersList = amiConnector.getUsersByAsterisk(asterisk.name_Asterisk);
                        GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList();
                        GridView_userTransfer.DataBind();
                        GridView_userTransfer.PageIndex = 0;
                        amiConnector.logoff();
                    }
                    else
                    {
                        writeToLog("<" + asterisk.name_Asterisk + ">: Načítanie užívateľa zlyhalo!\n");
                        GridView_userTransfer.SelectedIndex = -1;
                    }
                }
            }
        }
        else
        {
            GridView_userTransfer.Visible = false;
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
                amiPswd = amiConnector.DecryptAMIPassword(asterisk.password_AMI);
                if (amiConnector.login(asterisk.ip_address, asterisk.login_AMI, amiConnector.DecryptAMIPassword(asterisk.password_AMI)))
                {
                    usersDetailList = amiConnector.getUserDetail(GridView_userTransfer.SelectedRow.Cells[1].Text);
                    amiConnector.logoff();
                }
                else
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Zobrazenie užívateľov zlyhalo!\n");
                }
            }
        }
    }


    protected void GridView_userTransfer_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList(); ;
        GridView_userTransfer.DataBind();
        GridView_userTransfer.PageIndex = e.NewPageIndex;
        GridView_userTransfer.DataBind();
    }

    private AsteriskRoutingSystem.Asterisk asteriskFromDDL()
    {
        foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
        {
            if (asterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue))
            {
                return asterisk;
            }
        }
        return null;
    }

    private void writeToLog(string message)
    {
        TextBox_log.Text += message;
    }

    protected void Button_transfer_Click(object sender, EventArgs e)
    {
        TextBox_log.Text = string.Empty;
        if (GridView_userTransfer.SelectedIndex == -1 || GridView_userTransfer.Rows.Count == 0)
        {
            Label_notSelected.Visible = true;
        }
        else if (GridView_userTransfer.SelectedIndex != -1)
        {
            Label_notSelected.Visible = false;
            if (Page.IsValid)
            {
                TransferedUser user = new TransferedUser();
                if (amiConnector.login(asteriskFromDDL().ip_address, asteriskFromDDL().login_AMI, amiConnector.DecryptAMIPassword(asteriskFromDDL().password_AMI)))
                {
                    writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Pripojenie OK!\n");
                }
                else
                {
                    writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Pripojenie zlyhalo!\n");
                    return;
                }
                if (amiConnector.relocateUser(GridView_userTransfer.SelectedRow.Cells[1].Text, usersDetailList))
                {
                    user.transferedUser = GridView_userTransfer.SelectedRow.Cells[1].Text;
                    user.originalContext = amiConnector.originalContext;
                    user.originalAsterisk = DropDownList_from.SelectedValue;
                    user.currentAsterisk = DropDownList_to.SelectedValue;
                    writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Presun čísla OK!\n");
                }
                else
                {
                    writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Presun čísla zlyhal!\n");
                    return;
                }
                if (asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).Count == 0)
                {
                    asteriskAccessLayer.insertTransferedUser(user);
                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.addToTrunkContextOnOriginal, user.transferedUser,
                        user.originalAsterisk, null, null, null))
                    {
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov OK!\n");
                        amiConnector.logoff();
                    }
                    else
                    {
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                        amiConnector.logoff();
                        return;
                    }
                    if (amiConnector.login(fromAddress, amiLogin, amiPswd))
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie zlyhalo!\n");
                        return;
                    }
                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.addToOriginalContext, user.transferedUser,
                        null, user.currentAsterisk, user.originalContext, null) && amiConnector.deleteFromOriginal(user.transferedUser))
                    {
                        usersList = amiConnector.getUsersByAsterisk(asteriskFromDDL().name_Asterisk);
                        amiConnector.logoff();
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov zlyhala!\n");
                        return;
                    }
                    foreach (AsteriskRoutingSystem.Asterisk otherAsterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                    {
                        if (otherAsterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue) || otherAsterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                        {
                            continue;
                        }
                        if (amiConnector.login(otherAsterisk.ip_address, otherAsterisk.login_AMI, amiConnector.DecryptAMIPassword(otherAsterisk.password_AMI)))
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                        }
                        else
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                            return;
                        }
                        if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.addToOthersAsteriskDialPlans, user.transferedUser,
                            user.originalAsterisk, user.currentAsterisk, null, null))
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov OK!\n");
                        }
                        else
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                            return;
                        }
                        amiConnector.logoff();
                    }
                    GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList();
                    GridView_userTransfer.DataBind();
                    writeToLog("<" + DropDownList_from.SelectedValue + ">: Presun OK!\n");
                }
                else if (asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).ElementAt(0).originalAsterisk.Equals(DropDownList_to.SelectedValue))
                {
                    TransferedUser tu = asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).ElementAt(0);
                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.deleteInOriginalContext, tu.transferedUser,
                        null, tu.currentAsterisk, tu.originalContext, null) && amiConnector.returnOriginalContext(tu.transferedUser, tu.originalContext))
                    {
                        amiConnector.logoff();
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                        amiConnector.logoff();
                        return;
                    }
                    if (amiConnector.login(fromAddress, amiLogin, amiPswd))
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie zlyhalo!\n");
                        return;
                    }
                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.deleteFromSourceAsteriskDialPlan, tu.transferedUser,
                        tu.originalAsterisk, null, null, null) && amiConnector.deleteFromOriginal(tu.transferedUser))
                    {
                        usersList = amiConnector.getUsersByAsterisk(asteriskFromDDL().name_Asterisk);
                        amiConnector.logoff();
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov zlyhala!\n");
                        return;
                    }
                    foreach (AsteriskRoutingSystem.Asterisk otherAsterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                    {
                        if (otherAsterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue) || otherAsterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                        {
                            continue;
                        }
                        if (amiConnector.login(otherAsterisk.ip_address, otherAsterisk.login_AMI, amiConnector.DecryptAMIPassword(otherAsterisk.password_AMI)))
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                        }
                        else
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                            return;
                        }
                        if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.deleteFromRestAsteriskDialPlan, tu.transferedUser, tu.originalAsterisk, tu.currentAsterisk, null, null))
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov OK!\n");

                        }
                        else
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                            return;
                        }
                        amiConnector.logoff();
                    }
                    GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList();
                    GridView_userTransfer.DataBind();
                    asteriskAccessLayer.deleteTransferedUser(tu.transferedUser);
                    writeToLog("<" + DropDownList_from.SelectedValue + ">: Presun OK!\n");
                }
                else
                {
                    TransferedUser tu = asteriskAccessLayer.selectTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text).ElementAt(0);
                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.updateDialPlanInDestinationAsterisk, tu.transferedUser, tu.originalAsterisk, tu.currentAsterisk, null, null))
                    {
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov OK!\n");
                        amiConnector.logoff();
                    }
                    else
                    {
                        writeToLog("<" + asteriskFromDDL().name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                        return;
                    }
                    if (amiConnector.login(fromAddress, amiLogin, amiPswd))
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Pripojenie zlyhalo!\n");
                        return;
                    }

                    if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.updateInCurrentAsteriskDialPlan, tu.transferedUser, tu.originalAsterisk, DropDownList_to.SelectedValue, null, null) && amiConnector.deleteFromOriginal(tu.transferedUser))
                    {
                        usersList = amiConnector.getUsersByAsterisk(asteriskFromDDL().name_Asterisk);
                        amiConnector.logoff();
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + DropDownList_from.SelectedValue + ">: Zmena kontextov zlyhala!\n");
                        return;
                    }
                    foreach (AsteriskRoutingSystem.Asterisk otherAsterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
                    {
                        if (otherAsterisk.name_Asterisk.Equals(DropDownList_to.SelectedValue) || otherAsterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                        {
                            continue;
                        }
                        if (amiConnector.login(otherAsterisk.ip_address, otherAsterisk.login_AMI, amiConnector.DecryptAMIPassword(otherAsterisk.password_AMI)))
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                        }
                        else
                        {
                            writeToLog("<" + otherAsterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                            return;
                        }
                        if (otherAsterisk.name_Asterisk.Equals(tu.originalAsterisk))
                        {
                            if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.updateInOriginalAsteriskDialPlan, tu.transferedUser, null, tu.currentAsterisk, tu.originalContext, DropDownList_to.SelectedValue))
                            {
                                writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov OK!\n");
                            }
                            else
                            {
                                writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                                return;
                            }
                        }
                        else
                        {
                            if (amiConnector.sendUpdateDialPlanRequest(AMIConnector.updateDialPlanMessage.updateInRestAsteriskDialPlan, tu.transferedUser, tu.originalAsterisk, tu.currentAsterisk, null, DropDownList_to.SelectedValue))
                            {
                                writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov OK!\n");
                            }
                            else
                            {
                                writeToLog("<" + otherAsterisk.name_Asterisk + ">: Zmena kontextov zlyhala!\n");
                                return;
                            }
                        }
                    }

                    asteriskAccessLayer.updateTransferedUser(GridView_userTransfer.SelectedRow.Cells[1].Text, DropDownList_to.SelectedValue);
                    GridView_userTransfer.DataSource = usersList.Select(l => new { user_number = l }).ToList();
                    GridView_userTransfer.DataBind();
                    writeToLog("<" + DropDownList_from.SelectedValue + ">: Presun OK!\n");
                }
            }
        }
    }
}


