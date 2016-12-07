///Submitted for review 2016-Nov-21
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Scamps;

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

        public const int DefaultHoursToAdd = 72; //TODO: make this configurable

        /// <summary>
        /// Calulate new expiration time
        /// </summary>
        /// <returns>dateTime of updated expiration</returns>
        public static DateTime TimeTilExpires(int hours = DefaultHoursToAdd)
        {
            DateTime retVal = DateTime.Now.AddHours(hours);
            return retVal;
        } 
        /// <summary>
        /// Checks for the presence of the IRCDA cookie
        /// </summary>
        /// <param name="cookies"> a list of cookies (i.e. Request.Cookies)</param>
        /// <returns>bool true if found</returns>
        public static bool HasIRCDACookie(HttpCookieCollection cookies)
        {
            bool retVal = false;    //default assume no cookie
            if (cookies.Count >= 1)
            {
                //do we have one of our cookies?
                if (cookies[IRCDACookieName] != null) 
                    retVal = true;
            }
            return retVal;
        }
        /// <summary>
        /// Make an encrypted IRCDA cookie
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="value">if provided, is the Value (or Values [0]of the cookie)</param>
        /// <returns>New Cookie, encrypted or null if no Cookie Name</returns>
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
                retCookie = CookieExtensions.EncryptCookie(retCookie);
            }
            return retCookie;
        }
        /// <summary>
        /// Add subvalues to a cookie...
        /// </summary>
        /// <param name="cookie">Cookie to add kv pair to</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>new encrypted cookie with added value </returns>
        public static HttpCookie AddTo(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = CookieExtensions.DecryptCookie(cookie);
            //Unencrypted cookie...
            //Now the values can be manipulated
            retCookie.Values.Add(key, value); //does this add duplicate keys?
            retCookie = CookieExtensions.EncryptCookie(retCookie);
            //Encrypted cookie
            retCookie.Expires = TimeTilExpires();
            return retCookie;
        }

        /// <summary>
        /// Get value from a SCAMPS encrypted cookie 
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCookieValue(HttpCookie cookie, string key)
        {
            string retVal = null;
            HttpCookie temp = CookieExtensions.DecryptCookie(cookie);
            retVal = temp.Values[key];
            return retVal;
        }
        /// <summary>
        /// Get a particular value from the cookie
        /// </summary>
        /// <param name="cookies"></param>
        /// <param name="key"></param>
        /// <returns>return a specific key from the cookie</returns>
        public static string GetIrcdaCookieValue(HttpCookieCollection cookies, string key)
        {
            HttpCookie retCookie = cookies[HttpUtility.HtmlEncode(IRCDACookieName)];
            retCookie = CookieExtensions.DecryptCookie(retCookie);
            //cookie values are decoded
            return retCookie.Values[HttpUtility.HtmlEncode(key)];
        }
        public static void RenewCookie(HttpCookieCollection cookies)
        {
            cookies[HttpUtility.HtmlEncode(IRCDACookieName)].Expires = TimeTilExpires();
        }
        
    }
    public static class CookieExtensions
    {
        /// <summary>
        /// Simply duplicates a cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns>New cookie, simple copy of input</returns>
        /// <remarks>No change in encryption</remarks>
        public static HttpCookie Dupe(this HttpCookie cookie)
        {
            HttpCookie temp = new HttpCookie(cookie.Name, cookie.Value);
            temp.Path = cookie.Path;
            temp.Domain = cookie.Domain;
            temp.Expires = cookie.Expires;
            temp.HttpOnly = cookie.HttpOnly;
            temp.Secure = cookie.Secure;
            temp.Shareable = cookie.Shareable;
            return temp;
        }
        /// <summary>
        /// Return an new Encrypted cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns>new encrypted cookie</returns>
        public static HttpCookie EncryptCookie(HttpCookie cookie)
        {
            ///$$$do we want to leave the original untouched?
            HttpCookie retCookie = Dupe(cookie);
            if (!string.IsNullOrEmpty(cookie.Value))
            {
                retCookie.Value = HttpUtility.HtmlEncode(Tools.Encrypt(HttpUtility.HtmlEncode(cookie.Value)));
            } 
            return retCookie;
        }
        /// <summary>
        /// Return an new Decrypted cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns>new decrypted cookie</returns>
        public static HttpCookie DecryptCookie(HttpCookie cookie)
        {
            ///$$$ Do we want to leave the original untouched?
            HttpCookie retCookie = Dupe(cookie);
            if (!string.IsNullOrEmpty(cookie.Value))
            {
                retCookie.Value = HttpUtility.HtmlDecode(Tools.Decrypt(HttpUtility.HtmlDecode(cookie.Value)));
            }
            return retCookie;
        }


    }
    /// <summary>
    /// Object to represent a standard IRCDA cookie with certain subfields and encryption
    /// </summary>
    public class IRCDACookie
    {
        IRCDACookie()
        {
            HttpCookie cookie = CookieTools.MakeCookie(CookieTools.IRCDACookieName, null);
        }
    }

}
