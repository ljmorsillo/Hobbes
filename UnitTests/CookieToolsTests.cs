﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        HttpCookie gTestCookie = null;
        [TestInitialize]
        public void Setup()
        {
            gTestCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName);
        }
        [TestMethod()]
        public void MakeCookieTest()
        {
            ///Make a cookie with a value, no key
            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, testCookieValue);

            Assert.IsNotNull(testCookie, "Cookie is null");
            Assert.AreEqual(CookieTools.HobbesCookieName, HttpUtility.HtmlDecode(testCookie.Name), "Problem: Cookie name wrong");
            HttpCookieCollection coll = new HttpCookieCollection();
            coll.Add(testCookie);

            String actualValue = HttpUtility.HtmlDecode(CookieTools.GetHobbesCookieValue(coll, "TestCookie with spaces"));
            Assert.IsNull(actualValue, "Problem: Cookie has key, wrong");
            testCookie = CookieTools.AddTo(testCookie, "key1", "Ringo");
            actualValue = CookieTools.GetCookieValue(testCookie, "key1");
            Assert.AreEqual("Ringo", actualValue, "Problem: Cookie ['key1'] value incorrect");

            testCookie = null;
            //Test creating a cookie with no initial value
            testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, null);
            Assert.IsNotNull(testCookie, "Cookie is null");

            Assert.AreEqual(CookieTools.HobbesCookieName, HttpUtility.HtmlDecode(testCookie.Name), "Problem: Cookie name wrong");
            Assert.IsNull(testCookie.Value, "Problem: Cookie value wrong");

        }
        [TestMethod()]
        public void AddToCookieTest()
        {
            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, testCookieValue);
            testCookie = CookieTools.AddTo(testCookie, "TestagainKey", "TestAgainValue");
            Assert.IsNotNull(testCookie, "Cookie is null");
            Assert.AreEqual(CookieTools.HobbesCookieName, HttpUtility.HtmlDecode(testCookie.Name), "Problem: Cookie name wrong");
            Assert.AreEqual("TestAgainValue", CookieTools.GetCookieValue(testCookie, "TestagainKey"), "Problem: Cookie with subkey");

            testCookie = CookieTools.AddTo(testCookie, "TestagainKey", "TestAgainValue");
            Assert.AreEqual(CookieTools.HobbesCookieName, HttpUtility.HtmlDecode(testCookie.Name), "Problem: Cookie name wrong");

        }
        [TestMethod()]
        public void HasCookieTest()
        {
            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, testCookieValue);
            HttpCookie testCookie2 = CookieTools.MakeCookie("Bobs Cookie", "Bobs Cookie Val");
            HttpCookieCollection cookies = new HttpCookieCollection();
            cookies.Add(testCookie);
            cookies.Add(testCookie2);
            Assert.IsTrue(CookieTools.HasIRCDACookie(cookies));

        }
        [TestMethod()]
        public void GetIrcdaCookieValueTest()
        {
            string otherCookieVal = "Bobs Cookie";
            string testKey = "testkey";

            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, null);
            testCookie = CookieTools.AddTo(testCookie, testKey, "Bobs Cookie");
            HttpCookie testCookie2 = CookieTools.MakeCookie("Bobs Cookie", otherCookieVal);
            HttpCookieCollection cookies = new HttpCookieCollection();
            cookies.Add(testCookie);
            cookies.Add(testCookie2);
            string result = CookieTools.GetHobbesCookieValue(cookies, testKey);
            Assert.AreEqual("Bobs Cookie", result, "Problem: Didn't get value");
        }

        [TestMethod()]
        public void TimeTilExpiresTest()
        {

        }

        [TestMethod()]
        public void HasIRCDACookieTest()
        {

        }

        [TestMethod()]
        public void AddToTest()
        {

        }

        [TestMethod()]
        public void GetCookieValueTest()
        {

        }

        [TestMethod()]
        public void GetCookieValuesTest()
        {
            HttpCookie testCookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, testCookieValue);
            testCookie = CookieTools.AddTo(testCookie, "TestagainKey", "TestAgainValue");
            Dictionary<string, string> res = CookieTools.GetCookieValues(testCookie);
            Assert.AreEqual("TestAgainValue", res["TestagainKey"], "Problem:GetValues() incorrect value");

        }

        [TestMethod()]
        public void RenewCookieTest()
        {

        }
    }
}
