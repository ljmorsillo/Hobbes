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
        /* ??? These should simple be getters and get inited elsewhere (YAGNI?)*/
        public static readonly string IRCDACookieName = "Ircda";
        public static readonly string Userkey = "user";
        public static readonly string Rolekey = "role";

        public static bool HasCookie(HttpCookieCollection cookies)
        {
            bool retVal = false;    //default assume no cookie
            if (cookies.Count > 1)
            {
                //do we have one of our cookies?

                retVal = true;
            }
            else
            {
                retVal = false;
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
                retCookie = new HttpCookie(cookieName);
                retCookie.Expires = DateTime.Now.AddDays(1); //$$$ make this configurable
                if (!string.IsNullOrEmpty(value))
                {
                    retCookie.Value = HttpUtility.HtmlEncode(value);
                }
            }
            return retCookie;
        }

        public static HttpCookie AddTo(HttpCookie cookie, string key, string value)
        {
            HttpCookie retCookie = cookie;
            retCookie.Values.Add(key, value); //does this add duplicate keys?
            retCookie.Expires = DateTime.Now.AddDays(1); //$$$ make this configurable
            return retCookie;
        }

    }
    public class CookieFactory
    {
    }
}
