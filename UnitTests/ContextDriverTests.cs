//Test Driver for overall flow - for review 2016-Nov-21
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ircda.hobbes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;

namespace ircda.hobbes.Tests
{
    [TestClass()]
    public class ContextDriverTests
    {
        private HttpRequest request = new HttpRequest("", "http://localhost/", "");
        private HttpResponse response = new HttpResponse(null);
        [TestMethod()]
        public void VerifyAppDomainHasConfigurationSettings()
        {
            string value = ConfigurationManager.AppSettings["TestValue"];
            Assert.IsFalse(String.IsNullOrEmpty(value), "No App.Config found.");
        }
        [TestMethod()]
        public void CheckConfidenceTest()
        {
            SSOConfidence result = null;
            try
            {
                request.Cookies.Add(CookieTools.MakeCookie(CookieTools.HobbesCookieName, "testValue"));
                HttpContext testContext = new HttpContext(request, response);
                result = ContextDriver.CheckConfidences(testContext);  
            }
            catch(Exception ex)
            {
                Assert.Fail("Problem: Check Context Failure {0} @ {1}", ex.Message, ex.StackTrace);
            }
            Assert.IsNotNull(result, "Problem: ContextDriver.CheckConfidences returned null");
            Assert.IsTrue(result.SimpleValue > 50, "Problem: No Confidence at all");
        }
    }
}