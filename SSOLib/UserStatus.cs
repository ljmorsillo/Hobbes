﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace ircda.hobbes
{
    /// <summary>
    /// stateful object about user
    /// </summary>
    public class UserStatus
    {
        /// <summary>
        /// Confidence for this user for this session if calculated
        /// </summary>

        public SSOConfidence Confidence { get; set; }
        ///<summary>
        ///True if authenticated
        /// </summary>
        public bool Authenticated { get; set; }
        ///<summary>
        ///IRCDA Cookie reference
        /// </summary>
        public HttpCookie MyCookie { get; set; }
        ///<summary>
        ///Username used a lot
        /// </summary>
        public String Username { get; set; }

        Dictionary<string, string> userData = new Dictionary<string,string>();

        /// <summary>
        ///Create a user status with the authentication 
        /// </summary>
        /// <param name="authenticated"></param>
        public UserStatus(string username, bool authenticated, HttpCookie cookie, SSOConfidence confidence)
        {
            Authenticated = authenticated;
            MyCookie = cookie;
            Confidence = confidence;
            Username = username;

            UserManager userMgr = new UserManager();
            userData = userData.Union(userMgr.GetUser(username)).ToDictionary(pair => pair.Key,pair => pair.Value);
        }

        /// <summary>
        /// Check if the user is in a particular role
        /// </summary>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public bool IsInRole(string rolename)
        {
            bool retval = false;

            return retval;
        }

        public bool IsSessionValid()
        {
            bool retval = false;

            return retval;
        }
        public Dictionary<string,string> UserData()
        {
            //??? What are error conditions at this point
            return userData;
        }
    }
}
