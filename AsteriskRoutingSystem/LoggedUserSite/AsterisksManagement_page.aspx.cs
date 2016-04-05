using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data;
using System.Threading;


public partial class LoggedUserSite_AsterisksMnt_page : System.Web.UI.Page
{
    #region Variable

    #endregion
    #region helpFunctions
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Label_deletePermanently.Visible = false;
            Button_confirmDelete.Visible = false;
            Button_denyDelete.Visible = false;
            response_label.Visible = false;
            Session["loggedUser"] = Membership.GetUser().UserName.ToString();
            TrunkManager.trunkManagerInstance.addAsteriskErrorEvent += new TrunkManager.addAsteriskErrorHandler(RollbackManager.rollbackManagerInstance.rollbackAddAsterisk);
            TrunkManager.trunkManagerInstance.updateAsteriskErrorEvent += new TrunkManager.updateAsteriskErrorHandler(RollbackManager.rollbackManagerInstance.rollbackUpdateAsterisk);
            TrunkManager.trunkManagerInstance.deleteAsteriskErrorEvent += new TrunkManager.deleteAsteriskErrorHandler(RollbackManager.rollbackManagerInstance.rollbackDeleteAsterisk);
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
        int currentTLSStatus = int.Parse(GridView_Asterisks.SelectedRow.Cells[5].Text);
        CheckBox_TLS.Checked = (currentTLSStatus == 1) ? true : false;
        TextBox_certDestination.Text = GridView_Asterisks.SelectedDataKey["tls_certDestination"].ToString();
        Label_deletePermanently.Visible = false;
        Button_confirmDelete.Visible = false;
        Button_denyDelete.Visible = false;
        response_label.Visible = false;
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

    #region buttonFunctions
    protected void Button_confirm_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            response_label.Text = string.Empty;
            response_label.ForeColor = System.Drawing.Color.Green;

            Asterisks createdAsterisk = new Asterisks();
            createdAsterisk.name_Asterisk = TextBox_name.Text;
            createdAsterisk.prefix_Asterisk = TextBox_prefix.Text;
            createdAsterisk.ip_address = TextBox_ipAddress.Text;
            createdAsterisk.login_AMI = TextBox_login.Text;
            createdAsterisk.password_AMI = Utils.EncryptAMIPassword(TextBox_password.Text.Trim());
            createdAsterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
            createdAsterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
            createdAsterisk.tls_certDestination = TextBox_certDestination.Text;

            response_label.Text = TrunkManager.trunkManagerInstance.createTrunk(createdAsterisk);
            if (response_label.Text.StartsWith("Nastala") || response_label.Text.StartsWith("Čas"))
            {
                response_label.ForeColor = System.Drawing.Color.Red;
            }
            response_label.Visible = true;
            GridView_Asterisks.DataBind();
        }
    }

    protected void Button_delete_Click(object sender, EventArgs e)
    {
        response_label.Text = string.Empty;
        response_label.ForeColor = System.Drawing.Color.Green;

        int idAsterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
        int selectedRow = GridView_Asterisks.SelectedIndex;
        response_label.Text = TrunkManager.trunkManagerInstance.deleteTrunk(idAsterisk, false);
        if (response_label.Text.StartsWith("Nastala") || response_label.Text.StartsWith("Čas"))
        {
            response_label.ForeColor = System.Drawing.Color.Red;
            Label_deletePermanently.Visible = true;
            Button_confirmDelete.Visible = true;
            Button_denyDelete.Visible = true;
        }
        response_label.Visible = true;
        GridView_Asterisks.DataBind();
        closeEdit();
        GridView_Asterisks.SelectedIndex = selectedRow;
    }

    protected void Button_edit_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            response_label.Text = string.Empty;
            response_label.ForeColor = System.Drawing.Color.Green;

            Asterisks updatedAsterisk = new Asterisks();
            updatedAsterisk.id_Asterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
            updatedAsterisk.name_Asterisk = TextBox_name.Text;
            updatedAsterisk.prefix_Asterisk = TextBox_prefix.Text;
            updatedAsterisk.ip_address = TextBox_ipAddress.Text;
            updatedAsterisk.login_AMI = TextBox_login.Text;
            updatedAsterisk.password_AMI = Utils.EncryptAMIPassword(TextBox_password.Text.Trim());
            updatedAsterisk.asterisk_owner = Membership.GetUser().UserName.ToString();
            updatedAsterisk.tls_enabled = (CheckBox_TLS.Checked) ? 1 : 0;
            updatedAsterisk.tls_certDestination = TextBox_certDestination.Text;

            response_label.Text = TrunkManager.trunkManagerInstance.updateTrunk(updatedAsterisk);
            if (response_label.Text.StartsWith("Nastala") || response_label.Text.StartsWith("Čas"))
            {
                response_label.ForeColor = System.Drawing.Color.Red;
            }
            response_label.Visible = true;
            GridView_Asterisks.DataBind();
        }
    }

    protected void Button_cancel_Click(object sender, EventArgs e)
    {
        closeEdit();
    }

    protected void Button_denyDelete_Click(object sender, EventArgs e)
    {
        Label_deletePermanently.Visible = false;
        Button_confirmDelete.Visible = false;
        Button_denyDelete.Visible = false;
    }

    protected void Button_confirmDelete_Click(object sender, EventArgs e)
    {
        int idAsterisk = int.Parse(GridView_Asterisks.DataKeys[GridView_Asterisks.SelectedIndex]["id_Asterisk"].ToString());
        TrunkManager.trunkManagerInstance.deleteTrunk(idAsterisk, true);
        response_label.Text = "Asterisk Zmazaný!";
        GridView_Asterisks.DataBind();
        Label_deletePermanently.Visible = false;
        Button_confirmDelete.Visible = false;
        Button_denyDelete.Visible = false;
    }
    #endregion
}
