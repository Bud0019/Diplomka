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


public partial class LoggedUserSite_AsterisksMnt_page : System.Web.UI.Page
{
    TCPConnector tcp = new TCPConnector();

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
    protected void Page_Load(object sender, EventArgs e)
    {      
        if (!IsPostBack)
        {
            Session["loggedUser"] = Membership.GetUser().UserName.ToString();
            GridView_Asterisks.DataBind();
        }
    }



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

    protected void Button_confirm_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {           
            TextBox_log.Text = string.Empty;            
            if(tcp.login(TextBox_ipAddress.Text, TextBox_login.Text, TextBox_password.Text))
            {               
                AsteriskRoutingSystem.Asterisk asterisk = new AsteriskRoutingSystem.Asterisk();
                asterisk.name_Asterisk = TextBox_name.Text;
                asterisk.prefix_Asterisk = TextBox_prefix.Text;
                asterisk.ip_address = TextBox_ipAddress.Text;
                asterisk.login_AMI = TextBox_login.Text;
                asterisk.password_AMI = tcp.EncryptAMIPassword(TextBox_password.Text.Trim());
                asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
                asterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
                asterisk.tls_certDestination = TextBox_certDestination.Text;

                AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
                List<AsteriskRoutingSystem.Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                List<string> asteriskNamesList = new List<string>();
                foreach (AsteriskRoutingSystem.Asterisk asteriskName in asteriskList)
                {
                    asteriskNamesList.Add(asteriskName.name_Asterisk);
                }
                int returnCode;
                if ((returnCode = asteriskAccessLayer.insertNewUniqueASterisk(asterisk)) == -1)
                {
                    if (asteriskList.Count > 0)
                    {
                        TextBox_log.Text += "Pridávam k " + asterisk.name_Asterisk + "...\n";
                        foreach (AsteriskRoutingSystem.Asterisk oneAsterisk in asteriskList)
                        {
                            if (tcp.addTrunk(oneAsterisk.name_Asterisk, oneAsterisk.ip_address, oneAsterisk.tls_enabled))
                            {
                                tcp.addPrefix(oneAsterisk.name_Asterisk, oneAsterisk.prefix_Asterisk);
                                TextBox_log.Text += "Pridanie " + oneAsterisk.name_Asterisk + " OK.\n";
                            }
                            else
                                TextBox_log.Text += "Pridanie " + oneAsterisk.name_Asterisk + " zlyhalo!\n";
                            //osetrit co v takom pripade 
                        }

                        tcp.createInitialContexts(asteriskNamesList, asterisk.tls_enabled, asterisk.tls_certDestination);
                        tcp.reloadModules();
                        tcp.logoff();
                        foreach (AsteriskRoutingSystem.Asterisk oneAsterisk in asteriskList)
                        {
                            TextBox_log.Text += "Pridávam k " + oneAsterisk.name_Asterisk + "...\n";                          
                                if (tcp.login(oneAsterisk.ip_address, oneAsterisk.login_AMI, tcp.DecryptAMIPassword(oneAsterisk.password_AMI)))
                                {                               
                                    if (tcp.addTrunk(TextBox_name.Text, TextBox_ipAddress.Text, asterisk.tls_enabled))
                                    {
                                        tcp.addPrefix(TextBox_name.Text, TextBox_prefix.Text);
                                        TextBox_log.Text += "Pridanie " + TextBox_name.Text + " OK.\n";
                                    }
                                    else
                                    {
                                        TextBox_log.Text += "Pridanie " + TextBox_name.Text + " zlyhalo!\n";
                                        //osetrit co v takom pripade 
                                    }
                                }
                                else
                                {
                                    TextBox_log.Text += "Pripojenie k Asterisku " + oneAsterisk.name_Asterisk + " zlyhalo!\n";
                                    //osetrit co v takom pripade 
                                }                                                    
                            asteriskNamesList.Add(TextBox_name.Text);
                            if (tcp.addToRemoteDialPlans(asteriskNamesList))
                            {
                                TextBox_log.Text += "Pridanie " + TextBox_name.Text + " OK.\n";
                            }
                            else
                            {
                                TextBox_log.Text += "Pridanie " + TextBox_name.Text + " zlyhalo!\n";
                                //osetrit co v takom pripade 
                            }
                            tcp.reloadModules();
                            tcp.logoff();                          
                        }
                    }
                    else
                    {
                        if (tcp.createInitialContexts(asteriskNamesList, asterisk.tls_enabled, asterisk.tls_certDestination)) { 
                            TextBox_log.Text += "Pridanie " + TextBox_name.Text + " OK.\n";                           
                        }
                        else
                        {
                            //co v takom pripade 
                            TextBox_log.Text += "Pridanie " + TextBox_name.Text + " zlyhalo.\n";
                        }
                        tcp.reloadModules();
                        tcp.logoff();
                    }
                    GridView_Asterisks.DataBind();
                }
                else if (returnCode == 1)
                {
                    TextBox_log.Text += "Asterisk " + asterisk.name_Asterisk + " existuje.\n";
                }
                else if (returnCode == 2)
                {
                    TextBox_log.Text += "Asterisk s IP:" + asterisk.ip_address + " existuje.\n";
                }
                else if (returnCode == 3)
                {
                    TextBox_log.Text += "Prefix " + asterisk.prefix_Asterisk + " existuje.\n";
                }
            }
            else
            {
                TextBox_log.Text += "Pripojenie k Asterisku " + TextBox_login.Text + " zlyhalo!\n";
            }          
        }
    }

    protected void Button_delete_Click(object sender, EventArgs e)
    {

        TextBox_log.Text = string.Empty;
        AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
        List<AsteriskRoutingSystem.Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
        List<string> asteriskNamesList = new List<string>();
        foreach (AsteriskRoutingSystem.Asterisk asteriskName in asteriskList)
        {
            asteriskNamesList.Add(asteriskName.name_Asterisk);
        }
        StringBuilder sbDeletedAsterisk = new StringBuilder();
        StringBuilder sbRemoteAsterisk = new StringBuilder();
        foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskList)
        {
            if (asterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
            {               
                    if (tcp.login(asterisk.ip_address, asterisk.login_AMI, tcp.DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        if (asteriskList.Count > 1)
                            sbDeletedAsterisk.Append("Odstraňujem z: " + asterisk.name_Asterisk + "...\n");
                        else
                            sbDeletedAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " odstránený.\n");
                        asteriskAccessLayer.deleteAsterisk(asterisk.id_Asterisk);
                        foreach (AsteriskRoutingSystem.Asterisk otherasterisk in asteriskList)
                        {
                            if (otherasterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
                            {
                                continue;
                            }
                            else
                            {                            
                                if (tcp.deleteTrunk(otherasterisk.name_Asterisk, otherasterisk.tls_enabled, otherasterisk.tls_certDestination))
                                    sbDeletedAsterisk.Append("Asterisk: " + otherasterisk.name_Asterisk + " zmazaný!\n");
                                else
                                    //to do co v takom pripade 
                                    sbDeletedAsterisk.Append("Vymazanie: " + otherasterisk.name_Asterisk + " zlyhalo!\n");
                            }
                        }
                        tcp.deleteAllRemoteContexts(asteriskNamesList);
                        tcp.reloadModules();
                        tcp.logoff();
                    }
                    else
                    {
                        //to do co v takom pripade                        
                        sbDeletedAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " je nedostupný!\n");
                    }         
            }
            else
            {                
                    if (tcp.login(asterisk.ip_address, asterisk.login_AMI, tcp.DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        sbRemoteAsterisk.Append("Odstraňujem z: " + asterisk.name_Asterisk + "...\n");
                        int isChecked = (CheckBox_TLS.Checked) ? 1 : 0;
                        if (tcp.deleteTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, isChecked, TextBox_certDestination.Text))
                        {
                            sbRemoteAsterisk.Append("Asterisk: " + GridView_Asterisks.SelectedRow.Cells[1].Text + " zmazaný!\n");
                        }
                        else
                            //to do co v takom pripade 
                            sbRemoteAsterisk.Append("Vymazanie: " + GridView_Asterisks.SelectedRow.Cells[1].Text + " zlyhalo!\n");
                        tcp.deleteOneContext(TextBox_name.Text);
                        tcp.reloadModules();
                        tcp.logoff();
                    }
                    else
                    {
                        //to do co v takom pripade                       
                        sbRemoteAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " je nedostupný!\n");
                    }               
            }
        }
        TextBox_log.Text = sbDeletedAsterisk.ToString() + sbRemoteAsterisk.ToString();
        GridView_Asterisks.DataBind();
        closeEdit();
    }

    protected void Button_edit_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            TextBox_log.Text = string.Empty;                      
                if (tcp.login(TextBox_ipAddress.Text, TextBox_login.Text, TextBox_password.Text))
                {
                    StringBuilder sbUpdatedAsterisk = new StringBuilder();
                    StringBuilder sbRemoteAsterisk = new StringBuilder();

                    AsteriskRoutingSystem.Asterisk asterisk = new AsteriskRoutingSystem.Asterisk();
                    asterisk.id_Asterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
                    asterisk.name_Asterisk = TextBox_name.Text;
                    asterisk.prefix_Asterisk = TextBox_prefix.Text;
                    asterisk.ip_address = TextBox_ipAddress.Text;
                    asterisk.login_AMI = TextBox_login.Text;
                    asterisk.password_AMI = tcp.EncryptAMIPassword(TextBox_password.Text.Trim());
                    asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
                    asterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
                    asterisk.tls_certDestination = TextBox_certDestination.Text;
                

                    AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
                    List<AsteriskRoutingSystem.Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                    int returnCode;
                    if ((returnCode = asteriskAccessLayer.updateAsterisk(asterisk)) == -1)
                    {
                    //dopisat update do globals
                    if (asteriskList.Count > 1)
                        {
                            foreach (AsteriskRoutingSystem.Asterisk oneAsterisk in asteriskList)
                            {
                                if (oneAsterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
                                    continue;
                                else
                                {
                                    TextBox_log.Text += "Upravujem v " + oneAsterisk.name_Asterisk + "...\n";
                                   
                                        if (tcp.login(oneAsterisk.ip_address, oneAsterisk.login_AMI, tcp.DecryptAMIPassword(oneAsterisk.password_AMI)))
                                        {                                       
                                            if (tcp.updateTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, asterisk.name_Asterisk, asterisk.ip_address, int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text), asterisk.tls_certDestination, asterisk.tls_enabled))
                                            {
                                                TextBox_log.Text += "Upravovanie " + TextBox_name.Text + " OK.\n";
                                            }
                                            else
                                            {
                                                TextBox_log.Text += "Upravovanie " + TextBox_name.Text + " zlyhalo!\n";
                                                //osetrit co v takom pripade 
                                            }
                                        }
                                        else
                                        {
                                            TextBox_log.Text += "Pripojenie k AMI " + oneAsterisk.name_Asterisk + " zlyhalo!\n";
                                            //osetrit co v takom pripade 
                                        }
                                   
                                    tcp.updateDialPlans(GridView_Asterisks.SelectedRow.Cells[1].Text, TextBox_name.Text, TextBox_prefix.Text);
                                    tcp.reloadModules();
                                    tcp.logoff();
                                }
                            }
                        }
                        else
                        {
                            TextBox_log.Text += "Upravenie " + TextBox_name.Text + " OK.\n";
                        }
                        GridView_Asterisks.DataBind();
                    }
                    else if (returnCode == 1)
                    {
                        TextBox_log.Text += "Asterisk " + asterisk.name_Asterisk + " existuje.\n";
                    }
                    else if (returnCode == 2)
                    {
                        TextBox_log.Text += "Asterisk s IP:" + asterisk.ip_address + " existuje.\n";
                    }
                    else if (returnCode == 3)
                    {
                        TextBox_log.Text += "Prefix " + asterisk.prefix_Asterisk + " existuje.\n";
                    }
                }
                else
                {
                    TextBox_log.Text += "Pripojenie k AMI " + TextBox_name.Text + " zlyhalo!\n";
                }                        
        }
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
}