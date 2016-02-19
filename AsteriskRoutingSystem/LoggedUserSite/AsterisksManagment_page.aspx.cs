using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AsteriskRoutingSystem;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data;
using System.Threading;

public partial class LoggedUserSite_AsterisksMnt_page : System.Web.UI.Page
{
    #region Variable
    private AMIConnector ami = new AMIConnector();
    private AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();

    public int currentTLSStatus
    {
        get
        {
            if (ViewState["currentTLSStatus"] != null)
                return (int)(ViewState["currentTLSStatus"]);
            else
                return 0;
        }
        set
        {
            ViewState["currentTLSStatus"] = value;
        }
    }

    public string passwordForRollback
    {
        get
        {
            if (ViewState["passwordForRollback"] != null)
                return (string)(ViewState["passwordForRollback"]);
            else
                return null;
        }
        set
        {
            ViewState["passwordForRollback"] = value;
        }
    }

    public string certDestinationForRollback
    {
        get
        {
            if (ViewState["certDestinationForRollback"] != null)
                return (string)(ViewState["certDestinationForRollback"]);
            else
                return null;
        }
        set
        {
            ViewState["certDestinationForRollback"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {

            Session["loggedUser"] = Membership.GetUser().UserName.ToString();
            GridView_Asterisks.DataBind();
        }

    }
    #endregion
    #region helpFunction
    protected void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        Button_edit.Visible = true;
        Button_cancel.Visible = true;
        Button_delete.Visible = true;
        Label_addAsterisk.Text = "Upraviť Asterisk";
        GridViewRow row = GridView_Asterisks.SelectedRow;
        TextBox_name.Text = row.Cells[1].Text;
        TextBox_prefix.Text = row.Cells[2].Text;
        TextBox_ipAddress.Text = row.Cells[3].Text;
        TextBox_login.Text = row.Cells[4].Text;
        TextBox_password.Text = "";
        currentTLSStatus = int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text);
        CheckBox_TLS.Checked = (currentTLSStatus == 1) ? true : false;
        TextBox_certDestination.Text = GridView_Asterisks.SelectedDataKey["tls_certDestination"].ToString();
    }

    protected void Button_cancel_Click(object sender, EventArgs e)
    {
        closeEdit();
    }

    protected void button_clear_Click(object sender, EventArgs e)
    {
        TextBox_log.Text = string.Empty;
    }

    private List<string> getAsteriskList(List<AsteriskRoutingSystem.Asterisk> asteriskList)
    {
        List<string> asteriskNamesList = new List<string>();
        foreach (AsteriskRoutingSystem.Asterisk asteriskName in asteriskList)
        {
            asteriskNamesList.Add(asteriskName.name_Asterisk);
        }
        return asteriskNamesList;
    }

    private void writeToLog(string message)
    {
        TextBox_log.Text += message;
    }

    private AsteriskRoutingSystem.Asterisk findBackUpAsterisk(int selectedID, List<AsteriskRoutingSystem.Asterisk> asteriskList)
    {
        foreach (AsteriskRoutingSystem.Asterisk tmpAsterisk in asteriskList)
        {
            if (tmpAsterisk.id_Asterisk == selectedID)
            {
                return tmpAsterisk;              
            }
        }
        return null;
    }

    private void rollbackOfAdd(List<AsteriskRoutingSystem.Asterisk> rollbackListOfAdd, List<AsteriskRoutingSystem.Asterisk> asteriskList, List<string> asteriskNameList)
    {
        foreach(AsteriskRoutingSystem.Asterisk asterisk in rollbackListOfAdd)
        {
            ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI));
            ami.deleteTrunk(TextBox_name.Text);
            ami.deleteOneContext(TextBox_name.Text);
            ami.reloadModules();
            ami.logoff();
        }
        ami.login(TextBox_ipAddress.Text, TextBox_login.Text, ami.DecryptAMIPassword(TextBox_password.Text));
        ami.deleteTrunk(asteriskList, -1, CheckBox_TLS.Checked, TextBox_certDestination.Text);
        ami.deleteContexts(asteriskNameList);
        ami.deleteAllRemoteContexts(asteriskNameList);
        ami.reloadModules();
        ami.logoff();
    }

    private void rollbackOfDelete(List<AsteriskRoutingSystem.Asterisk> rollbackListOfDelete, List<AsteriskRoutingSystem.Asterisk> asteriskList, List<string> asteriskNameList)
    {
        foreach (AsteriskRoutingSystem.Asterisk asterisk in rollbackListOfDelete)
        {
            ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI));
            ami.addTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, GridView_Asterisks.SelectedRow.Cells[3].Text,
                        int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text));
            ami.addPrefix(GridView_Asterisks.SelectedRow.Cells[1].Text, GridView_Asterisks.SelectedRow.Cells[2].Text, true);
            ami.reloadModules();
            ami.logoff();
        }
        ami.login(TextBox_ipAddress.Text, TextBox_login.Text, passwordForRollback);
        ami.addTrunk(asteriskList, int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()));
        ami.addPrefix(asteriskList, int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()));
        ami.createInitialContexts(asteriskNameList, int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text), certDestinationForRollback);
        ami.reloadModules();
        ami.logoff();
    }

    private void rollbackOfUpdate(List<AsteriskRoutingSystem.Asterisk> rollbackListOfUpdate, AsteriskRoutingSystem.Asterisk backupAsterisk, List<AsteriskRoutingSystem.Asterisk> asteriskList)
    {
        foreach (AsteriskRoutingSystem.Asterisk asterisk in rollbackListOfUpdate)
        {
            ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI));
            ami.updateTrunk(asterisk.name_Asterisk, backupAsterisk.name_Asterisk, backupAsterisk.ip_address);
            ami.updateDialPlans(asterisk.name_Asterisk, backupAsterisk.name_Asterisk, backupAsterisk.prefix_Asterisk);
            ami.logoff();
        }
        ami.login(TextBox_ipAddress.Text, TextBox_login.Text, ami.DecryptAMIPassword(TextBox_password.Text));
        ami.updateTLS(backupAsterisk.tls_enabled, backupAsterisk.tls_certDestination, int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text), false, asteriskList, TextBox_name.Text);
        ami.logoff();
    }

    private void closeEdit()
    {
        GridView_Asterisks.SelectedIndex = -1;
        Button_cancel.Visible = false;
        Button_delete.Visible = false;
        Button_edit.Visible = false;
        Label_addAsterisk.Text = "Pridať Asterisk";
        TextBox_name.Text = "";
        TextBox_prefix.Text = "";
        TextBox_ipAddress.Text = "";
        TextBox_login.Text = "";
        TextBox_password.Text = "";
    }
    #endregion

    #region buttonFunction
    protected void Button_confirm_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            List<AsteriskRoutingSystem.Asterisk> asteriskList;
            List<string> asteriskNamesList;
            AsteriskRoutingSystem.Asterisk asterisk = new AsteriskRoutingSystem.Asterisk();
            TextBox_log.Text = string.Empty;

            if (ami.login(TextBox_ipAddress.Text, TextBox_login.Text, TextBox_password.Text))
            {
                asterisk.name_Asterisk = TextBox_name.Text;
                asterisk.prefix_Asterisk = TextBox_prefix.Text;
                asterisk.ip_address = TextBox_ipAddress.Text;
                asterisk.login_AMI = TextBox_login.Text;
                asterisk.password_AMI = ami.EncryptAMIPassword(TextBox_password.Text.Trim());
                asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
                asterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
                asterisk.tls_certDestination = TextBox_certDestination.Text;

                asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                asteriskNamesList = getAsteriskList(asteriskList);

                writeToLog("<" + TextBox_name.Text + ">: Pripojenie OK!\n");
            }
            else
            {
                writeToLog("<" + TextBox_name.Text + ">: Pripojenie zlyhalo!\n");
                return;
            }


            int returnCode;
            if ((returnCode = asteriskAccessLayer.insertNewUniqueASterisk(asterisk)) == -1)
            {
                writeToLog("<" + TextBox_name.Text + ">: Zápis do DB OK!\n");
            }
            else if (returnCode == 1)
            {
                writeToLog("<" + TextBox_name.Text + ">: Názov je zadaný!\n");
                return;
            }
            else if (returnCode == 2)
            {
                writeToLog("<" + TextBox_name.Text + ">: IP adresa je zadaná!\n");
                return;
            }
            else if (returnCode == 3)
            {
                writeToLog("<" + TextBox_name.Text + ">: Prefix je zadaný!\n");
                return;
            }

            if (asteriskList.Count > 0)
            {
                if (ami.addTrunk(asteriskList, -1))
                {
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie trunkov OK!\n");
                }
                else
                {
                    asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie trunk zlyhalo!\n");
                    return;
                }

                if (ami.addPrefix(asteriskList, -1))
                {
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie prefixu OK!\n");
                }
                else
                {
                    ami.deleteTrunk(asteriskList, -1, false, null);
                    asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie prefixu zlyhalo!\n");
                    return;
                }

                if (ami.createInitialContexts(asteriskNamesList, asterisk.tls_enabled, asterisk.tls_certDestination))
                {
                    writeToLog("<" + TextBox_name.Text + ">: Vytvorenie kontextov OK!\n");
                }
                else
                {
                    ami.deleteTrunk(asteriskList, -1, CheckBox_TLS.Checked, TextBox_certDestination.Text);
                    ami.deleteContexts(asteriskNamesList);                   
                    asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                    writeToLog("<" + TextBox_name.Text + ">: Vytvorenie kontextov zlyhalo!\n");
                    return;
                }
                ami.reloadModules();
                ami.logoff();
                List<AsteriskRoutingSystem.Asterisk> asteriskListForRollback = new List<AsteriskRoutingSystem.Asterisk>();

                foreach (AsteriskRoutingSystem.Asterisk oneAsterisk in asteriskList)
                {
                    if (ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n\n");
                        rollbackOfAdd(asteriskListForRollback, asteriskList, asteriskNamesList);
                        asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                        break;                       
                    }

                    if (ami.addTrunk(TextBox_name.Text, TextBox_ipAddress.Text, asterisk.tls_enabled))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie trunku OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie trunku zlyhalo!\n");                      
                        rollbackOfAdd(asteriskListForRollback, asteriskList, asteriskNamesList);
                        ami.logoff();
                        asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                        break;
                    }
                    if (ami.addPrefix(TextBox_name.Text, TextBox_prefix.Text, false))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie prefixu OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie prefixu zlyhalo!\n");
                        ami.deleteTrunk(TextBox_name.Text);                        
                        rollbackOfAdd(asteriskListForRollback, asteriskList, asteriskNamesList);
                        ami.logoff();
                        asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                        break;
                    }
                    asteriskNamesList.Add(TextBox_name.Text);
                    if (ami.addToRemoteDialPlans(asteriskNamesList))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie do dialplanu OK!\n");
                    }
                    else
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie do dialplanu zlyhalo!\n");                        
                        ami.deleteOneContext(TextBox_name.Text);
                        ami.deleteTrunk(TextBox_name.Text);
                        rollbackOfAdd(asteriskListForRollback, asteriskList, asteriskNamesList);
                        ami.logoff();
                        asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                        break;
                    }                    
                    ami.reloadModules();
                    ami.logoff();
                    asteriskListForRollback.Add(oneAsterisk);
                }
            }
            else
            {
                if (ami.createInitialContexts(asteriskNamesList, asterisk.tls_enabled, asterisk.tls_certDestination))
                {
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie OK!\n");
                }
                else
                {
                    asteriskAccessLayer.deleteAsteriskByName(TextBox_name.Text);
                    writeToLog("<" + TextBox_name.Text + ">: Pridanie zlyhalo!\n");
                    return;
                }
                ami.reloadModules();
                ami.logoff();
            }
            GridView_Asterisks.DataBind();
        }
    }

    protected void Button_delete_Click(object sender, EventArgs e)
    {

        TextBox_log.Text = string.Empty;
        AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
        List<AsteriskRoutingSystem.Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
        List<string> asteriskNamesList = getAsteriskList(asteriskList);
        bool fault = false;
        List<AsteriskRoutingSystem.Asterisk> asteriskListForRollback = new List<AsteriskRoutingSystem.Asterisk>();

        foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskList)
        {
            if (asterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
            {
                passwordForRollback = ami.DecryptAMIPassword(asterisk.password_AMI);
                certDestinationForRollback = asterisk.tls_certDestination;
                if (ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI)))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie OK!\n");
                }
                else
                {
                    fault = true;                                       
                    writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                    break;
                }

                if (ami.deleteTrunk(asteriskList, int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()), true, asterisk.tls_certDestination))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Zmazanie trunkov OK!\n");
                }
                else
                {
                    fault = true;
                    writeToLog("<" + asterisk.name_Asterisk + ">: Zmazanie trunkov zlyhalo!\n");
                    break;
                }
                if (ami.deleteAllRemoteContexts(asteriskNamesList))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Zmazanie kontextov OK!\n");
                }
                else
                {
                    fault = true;                   
                    writeToLog("<" + asterisk.name_Asterisk + ">: Zmazanie kontextov zlyhalo!\n");
                    ami.addTrunk(asteriskList, int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()));
                    break;
                }
                ami.reloadModules();
                ami.logoff();
            }
            else
            {
                if (ami.login(asterisk.ip_address, asterisk.login_AMI, ami.DecryptAMIPassword(asterisk.password_AMI)))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie OK!\n");
                }
                else
                {
                    fault = true;                      
                    writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                    rollbackOfDelete(asteriskListForRollback, asteriskList, asteriskNamesList);
                    break;
                }

                if (ami.deleteTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Vymazanie trunk OK!\n");
                }
                else
                {
                    fault = true; 
                    writeToLog("<" + asterisk.name_Asterisk + ">: Vymazanie trunk zlyhalo!\n");
                    rollbackOfDelete(asteriskListForRollback, asteriskList, asteriskNamesList);
                    break;
                }

                if (ami.deleteOneContext(TextBox_name.Text))
                {
                    writeToLog("<" + asterisk.name_Asterisk + ">: Vymazanie kontextu OK!\n");
                }
                else
                {
                    fault = true;
                    writeToLog("<" + asterisk.name_Asterisk + ">: Vymazanie kontextu zlyhalo!\n");
                    ami.addTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, GridView_Asterisks.SelectedRow.Cells[3].Text, 
                        int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text));
                    rollbackOfDelete(asteriskListForRollback, asteriskList, asteriskNamesList);
                    break;
                }
                ami.reloadModules();
                ami.logoff();
                asteriskListForRollback.Add(asterisk);
            }
        }
        if (!fault)
            asteriskAccessLayer.deleteAsterisk(int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()));
        GridView_Asterisks.DataBind();
        closeEdit();
    }

    protected void Button_edit_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            TextBox_log.Text = string.Empty;
            AsteriskRoutingSystem.Asterisk asterisk = new AsteriskRoutingSystem.Asterisk();
            AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
            List<AsteriskRoutingSystem.Asterisk> asteriskList;
            List<AsteriskRoutingSystem.Asterisk> asteriskListForRollback = new List<AsteriskRoutingSystem.Asterisk>();

            if (ami.login(TextBox_ipAddress.Text, TextBox_login.Text, TextBox_password.Text))
            {
                asterisk.id_Asterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
                asterisk.name_Asterisk = TextBox_name.Text;
                asterisk.prefix_Asterisk = TextBox_prefix.Text;
                asterisk.ip_address = TextBox_ipAddress.Text;
                asterisk.login_AMI = TextBox_login.Text;
                asterisk.password_AMI = ami.EncryptAMIPassword(TextBox_password.Text.Trim());
                asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
                asterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
                asterisk.tls_certDestination = TextBox_certDestination.Text;

                asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie OK!\n");
            }
            else
            {                   
                writeToLog("<" + asterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                return;
            }
           
            AsteriskRoutingSystem.Asterisk backupAsterisk = findBackUpAsterisk(asterisk.id_Asterisk, asteriskList);
          
            int returnCode;
            if ((returnCode = asteriskAccessLayer.updateAsterisk(asterisk)) == -1)
            {
                writeToLog("<" + asterisk.name_Asterisk + ">: Pridanie do DB OK!\n");
            }
            else if (returnCode == 1)
            {
                writeToLog("<" + asterisk.name_Asterisk + ">: Nazov je zadany!\n");
                return;
            }
            else if (returnCode == 2)
            {
                writeToLog("<" + asterisk.name_Asterisk + ">: IP adresa je zadana!\n");
                return;
            }
            else if (returnCode == 3)
            {
                writeToLog("<" + asterisk.name_Asterisk + ">: Prefix je zadany!\n");
                return;
            }

            if (ami.updateTLS(asterisk.tls_enabled, asterisk.tls_certDestination, backupAsterisk.tls_enabled, false, asteriskList, asterisk.name_Asterisk))
            {
                writeToLog("<" + asterisk.name_Asterisk + ">: Zmena TLS OK!\n");
                ami.logoff();
            }
            else
            {             
                asteriskAccessLayer.updateAsterisk(backupAsterisk);
                writeToLog("<" + asterisk.name_Asterisk + ">: Zmena TLS zlyhala!\n");
                ami.logoff();
                return;
            }
            foreach (AsteriskRoutingSystem.Asterisk oneAsterisk in asteriskList)
            {
                if (oneAsterisk.id_Asterisk != int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
                {
                    if (ami.login(oneAsterisk.ip_address, oneAsterisk.login_AMI, ami.DecryptAMIPassword(oneAsterisk.password_AMI)))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                    }
                    else
                    {
                        rollbackOfUpdate(asteriskListForRollback, backupAsterisk, asteriskList);
                        asteriskAccessLayer.updateAsterisk(backupAsterisk);
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pripojenie zlyhalo!\n");
                        break;
                    }
                    if (ami.updateTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, asterisk.name_Asterisk, asterisk.ip_address))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie trunku OK!\n");
                    }
                    else
                    {
                        rollbackOfUpdate(asteriskListForRollback, backupAsterisk, asteriskList);
                        asteriskAccessLayer.updateAsterisk(backupAsterisk);
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pridanie trunku zlyhalo!\n");
                        break;
                    }
                    if(ami.updateDialPlans(GridView_Asterisks.SelectedRow.Cells[1].Text, TextBox_name.Text, TextBox_prefix.Text))
                    {
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Zmena dialplanu OK!\n");
                    }
                    else
                    {
                        ami.updateTrunk(asterisk.name_Asterisk, backupAsterisk.name_Asterisk, backupAsterisk.ip_address);
                        rollbackOfUpdate(asteriskListForRollback, backupAsterisk, asteriskList);
                        asteriskAccessLayer.updateAsterisk(backupAsterisk);
                        writeToLog("<" + oneAsterisk.name_Asterisk + ">: Pripojenie OK!\n");
                        break;
                    }
                    asteriskListForRollback.Add(oneAsterisk);
                    ami.reloadModules();
                    ami.logoff();                    
                }
            }
            GridView_Asterisks.DataBind();
        }
    }
    #endregion
}