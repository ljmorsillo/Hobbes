using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Configuration;
using System.Xml.Linq;
using System.IO;
using ircda.hobbes;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Configuration webConfig = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
        ConfigurationSection cs = webConfig.GetSection("system.web");
        if (cs != null)
        {
            XDocument xml = XDocument.Load(new StringReader(cs.SectionInformation.GetRawXml()));
            XElement element = xml.Element("authentication");
        }
    }
    protected string DumpCookies()
    {
        Request.Cookies.Add(CookieTools.MakeCookie(CookieTools.IRCDACookieName, "TestCookie"));
        System.Text.StringBuilder output = new System.Text.StringBuilder("DumpCookies: ") ;
        foreach (string key in Request.Cookies.AllKeys)
        {
            output.AppendFormat("{0}:{1}, ", key, Request.Cookies[key].Value);
        }
        if (CookieTools.HasCookie(Request.Cookies))
        {
            output.AppendFormat(" - Found IRCDA Cookie\n");
        }
        return output.ToString();
    }
}