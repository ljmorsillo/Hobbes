using Microsoft.VisualStudio.TestTools.UnitTesting;
using ircda.hobbes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ircda.hobbes.Tests
{
    [TestClass()]
    public class UserManagerTests
    {
        UserManager uut = null;
        [TestMethod()]
        public void GetUserTest()
        {
            uut = new UserManager();
            Dictionary<string, string> usersfound = uut.GetUser("TestUser");
        }

        [TestMethod()]
        public void HashPasswordTest()
        {
            uut = new UserManager();
            byte[] salt = uut.GenerateSalt();
            string hashedPW = uut.HashPassword("Welcome", salt);
            string saltString = Convert.ToBase64String(salt);
            Assert.IsNotNull(saltString);
            Assert.IsTrue(salt.SequenceEqual(Convert.FromBase64String(saltString)));
            System.Console.WriteLine("HPW: {0}, Salt: {1}", hashedPW, saltString);


        }

        [TestMethod()]
        public void AuthenticateUserTest()
        {
            uut = new UserManager();
            uut.UpdateUserHash("Tester", "Welcome");
            int authmode = 0;
            int result = uut.AuthenticateUser("Tester", "Welcome", out authmode);
        }
        [TestMethod()]
        public void CreateUserTest()
        {
            uut = new UserManager();
            int result = uut.CreateNewUser("Tester", "Welcome");

            int authmode = 0;
            result = uut.AuthenticateUser("Tester", "Welcome", out authmode);
        }

        [TestMethod()]
        public void DeleteUserTest()
        {
            uut = new UserManager();
            int resultCreate = uut.CreateNewUser("TestDelete", "Welcome");
            int resultDelete = uut.DeleteUser("TestDelete");
            Assert.AreEqual(resultCreate, resultDelete, "Problem: create & delete are unequal");

        }

        [TestMethod()]
        public void UserStatusTest()
        {
            SSOConfidence conf = new SSOConfidence();
            UserStatus uut = new UserStatus("Tester", false, CookieTools.MakeCookie("TestCookie", "TestValue"),
                conf);
            Assert.IsTrue(uut.Username.Equals("Tester"), "Problem: Username incorrect");
            Assert.IsTrue(uut.IsSessionValid(), "Session Invalid"); 
        }

        [TestMethod()]
        public void IsInRoleTest()
        {

        }

        [TestMethod()]
        public void IsSessionValidTest()
        {

        }
    }
}