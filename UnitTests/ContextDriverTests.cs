//Test Driver for overall flow - for review 2016-Nov-21
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
    public class ContextDriverTests
    {
        private HttpRequest request = new HttpRequest("", "http://localhost/", "");
        private HttpResponse response = new HttpResponse(null);

        [TestMethod()]
        public void CheckConfidenceTest()
        {
            
            try
            {
                request.Cookies.Add(CookieTools.MakeCookie(CookieTools.IRCDACookieName, "testValue"));
                HttpContext testContext = new HttpContext(request, response);
                ContextDriver.CheckConfidences(testContext);  // If this fails, there will be an exception
            }
            catch(Exception ex)
            {
                Assert.Fail("Problem: Check Context Failure {0} @ {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}