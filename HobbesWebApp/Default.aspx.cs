using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ircda.hobbes;
using System.Text;


    public partial class _Default : System.Web.UI.Page
    {

        TextBox output = null; //Hack - page needs to be recreated

        protected void Page_Load(object sender, EventArgs e)
        {
            output = (TextBox)FindControl("TextOut");
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected void SetCookiesInResponse_Click(object sender, EventArgs e)
        {
            HttpCookie cookie = CookieTools.MakeCookie(CookieTools.IRCDACookieName, "HobbesSession");
            CookieTools.AddTo(cookie, "Session", "TestValue");
            CookieTools.AddTo(cookie, CookieTools.UserID, "Tester");
            Context.Response.Cookies.Add(cookie);
            //MakeButton.PostBackUrl = "~/default.aspx";
        }

        protected void ShowCookiesInRequest_Click(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                for (int ii = 0; ii < Request.Cookies.Count; ii++)
                {
                    HttpCookie cookie = Request.Cookies[ii];
                    string userID = CookieTools.GetIrcdaCookieValue(Request.Cookies, "UserID");
                    string cookieOut = string.Format("Cookie Name: {0}, Val: {1}, Expiration: {2}, UserID: {3}",
                        cookie.Name, cookie.Value, cookie.Expires, userID);
                    StringBuilder kvout = new System.Text.StringBuilder();
                    System.Collections.Specialized.NameValueCollection cookieValues = cookie.Values;
                    
                    foreach (string valuething in cookie.Values)
                    {
                        kvout.AppendFormat("Val:{0}{1}", valuething, System.Environment.NewLine);
                    }
                    output = (TextBox)FindControl("TextOut");
                    output.Text = output.Text + System.Environment.NewLine + cookieOut + kvout.ToString();
                    
                }
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            output.Text = "";
        }
    }