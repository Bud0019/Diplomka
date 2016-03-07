using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.IO;
using System.Text;


public partial class LoggedUserSite_UserTransfer_page : System.Web.UI.Page
{
    public List<string> userDetailList
    {
        get
        {
            if (ViewState["userDetailList"] == null)
            {
                ViewState["userDetailList"] = new List<string>();
            }
            return (List<string>)(ViewState["userDetailList"]);
        }
        set { ViewState["userDetailList"] = value; }
    }

    public List<string> usersList
    {
        get
        {
            if (ViewState["usersList"] == null)
            {
                ViewState["usersList"] = new List<string>();
            }
            return (List<string>)(ViewState["usersList"]);
        }
        set { ViewState["usersList"] = value; }
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
        Label_result.Visible = false;
        if (DropDownList_from.SelectedIndex != 0)
        {
            string errorMessage;
                usersList = TransferUserManager.TransferUserManagerInstance.loadUsersInAsterisk(DropDownList_from.SelectedValue, out errorMessage);
                GridView_userTransfer.DataSource = usersList.Select(l => new { name_user = l }).ToList();
                GridView_userTransfer.DataBind();
                GridView_userTransfer.PageIndex = 0;
                GridView_userTransfer.Visible = true;
            if (errorMessage.Length > 0)
            {               
                Label_result.Text = errorMessage;
                Label_result.Visible = true;
            }
        }
        else
        {
            GridView_userTransfer.Visible = false;
        }
    }

    protected void GridView_userTransfer_SelectedIndexChanged(object sender, EventArgs e)
    {
        string errorMessage;
        userDetailList = TransferUserManager.TransferUserManagerInstance.loadUserDetailList(GridView_userTransfer.SelectedRow.Cells[1].Text, out errorMessage);
        if(errorMessage.Length > 0) { 
            Label_result.Text = errorMessage;
            Label_result.Visible = true;
        }
    }


    protected void GridView_userTransfer_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridView_userTransfer.DataSource = usersList.Select(l => new { name_user = l }).ToList(); ;
        GridView_userTransfer.DataBind();
        GridView_userTransfer.PageIndex = e.NewPageIndex;
        GridView_userTransfer.DataBind();
    }

    protected void Button_transfer_Click(object sender, EventArgs e)
    {
        if (GridView_userTransfer.SelectedIndex == -1 || GridView_userTransfer.Rows.Count == 0)
        {
            Label_notSelected.Visible = true;
        }
        else if (GridView_userTransfer.SelectedIndex != -1)
        {
            Label_notSelected.Visible = false;
            if (Page.IsValid)
            {
                string errorMessage;
                Label_result.Text = TransferUserManager.TransferUserManagerInstance.transfer(DropDownList_from.SelectedValue, DropDownList_to.SelectedValue,
                        Membership.GetUser().UserName.ToString(), GridView_userTransfer.SelectedRow.Cells[1].Text, userDetailList);
                GridView_userTransfer.DataSource = TransferUserManager.TransferUserManagerInstance.loadUsersInAsterisk(DropDownList_from.SelectedValue, out errorMessage)
                        .Select(l => new { name_user = l }).ToList();
                GridView_userTransfer.DataBind();
                Label_result.Visible = true;
            }
        }
    }
}


