// CookieTools.cs 
// Submitted for review 2016-Nov-21
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Scamps;
using ircda.hobbes;

namespace ircda.hobbes
{
    /// <summary>
    /// Utility routine for our specific cookie needs
    /// </summary>
    public class CookieTools
    {
        //??? Should these be simple  getters and get inited elsewhere (YAGNI?) or const
        //$$$ Make these a HtmlString perhaps?
        /// <summary>
        /// The name of the cookie this library uses to shuttle information server-client 
        /// </summary>
        public static string HobbesCookieName = "Hobbes";
        /// <summary>
        /// key for User id in cookie
        /// </summary>
        public static readonly string UserID = "user";
        /// <summary>
        /// key for role info in cookie
        /// </summary>
        public static readonly string Role = "role";
        /// <summary>
        /// key for token info in cookie
        /// </summary>
        public static readonly string Token = "token";
        /// <summary>
        /// key for Session expiration time in cookie
        /// </summary>
        public static readonly string SessionExpires = "SessExp";
        /// <summary>
        /// key for other active expiration time
        /// </summary>
        public static readonly string ActiveExpires = "ActExp";

        /// <summary>
        /// default number if hours added to cookie expiration 
        /// </summary>
        public const int DefaultHoursToAdd = 72; //TODO: make this configurable

        /// <summary>
        /// Calulate new expiration time
        /// </summary>
        /// <returns>dateTime of updated expiration</returns>
        public static DateTime NewExpiresTime(int hours = DefaultHoursToAdd)
        {
            DateTime retVal = DateTime.Now.AddHours(hours);
            return retVal;
        }
        /// <summary>
        /// Given an expiration time string, return how much time is left
        /// </summary>
        /// <returns>TimeSpan</returns>
        public static TimeSpan TimeTilExpires(string expiresTime)
        {
            DateTime expiresDT = Convert.ToDateTime(expiresTime);
            return TimeTilExpires(expiresDT);
        }
        /// <summary>
        /// Given an expiration time DateTime, return TimeSpan how much time is left
        /// </summary>
        /// <returns>TimeSpan</returns>
        public static TimeSpan TimeTilExpires(DateTime expiresTime)
        {
            //$$$ Assuming expiration is later than now
            TimeSpan ts = expiresTime - DateTime.Now;
            return ts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentExpiration"></param>
        /// <returns></returns>
        public static DateTime UpdateTimeLeft(string currentExpiration)
        {
            //$$$ Validate param....currently trusting since this routine creates, but...
            DateTime retval = Convert.ToDateTime(currentExpiration);


            return retval;
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
                if (cookies[HobbesCookieName] != null) 
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
        public static HttpCookie MakeCookie(string cookieName, string value=null)
        {
            HttpCookie retCookie = null;
            if (!string.IsNullOrEmpty(cookieName))
            {
                retCookie = new HttpCookie(HttpUtility.HtmlEncode(cookieName));
                retCookie.Expires = CookieTools.NewExpiresTime();
                if (!string.IsNullOrEmpty(value))
                {
                    retCookie.Value = HttpUtility.HtmlEncode(value);
                }
                retCookie = CookieExtensions.EncryptCookie(retCookie);
            }
            return retCookie;
        }
        /// <summary>
        /// Add subvalues to a cookie...creates new KV pair if nonexistant
        /// Adds additional values to key otherwise!
        /// </summary>
        /// <param name="cookie">Cookie to add kv pair to</param>
        /// <param name="key">key</param>
        /// <param name="value">concatentaed values (if more than one)</param>
        /// <returns>new encrypted cookie with added value </returns>
        public static HttpCookie AddTo(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = CookieExtensions.DecryptCookie(cookie);
            //Unencrypted cookie...
            //Now the values can be manipulated
            retCookie.Values.Add(key, value); //does this add duplicate keys?
            retCookie = CookieExtensions.EncryptCookie(retCookie);
            //Encrypted cookie
            retCookie.Expires = NewExpiresTime();
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
        /// Update a cookie key value pair
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>new encrypted cookie with updated key,value pair</returns>
        public static HttpCookie SetCookieValue(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = CookieExtensions.DecryptCookie(cookie);
            //Unencrypted cookie...
            //Now the values can be manipulated
            retCookie.Values[key] = value;  
            retCookie = CookieExtensions.EncryptCookie(retCookie);
            //Encrypted cookie
            retCookie.Expires = NewExpiresTime();
            return retCookie;
        }
        /// <summary>
        /// Get a particular value from the cookie
        /// </summary>
        /// <param name="cookies"></param>
        /// <param name="key"></param>
        /// <returns>return a specific key from the cookie</returns>
        public static string GetHobbesCookieValue(HttpCookieCollection cookies, string key)
        {
            HttpCookie retCookie = cookies[HttpUtility.HtmlEncode(HobbesCookieName)];
            retCookie = CookieExtensions.DecryptCookie(retCookie);
            //cookie values are decoded
            return retCookie.Values[HttpUtility.HtmlEncode(key)];
        }
        /// <summary>
        /// Return all cookie values as a dictionary
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns>Dictionary of cookie key value pairs</returns>
        public static Dictionary<string, string> GetCookieValues(HttpCookie cookie)
        {
            Dictionary<string, string> retval = new Dictionary<string, string>();
            if (cookie != null)
            {
                cookie = CookieExtensions.DecryptCookie(cookie);
                //make dictionary with decoded values
                foreach (string key in cookie.Values )
                {
                    var val = cookie.Values[key];
                    if (!key.IsNullOrEmpty())
                        retval.Add(key, HttpUtility.HtmlDecode(val));
                }
            }
            return retval;
        }
        /// <summary>
        /// Renew the IRCDA (Hobbes)specific cookie 
        /// </summary>
        /// <param name="cookies"></param>
        public static void RenewCookie(HttpCookieCollection cookies)
        {
            cookies[HttpUtility.HtmlEncode(HobbesCookieName)].Expires = NewExpiresTime();
        }      
    }
    /// <summary>
    /// Should work as extension class as well 
    /// </summary>
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
            //$$$ Do we want to leave the original untouched?
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
            //$$$ Do we want to leave the original untouched?
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
            HttpCookie cookie = CookieTools.MakeCookie(CookieTools.HobbesCookieName, null);
        }
    }

}
