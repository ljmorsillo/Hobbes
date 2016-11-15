using Microsoft.VisualStudio.TestTools.UnitTesting;
using ircda.hobbes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;


namespace ircda.hobbes.Tests
{
    [TestClass()]
    public class CookieToolsTests
    {
        readonly string testCookieValue = "TestCookie with spaces";
        [TestInitialize]
        public void Setup()
        {

        }
        [TestMethod()]
        public void MakeCookieTest()
        {
            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.IRCDACookieName,testCookieValue);
            Assert.IsNotNull(testCookie, "Cookie is null");
            Assert.AreEqual(CookieTools.IRCDACookieName, HttpUtility.HtmlDecode(testCookie.Name), "Problem: Cookie value wrong");
        }
    }
}