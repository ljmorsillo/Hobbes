///<summary>
///Context Checks for Single-Signon, Reduced Sign on
///Submitted for review 2016-Nov-21 please start with ContextDriver to see overall flow
///</summary>
using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Scamps;
using System.Text.RegularExpressions;

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
    }
    /// <summary>
    /// Actions which are same across all context checks 
    /// </summary>
    public class ContextActions
    {
        public DBadapter db;
        public DataTools dt;
        ///<summary> ConfigKeys is a context dependent list of KV pairs which allow flexibility
        ///defining confidence ranges, endpoint expressions and other configurable values
        ///</summary>
        protected Dictionary<string, string> ConfigKeys { get; }

        public ContextActions()
        {
            //initialize from external sources at creation
            ConfigKeys = new Dictionary<string, string>();
            
            //Just creating new adapter doesn't really work - defaults to SCAMPS connection....
            //db = new DBadapter();

            //System.Configuration.ConfigurationManager.ConnectionStringsSettings cstring = 
            //dt = new DataTools(System.Configuration.ConfigurationManager.ConnectionStrings["hobbes"]);
        }
        /// <summary>
        /// Do we have this user locally?
        /// </summary>
        /// <param name="userToFind"></param>
        /// <returns></returns>
        public string GetLocalUserID(Id userToFind)
        {
            string retVal = null;

            return retVal;
        }
        public string AddUserToLocal(Id userToAdd)
        {
            string retVal = null;

            return retVal;
        }
    }
    public static class ContextDriver
    {
        public static void CheckConfidences(HttpContext context)
        {
            /***
            Process for applying context checks to an incoming request and
            to calculate a confidence interval then handle the proper challenge
            ***/
            //Initial checks:
            // EndpointContext /*isEndpoint?setHighConfidence:NoConfidence */
            // CookieContext /*isCookie?checkCoookie():setNoConfidence; calculateCookieConfidence() */
            // NetworkContext /*networkId?checkCredential:calculateNetworkConfidence() */
            List<IContextChecker> contextchecks = new List<IContextChecker>();
            contextchecks.Add(new EndpointContext());
            contextchecks.Add(new NetworkContext());
            contextchecks.Add(new CookieContext());

            SSOConfidence cumulativeConfidence = new SSOConfidence();
            foreach (IContextChecker check in contextchecks)
            {
                cumulativeConfidence = SSOConfidence.Accumulate(check.CheckRequest(context, cumulativeConfidence));
            }
            /** Get the confidence settings from the configuration file **/
            //Confidence high = readConfigValue("HighConfidence") partialChallenge = readConfigValue("PartialConfidence"), 
            //  fullChallenge == readConfigValue("NoConfidence")
            /* following provides rout to failed login, partial login, full login */
            // EvaluateConfidenceRules()    
            // RouteBasedOnConfidence()
        }

    }
    /// <summary>
    /// Check to see if directed to an endpoint - if so,
    /// mark the Action of the SSOConfidence to go handle the endpoint 
    /// </summary>
    public class EndpointContext : ContextActions, IContextChecker
    {
        protected Regex simpleEndpointRE = new Regex(@".*/$");
        public EndpointContext()
        {
            ///TODO read from DB
            ///Select "WhitelistEndpoints" from DB
            ///
            if (ConfigKeys != null)
            {
                ConfigKeys.Add("endpoint", "http://localhost/hobbes/ehr.ajax");

            }
        }


        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            /*check whitelists here */
            SSOConfidence retval = confidenceIn != null ? confidenceIn : new SSOConfidence();
            if (IsURLanEndPoint(context))
            {
                retval.SimpleValue = SSOConfidence.Complete;
                //retval.Instruction = ProcessDocument(endpoint);
            }
            else
            {
                retval.SimpleValue = SSOConfidence.NoConfidence;
            }
            return retval;
        }

        private bool IsURLanEndPoint(HttpContext context)
        {
            bool retVal = false;
            //Check the request
            string requestedURL = context.Request.Url.AbsolutePath;
            //parse to determine if endpoint

            //RegEx Checks
            if (simpleEndpointRE.IsMatch(requestedURL))
            {
                return true; //Restructure....
            }

            ///Whitelist check
            /// pull out hostname and check if in whitelist - foreach (item in whitelist){}
            /// later: gethostbyname() and check addresses...
            
            return retVal;
        }
    }
    public class CookieContext : ContextActions, IContextChecker
    {
        public CookieContext()
        {   
            // anything to do here?
            
        }
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            SSOConfidence retval = (confidenceIn != null) ? confidenceIn : new SSOConfidence();
            //Check if we have cookies and if we have our cookie
            //??? Will we ever want to check for other cookies?
            if (CookieTools.HasCookie(context.Request.Cookies))
            {
                /*either High or PartialConfidence */
                HttpCookie target = context.Request.Cookies[CookieTools.IRCDACookieName];
                retval.SimpleValue = calculateCookieConfidence(target);

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
    }
    class NetworkContext : ContextActions, IContextChecker
    {
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            SSOConfidence retval = confidenceIn != null?confidenceIn:new SSOConfidence();
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

        ///<summary>
        ///Details of network confidence calculations...
        ///</summary>
        ///<returns> Simple confidence value (int) </returns>
        private int CalculateNetworkConfidence(Id netId, HttpContext context)
        {
            int retVal = 0;
            if (IsKnownIdLocally(netId))
                retVal = 50; //Partial Confidence value
            return retVal;
        }
        /// <summary>
        /// Resolve what the network ID of the user is...
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Id getNetworkID(HttpContext context)
        {
            Id retVal = new Id();
            if (context.Request.Headers["Authorization"] != null)
            {
                string value = context.Request.Headers["Authorization"];
                retVal.Name = value.Split(':')[0]; //string before : is username
            }
            //??? What if we have both?
            if (String.IsNullOrEmpty(retVal.Name) && context.User != null && context.User.Identity != null)
            {
                retVal.Name = context.User.Identity.Name;
            };

            //$$$ The following will probably not fail gracefully
            if (!string.IsNullOrEmpty(context.Request.Url.UserInfo))
            {
                retVal.Name = context.Request.Url.UserInfo; //???No really, this is user:password@<host>... - do more with it!
            }
            return retVal;
        }

        private bool IsKnownIdLocally(Id netId)
        {
            bool retVal = false;
            //check local database
            if (!string.IsNullOrEmpty(GetLocalUserID(netId)))
            {
                //TODO: Db check in providers (users)
                retVal = true;
            }
            else
            {
                //check cookies?
            }
            return retVal;
        }
    }
}
