using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ircda.hobbes;

public partial class TestLogin : System.Web.UI.Page
{
    TextBox userTb = null;

    protected void Page_Load(object sender, EventArgs e)
    {
       userTb = FindControl("UserTB") as TextBox;
       SSOConfidence result = null;
       string userval = CookieTools.GetIrcdaCookieValue(Request.Cookies, CookieTools.UserID);
        
        result = ContextDriver.CheckConfidences(this.Context);
        //check result
        if (result.SimpleValue >= 50)
        {
            userTb = FindControl("UserTB") as TextBox;
            userTb.Text = userval;
        }
        //If we have partial confidence
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        ((Button)sender).Text = "Changed";
    }
}