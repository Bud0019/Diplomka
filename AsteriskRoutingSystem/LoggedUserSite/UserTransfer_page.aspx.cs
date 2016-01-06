using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class LoggedUserSite_UserTransfer_page : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Session["loggedUser"] = Membership.GetUser().UserName.ToString();          
        }      
    }

    protected void DropDownList_from_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(DropDownList_from.SelectedIndex != 0)
        {

            GridView_userTransfer.Visible = true;
            GridView_userTransfer.DataBind();
        }
        else
        {
            GridView_userTransfer.Visible = false;
        }
    }
}