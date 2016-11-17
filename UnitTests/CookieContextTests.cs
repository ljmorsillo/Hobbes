using Microsoft.VisualStudio.TestTools.UnitTesting;
using ircda.hobbes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ircda.hobbes.Tests
{
    [TestClass()]
    public class CookieContextTests
    {
        private HttpRequest request = new HttpRequest("", "http://localhost/", "");
        private HttpResponse response = new HttpResponse(null);
        [TestMethod()]
        public void CheckRequestTest()
        {
            request.Cookies.Add(CookieTools.MakeCookie(CookieTools.IRCDACookieName, "testValue"));
            HttpContext testContext = new HttpContext(request, response);

            IContextChecker uot = new ircda.hobbes.CookieContext();
            SSOConfidence conf = new SSOConfidence();
            SSOConfidence testconf = uot.CheckRequest(testContext, conf);
            Assert.IsNotNull(testconf);
            Assert.AreEqual(testconf.SimpleValue, 0, "Problem: Confidence is wrong,");
        }
    }
}