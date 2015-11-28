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
    }

    protected void Button_cancel_Click(object sender, EventArgs e)
    {
        closeEdit();
    }

    protected void Button_confirm_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            TextBox_log.Text = string.Empty;
            TCPConnector tcp = new TCPConnector();
            if (tcp.connect(TextBox_ipAddress.Text))
            {
                if (tcp.login(TextBox_login.Text, TextBox_password.Text))
                {
                    Asterisk asterisk = new Asterisk();
                    asterisk.name_Asterisk = TextBox_name.Text;
                    asterisk.prefix_Asterisk = TextBox_prefix.Text;
                    asterisk.ip_address = TextBox_ipAddress.Text;
                    asterisk.login_AMI = TextBox_login.Text;
                    asterisk.password_AMI = EncryptAMIPassword(TextBox_password.Text.Trim());
                    asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
                   
                    AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
                    List<Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                    int returnCode;
                    if ((returnCode = asteriskAccessLayer.insertNewUniqueASterisk(asterisk)) == -1)
                    {                       
                        if (asteriskList.Count > 0)
                        {
                            TextBox_log.Text += "Pridávam k " + asterisk.name_Asterisk + "...\n";
                            foreach (Asterisk oneAsterisk in asteriskList)
                            {
                                if (tcp.addTrunk(oneAsterisk.name_Asterisk, oneAsterisk.ip_address))
                                    TextBox_log.Text += "Pridanie " + oneAsterisk.name_Asterisk + " OK.\n";
                                else
                                    TextBox_log.Text += "Pridanie " + oneAsterisk.name_Asterisk + " zlyhalo!\n";
                                //osetrit co v takom pripade 
                            }
                            tcp.reloadModules();
                           
                            tcp.logout();
                            tcp.disconnect();
                            foreach (Asterisk oneAsterisk in asteriskList)
                            {
                                TextBox_log.Text += "Pridávam k " + oneAsterisk.name_Asterisk + "...\n";
                                if (tcp.connect(oneAsterisk.ip_address))
                                {
                                    if (tcp.login(oneAsterisk.login_AMI, DecryptAMIPassword(oneAsterisk.password_AMI)))
                                    {
                                        if (tcp.addTrunk(TextBox_name.Text, TextBox_ipAddress.Text))
                                        {
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
                                        TextBox_log.Text += "Pripojenie k AMI " + oneAsterisk.name_Asterisk + " zlyhalo!\n";
                                        //osetrit co v takom pripade 
                                    }
                                }
                                else
                                {
                                    TextBox_log.Text += "Asterisk" + oneAsterisk.name_Asterisk + " je nedostupný!\n";
                                    //osetrit co v takom pripade 
                                }                               
                                tcp.reloadModules();
                                tcp.logout();
                                tcp.disconnect();
                            }
                        }
                        else
                        {
                            tcp.getDialPlanContexts();
                            TextBox_log.Text += "Pridanie " + TextBox_name.Text + " OK.\n";
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
                tcp.disconnect();
            }
            else
            {
                TextBox_log.Text += "Pripojenie na socket " + TextBox_name.Text + " zlyhalo!\n";
            }
        }
    }

    protected void Button_delete_Click(object sender, EventArgs e)
    {
        TextBox_log.Text = string.Empty;
        TCPConnector tcp = new TCPConnector();
        AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
        List<Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
        StringBuilder sbDeletedAsterisk = new StringBuilder();
        StringBuilder sbRemoteAsterisk = new StringBuilder();
        foreach (Asterisk asterisk in asteriskList)
        {
            if (asterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
            {
                if (tcp.connect(asterisk.ip_address))
                {
                    if (tcp.login(asterisk.login_AMI, DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        if (asteriskList.Count > 1)
                            sbDeletedAsterisk.Append("Odstraňujem z: " + asterisk.name_Asterisk + "...\n");
                        else
                            sbDeletedAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " odstránený.\n");
                        asteriskAccessLayer.deleteAsterisk(asterisk.id_Asterisk);
                        foreach (Asterisk otherasterisk in asteriskList)
                        {
                            if (otherasterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
                            {
                                continue;
                            }
                            else
                            {
                                if (tcp.deleteTrunk(otherasterisk.name_Asterisk))
                                    sbDeletedAsterisk.Append("Asterisk: " + otherasterisk.name_Asterisk + " zmazaný!\n");
                                else
                                    //to do co v takom pripade 
                                    sbDeletedAsterisk.Append("Vymazanie: " + otherasterisk.name_Asterisk + " zlyhalo!\n");
                            }
                        }
                        tcp.reloadModules();
                        tcp.logout();
                        tcp.disconnect();
                    }
                    else
                    {
                        //to do co v takom pripade
                        tcp.disconnect();
                        sbDeletedAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " je nedostupný!\n");
                    }
                }
                else
                {
                    //to do co v takom pripade
                    sbDeletedAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " je nedostupný!\n");
                }
            }
            else
            {
                if (tcp.connect(asterisk.ip_address))
                {
                    if (tcp.login(asterisk.login_AMI, DecryptAMIPassword(asterisk.password_AMI)))
                    {
                        sbRemoteAsterisk.Append("Odstraňujem z: " + asterisk.name_Asterisk + "...\n");
                        if (tcp.deleteTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text))
                        {
                            sbRemoteAsterisk.Append("Asterisk: " + GridView_Asterisks.SelectedRow.Cells[1].Text + " zmazaný!\n");
                        }
                        else
                            //to do co v takom pripade 
                            sbRemoteAsterisk.Append("Vymazanie: " + GridView_Asterisks.SelectedRow.Cells[1].Text + " zlyhalo!\n");
                        tcp.reloadModules();
                        tcp.logout();
                        tcp.disconnect();
                    }
                    else
                    {
                        //to do co v takom pripade
                        tcp.disconnect();
                        sbRemoteAsterisk.Append("Asterisk: " + asterisk.name_Asterisk + " je nedostupný!\n");
                    }
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

    protected void button_clear_Click(object sender, EventArgs e)
    {
        TextBox_log.Text = string.Empty;
    }

    protected void Button_edit_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            TextBox_log.Text = string.Empty;
            TCPConnector tcp = new TCPConnector();
            if (tcp.connect(TextBox_ipAddress.Text))
            {
                if (tcp.login(TextBox_login.Text, TextBox_password.Text))
                {
                    StringBuilder sbUpdatedAsterisk = new StringBuilder();
                    StringBuilder sbRemoteAsterisk = new StringBuilder();

                    Asterisk asterisk = new Asterisk();
                    asterisk.id_Asterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
                    asterisk.name_Asterisk = TextBox_name.Text;
                    asterisk.prefix_Asterisk = TextBox_prefix.Text;
                    asterisk.ip_address = TextBox_ipAddress.Text;
                    asterisk.login_AMI = TextBox_login.Text;
                    asterisk.password_AMI = EncryptAMIPassword(TextBox_password.Text.Trim());
                    asterisk.asterisk_owner = Membership.GetUser().UserName.ToString();

                    AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();
                    List<Asterisk> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
                    int returnCode;
                    if ((returnCode = asteriskAccessLayer.updateAsterisk(asterisk)) == -1)
                    {
                        if (asteriskList.Count > 1)
                        {
                            foreach (Asterisk oneAsterisk in asteriskList)
                            {
                                if (oneAsterisk.id_Asterisk == int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString()))
                                    continue;
                                else
                                {
                                    TextBox_log.Text += "Upravujem v " + oneAsterisk.name_Asterisk + "...\n";
                                    if (tcp.connect(oneAsterisk.ip_address))
                                    {
                                        if (tcp.login(oneAsterisk.login_AMI, DecryptAMIPassword(oneAsterisk.password_AMI)))
                                        {
                                            if (tcp.updateTrunk(GridView_Asterisks.SelectedRow.Cells[1].Text, asterisk.name_Asterisk, asterisk.ip_address))
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
                                    }
                                    else
                                    {
                                        TextBox_log.Text += "Asterisk" + oneAsterisk.name_Asterisk + " je nedostupný!\n";
                                        //osetrit co v takom pripade 
                                    }
                                    tcp.reloadModules();
                                    tcp.logout();
                                    tcp.disconnect();
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
                tcp.disconnect();
            }
            else
            {
                TextBox_log.Text += "Pripojenie na socket " + TextBox_name.Text + " zlyhalo!\n";
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

    private string EncryptAMIPassword(string clearText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }

    private string DecryptAMIPassword(string cipherText)
    {
        string EncryptionKey = "MAKV2SPBNI99212";
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }
}