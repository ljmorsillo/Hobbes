using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ircda.hobbes
{
    public class CookieTools
    {
        /* ??? These should simple be getters and get inited elsewhere (YAGNI?) or const */
        //$$$ Make this a HtmlString perhaps?
        public static readonly string IRCDACookieName = "Ircda";
        public static readonly string UserID = "user";
        public static readonly string Role = "role";
        public static readonly string Token = "token";

        public static readonly int HoursToAdd = 72; //$$$ make this configurable

        /// <summary>
        /// Calulate new expiration time
        /// </summary>
        /// <returns>dateTime of updated expiration</returns>
        public static DateTime TimeTilExpires()
        {
            DateTime retVal = DateTime.Now.AddDays(HoursToAdd);
            return retVal;
        } 
        /// <summary>
        /// Checks for the presence of the IRCDA cookie
        /// </summary>
        /// <param name="cookies"> a list of cookies (i.e. Request.Cookies)</param>
        /// <returns>bool true if found</returns>
        public static bool HasCookie(HttpCookieCollection cookies)
        {
            bool retVal = false;    //default assume no cookie
            if (cookies.Count > 1)
            {
                //do we have one of our cookies?
                if (cookies[IRCDACookieName] != null) 
                    retVal = true;
            }
            return retVal;
        }
        /// <summary>
        /// Mostly for making test cookies
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="value">if provided, is the Value (or Values [0]of the cookie)</param>
        /// <returns>New Cookie</returns>
        public static HttpCookie MakeCookie(string cookieName, string value)
        {
            HttpCookie retCookie = null;
            if (!string.IsNullOrEmpty(cookieName))
            {
                retCookie = new HttpCookie(HttpUtility.HtmlEncode(cookieName));
                retCookie.Expires = CookieTools.TimeTilExpires();
                if (!string.IsNullOrEmpty(value))
                {
                    retCookie.Value = HttpUtility.HtmlEncode(value);
                }
            }
            return retCookie;
        }
        /// <summary>
        /// Add subvalues to a cookie...
        /// </summary>
        /// <param name="cookie">Cookie to add kv pair to</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>original cookie changed (side effect)</returns>
        public static HttpCookie AddTo(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = cookie;
            retCookie.Values.Add(key, value); //does this add duplicate keys?
            retCookie.Expires = TimeTilExpires();
            return retCookie;
        }
        /// <summary>
        /// Get a particular value from the cookie
        /// </summary>
        /// <param name="cookies"></param>
        /// <param name="key"></param>
        /// <returns>return a specific key from the cookie</return>
        public static string GetIrcdaCookieValue(HttpCookieCollection cookies, string key)
        {
            HttpCookie retCookie = cookies[HttpUtility.HtmlEncode(IRCDACookieName)];         
            return retCookie.Values[HttpUtility.HtmlEncode(key)];
        }
        public static void RenewCookie(HttpCookieCollection cookies)
        {
            cookies[HttpUtility.HtmlEncode(IRCDACookieName)].Expires = TimeTilExpires();
        }
    }

}
