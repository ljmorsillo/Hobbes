using System;
using System.Collections.Generic;
using System.Web;

namespace ircda.hobbes
{
    /// <summary>
    /// UserId holder
    /// <note>stateful object</note>
    /// </summary>
    public class Id
    {
        public string Name { get; set; }
    }
 

    public interface IContextChecker
    {
        /// <summary>
        /// Do the confidence check of the request and return an updated, aggregate confidence
        /// Each type of context does different checks. Confidence will be checked against 
        /// thresholds to determine if SSO or chanllenge of some sort
        /// </summary>
        /// <param name="context">The Http request needs to be here</param>
        /// <param name="confidenceIn">Allows "chaining" the calls and aggregating the confidence</param>
        /// <returns>New confidence value summarizing what was checked togheter with previous checks</returns>
        SSOConfidence CheckRequest(HttpContext context,SSOConfidence confidenceIn);
        /// <summary>
        /// Each Context checker has it's own configuration key values
        /// each ConfigKeys will get the specific keys for the context
        /// </summary>
        Dictionary<string,string> ConfigKeys { get; }
    }

    class EndpointContext : IContextChecker
    {
        public Dictionary<string, string> ConfigKeys
        {
            /// Get the specific keys for endpoint context
            get
            {
                throw new NotImplementedException();
            }
        }

        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
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
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            SSOConfidence retval = confidenceIn;

            if (CookieTools.HasCookie(context.Request.Cookies))
            {
                /*either High or PartialConfidence */
                HttpCookie cookie = context.Request.Cookies[CookieTools.IRCDACookieName];

                retval.SimpleValue = calculateCookieConfidence(cookie);

            }
            else
            {
                retval.SimpleValue = SSOConfidence.NoConfidence;
            }
            return retval;

        }

        private int calculateCookieConfidence(HttpCookie cookie)
        {
            int retVal = 0;
            string userId = HttpUtility.HtmlDecode(cookie[CookieTools.IRCDACookieName]); //???get cookie subkey
            return retVal;
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
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
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
