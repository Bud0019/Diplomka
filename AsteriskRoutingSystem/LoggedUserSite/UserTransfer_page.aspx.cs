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
            AsteriskAccessLayer asteriskAccessLayer = new AsteriskAccessLayer();                    
            foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString()))
            {
                if (asterisk.name_Asterisk.Equals(DropDownList_from.SelectedValue))
                {                   
                    if(tcp.login(asterisk.ip_address, asterisk.login_AMI, DecryptAMIPassword(asterisk.password_AMI)))
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

            }
        }        
    }   
}