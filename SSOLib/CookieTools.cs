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
        
        public static DateTime TimeTilExpires = DateTime.Now.AddDays(1); //$$$ make this configurable

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
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpCookie MakeCookie(string cookieName, string value)
        {
            HttpCookie retCookie = null;
            if (!string.IsNullOrEmpty(cookieName))
            {
                retCookie = new HttpCookie(HttpUtility.HtmlEncode(cookieName));
                retCookie.Expires = CookieTools.TimeTilExpires;
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
        /// <param name="cookie"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpCookie AddTo(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = cookie;
            retCookie.Values.Add(key, value); //does this add duplicate keys?
            retCookie.Expires = TimeTilExpires;
            return retCookie;
        }

        public static string GetIrcdaCookieValue(HttpCookieCollection cookies, string key)
        {
            HttpCookie retCookie = cookies[HttpUtility.HtmlEncode(IRCDACookieName)];         
            return retCookie.Values[HttpUtility.HtmlEncode(key)];
        }

    }

}
