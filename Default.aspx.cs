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
}