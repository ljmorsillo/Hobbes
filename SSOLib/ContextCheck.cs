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
using System.Configuration;
using System.Linq;

namespace ircda.hobbes
{

    /// <summary>
    /// Driver object to call confidence pieces....
    /// </summary>
    public static class ContextDriver
    {
        public static SSOConfidence CheckConfidences(HttpContext context)
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

            SSOConfidence cumulativeConfidence = null; //@first there is no SSOConfidence(), one is created by checkRequest;
            foreach (IContextChecker check in contextchecks)
            {
                cumulativeConfidence = check.CheckRequest(context, cumulativeConfidence);
            }
            /** Get the confidence settings from the configuration file **/
            //Confidence high = readConfigValue("HighConfidence") partialChallenge = readConfigValue("PartialConfidence"), 
            //  fullChallenge == readConfigValue("NoConfidence")
            /* following provides route to failed login, partial login, full login */
            // EvaluateConfidenceRules()    
            // RouteBasedOnConfidence()
            return cumulativeConfidence;
        }
    }

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
        //public DBadapter db;
    
        public DataTools dt;
        ///<summary> ConfigKeys is a context dependent list of KV pairs which allow flexibility
        ///defining confidence ranges, endpoint expressions and other configurable values
        ///</summary>
        protected List<KeyValuePair<string,string>> ConfigKeys { get; set; }
        public string provider;
        public string connectionString;
        public ContextActions()
        {
            try
            {
                System.Configuration.Configuration rootWebConfig1 =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
                provider = System.Configuration.ConfigurationManager.ConnectionStrings["scamps"].ToString();
                connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SCAMPs"].ToString();
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }

            //initialize from external sources at creation
            ConfigKeys = new List<KeyValuePair<string, string>>();
            ConfigKeys.Add(new KeyValuePair<string, string>("Version", "0.2"));
            //Just creating new adapter doesn't really work - defaults to SCAMPS connection....
            //db = new DBadapter();

            //System.Configuration.ConfigurationManager.ConnectionStringsSettings cstring = 
            dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            //dt.OpenConnection();
        }
        /// <summary>
        /// Given a DataTools object where the query is set to grab the appropriate keys, 
        /// put the ContextSpecific keys into ConfigKeys
        /// </summary>
        /// <param name="db"></param>
        protected void InitConfigKeys(DataTools db)
        {
            if (ConfigKeys != null)
            {
                while (!db.EOF)
                {
                    var row = db.GetRow();
                    var nm = row["value"];
                    ConfigKeys.Add(new KeyValuePair<string, string>("endpoint", nm.ToString()));
                    db.NextRow();
                }
            }
        }
        /// <summary>
        /// Make sure the key inputs to CheckRequest and other methods are OK
        /// Returns an SSOConfidence object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="confidenceIn"></param>
        /// <returns>Confidence object - new (copy) object </returns>
        protected SSOConfidence CheckInputs(HttpContext context, SSOConfidence confidenceIn)
        {
            SSOConfidence retVal = confidenceIn != null ? new SSOConfidence(confidenceIn) : new SSOConfidence();
            if (context.Request == null)
                retVal.Action = SSOConfidence.BadRequest;
            return (retVal);
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
    /// <summary>
    /// Check to see if directed to an endpoint - if so,
    /// mark the Action of the SSOConfidence to go handle the endpoint 
    /// </summary>
    public class EndpointContext : ContextActions, IContextChecker
    {
        protected Regex simpleEndpointRE = new Regex(@".*/$");
        public EndpointContext() : base()
        {
            ///TODO read from DB
            ///Select "WhitelistEndpoints" from DB
            string query = "select value from environment where name like '%Whitelist-endpoint'"; ///$$$ Remove to external file
            using (var db = new Scamps.DataTools(System.Configuration.ConfigurationManager.ConnectionStrings["SCAMPs"]))
            {
                db.GetResultSet(query);
                //cseed = db.GetValue("select value from environment where name like '%Whitelist-endpoint'").ToString();
                InitConfigKeys(db);
            }
        }

         /// <summary>
         /// Check request to see if the endpoint is trusted, add to confidence 
         /// </summary>
         /// <param name="context">HttpContext object</param>
         /// <param name="confidenceIn">any previous confidence numbers if any or null</param>
         /// <returns></returns>
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            //Inputs
            SSOConfidence retval = CheckInputs(context, confidenceIn);
            if (retval.IsBadRequest())
                return retval; //Don't bother with further processing, it'll break

            int confidenceVal = 0;
            if (IsURLanEndPoint(context))
            {
                confidenceVal = SSOConfidence.CompleteConfidence;
                retval.Action = SSOConfidence.ProcessEndpoint;  //ProcessDocument(endpoint);
            }
            else
            {
                confidenceVal = SSOConfidence.NoConfidence;
            }
            retval.SimpleValue = confidenceVal;

            if (confidenceIn != null)
            {
                retval = retval.Accumulate(confidenceIn);
            }
            return retval;
        }
        /// <summary>
        /// Determine if requested url is an endpoint....
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
            /// pull out scheme and check if in whitelist - foreach (item in whitelist){}
            /// later: gethostbyname() and check addresses...
            requestedURL = context.Request.Url.Host + context.Request.Url.AbsolutePath;
            //list the endpoints to search
            var endpointlist = from key in ConfigKeys
                       where key.Key == "endpoint"
                       select key.Value;
            foreach (string endpoint in endpointlist)
            {
                if (requestedURL.Equals(endpoint.Trim()))
                {
                    retVal = true;
                    break;
                }
            }
            
            return retVal;
        }
    }
    public class CookieContext : ContextActions, IContextChecker
    {
        public CookieContext() : base()
        {   
            // anything to do here?
            // What are the config keys here?
        }
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            SSOConfidence retval = CheckInputs(context,confidenceIn);
            if (retval.IsBadRequest())
                return retval;
            int confidenceVal = 0;
            //Check if we have cookies and if we have our cookie
            //??? Will we ever want to check for other cookies?
            if (CookieTools.HasCookie(context.Request.Cookies))
            {
                /*either High or PartialConfidence */
                HttpCookie target = context.Request.Cookies[CookieTools.IRCDACookieName];
                confidenceVal = calculateCookieConfidence(target);         
            }
            else
            {
                confidenceVal = SSOConfidence.NoConfidence;
            }
            retval.SimpleValue = confidenceVal;

            if (confidenceIn != null)
            {
                retval = retval.Accumulate(confidenceIn);
            }
            return retval;

        }

        private int calculateCookieConfidence(HttpCookie cookie)
        {
            int retVal = 0;
            string userId = cookie[CookieTools.UserID];
            if (!string.IsNullOrEmpty(userId))
            {
                UserManager mg = new UserManager();
                Dictionary<string,string> usersfound = mg.GetUser(userId);
                if (usersfound.Count > 0)
                {
                    if (userId.Equals(usersfound["username"]))
                    {
                        retVal = 50;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get the specific configuration key value pairs for this type
        /// </summary>
    }
    class NetworkContext : ContextActions, IContextChecker
    {
        private string query = "select value from environment where name like '%Whitelist-network'"; ///$$$ Remove to external file
        public NetworkContext() :  base()
        {          
            using (var db = new Scamps.DataTools(System.Configuration.ConfigurationManager.ConnectionStrings["SCAMPs"])) //$$$ Hardcoded string!
            {
                db.GetResultSet(query);
                InitConfigKeys(db);
            }
        }
        public SSOConfidence CheckRequest(HttpContext context, SSOConfidence confidenceIn)
        {
            //Inputs
            SSOConfidence retval = CheckInputs(context, confidenceIn);
            if (retval.IsBadRequest())
                return retval; //Don't bother with further processing, it'll break
            int confidenceVal = 0;

            Id netId = getNetworkID(context);
            if (!String.IsNullOrEmpty(netId.Name ))
            {
                if (IsKnownIdLocally(netId))
                {
                    confidenceVal = CalculateNetworkConfidence(netId, context);
                }
            }
            else
            {
                confidenceVal = SSOConfidence.NoConfidence;
            }
            retval.SimpleValue = confidenceVal;

            if (confidenceIn != null)
            {
                retval = retval.Accumulate(confidenceIn);
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
