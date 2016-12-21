using System;
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
        ///IRCDA Cookie reference (encrypted)
        /// </summary>
        public HttpCookie MyCookie { get; set; }
        ///<summary>
        ///Username used a lot
        /// </summary>
        public String Username { get; set; }

        Dictionary<string, string> userData = new Dictionary<string,string>();

        /// <summary>
        ///Create a user status with the authentication state and cookie 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="authenticated"></param>
        /// <param name="cookie"></param>
        /// <param name="confidence"></param>
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
            string role = CookieTools.GetCookieValue(MyCookie, CookieTools.Roles);
            if (string.IsNullOrEmpty(rolename))
            {
                return retval; //false, no null roles
            }
            //possible that role is in one of two places
            //return first match
            if (!string.IsNullOrEmpty(role))
            {
                retval = role.Equals(rolename);
                return retval;
            }
            if (userData.ContainsKey(CookieTools.Roles))
            {
                retval = userData.Contains(new KeyValuePair<string,string>(CookieTools.Roles, role));
            }
            return retval;
        }
        ///<summary>Quick check if we have run past any expirations we are carrying around...</summary>
        public bool IsSessionValid()
        {
            bool retval = false;
            //Check times
            string sessionExpires = CookieTools.GetCookieValue(MyCookie, CookieTools.SessionExpires);
            //if session expiration is not defined - it's false, it's required
            if (string.IsNullOrEmpty(sessionExpires))
            {
                return retval;
            }
            DateTime sessionDT= Convert.ToDateTime(sessionExpires);
            TimeSpan sessionTS = CookieTools.TimeTilExpires(sessionDT);
            if (sessionTS > new TimeSpan(0))
                retval = true; 
            //other tests that define valid session?

            return retval;
        }
        ///<summary>Return a dictionary of user data - returns the db columns and cookie data as key val pairs </summary>
        public Dictionary<string,string> UserData()
        {
            //merge cookie data to it
            Dictionary<string, string> cookieVals = CookieTools.GetCookieValues(MyCookie);
            foreach (var kvp in cookieVals)
                userData[kvp.Key] = kvp.Value;
            
            return userData;
        }
    }
}
