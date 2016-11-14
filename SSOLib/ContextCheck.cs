using System;
using System.Collections.Generic;
using System.Web;

namespace ircda.hobbes
{
    public class Id
    {
        public string Name { get; set; }
    }
    public class Cookies
    {
        public static bool HasCookie(HttpContext context)
        {
            throw new NotImplementedException();
            //return false;
        }
    }

    public interface IContextChecker
    {
        SSOConfidence CheckRequest(HttpContext context);
        Dictionary<string,string> ConfigKeys { get; }
    }

    class EndpointContext : IContextChecker
    {
        public Dictionary<string, string> ConfigKeys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SSOConfidence CheckRequest(HttpContext context)
        {
            /*check whitelists here */
            SSOConfidence retval = null;
            if (IsURLanEndPoint(context))
            {
                retval.SimpleValue = SSOConfidence.Complete;
                //retval.Instruction = ProcessDocument(endpoint);
            }
            else {
                retval.SimpleValue = SSOConfidence.NoConfidence;
            }
            return retval;
        }

        private bool IsURLanEndPoint(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
    class CookieContext : IContextChecker
    {
        public SSOConfidence CheckRequest(HttpContext context)
        {
            SSOConfidence retval = null;
            if (Cookies.HasCookie(context))
            {
                /*either High or PartialConfidence */
                retval.SimpleValue = CalculateCookieConfidence(context);

            }
            else {
                retval.SimpleValue = SSOConfidence.NoConfidence;

            }
            return retval;

        }

        private int CalculateCookieConfidence(HttpContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the specific configuration key value pairs for this type
        /// </summary>
        public Dictionary<string, string> ConfigKeys
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
    class NetworkContext : IContextChecker
    {
        public SSOConfidence CheckRequest(HttpContext context)
        {
            SSOConfidence retval = null;
            Id netId = getNetworkID(context);
            if (!String.IsNullOrEmpty(netId.Name ))
            {
                if (IsKnownIdLocally(netId))
                {
                    retval.SimpleValue = CalculateNetworkConfidence(netId, context);
    
                }
            }
            else
            {
                retval.SimpleValue = SSOConfidence.NoConfidence;
            }
            return retval;
        }
        /// <summary>
        /// Get the specific configuration key value pairs for this type
        /// </summary>
        public Dictionary<string, string> ConfigKeys
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        private int CalculateNetworkConfidence(Id netId, HttpContext context)
        {
            throw new NotImplementedException();
        }

        private Id getNetworkID(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private bool IsKnownIdLocally(Id netId)
        {
            throw new NotImplementedException();
        }
    }
}
