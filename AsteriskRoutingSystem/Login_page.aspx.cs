using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login_page : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Login_Authenticate(object sender, AuthenticateEventArgs e)
    {
        if (Membership.ValidateUser(Login.UserName, Login.Password))
        {
            FormsAuthentication.RedirectFromLoginPage(Login.UserName, false);
        }
    }
}