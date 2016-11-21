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
    public class EndpointContextTests
    {
        private HttpRequest request = new HttpRequest("", "http://localhost/", "");
        private HttpResponse response = new HttpResponse(null);
        private EndpointContext endpointUUT = null;
        private SSOConfidence confidence = null;

        [TestInitialize]
        public void Setup()
        {
            endpointUUT = new EndpointContext();
            confidence = new SSOConfidence();
        }
        /// <summary>
        /// Very simple and lame test....
        /// </summary>
        [TestMethod()]
        public void CheckRequestTest()
        {
            request.Cookies.Add(CookieTools.MakeCookie(CookieTools.IRCDACookieName, "testValue"));
            HttpContext testContext = new HttpContext(request, response);
            
            confidence = endpointUUT.CheckRequest(testContext,confidence);
            Assert.IsTrue(confidence.SimpleValue == 100);
        }
    }
}