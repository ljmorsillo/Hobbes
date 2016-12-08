// Confidence object - this is initially simple - submitted for review 2016-Nov-21
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ircda.hobbes
{
    /// <summary>
    /// Confidence Level for SSO/RSO based on various forms of context evidence
    /// evaluated against external settings
    ///Gets the confidence settings from esternal configuration data 
    ///  Example: Confidence high = readConfigValue("HighConfidence") 
    /// partialChallenge=readConfigValue("PartialConfidence"), fullChallenge=readConfigValue("NoConfidence")
    /// </summary>
    public class SSOConfidence
    {
        ///<summary> No confidence simple value</summary>
        public static readonly int NoConfidence = 0;
        ///<summary> Complete confidence simple value</summary>
        public static readonly int CompleteConfidence = 100;

        ///<summary>Action value</summary>
        public static readonly string BadRequest = "badrequest";
        ///<summary>Action Value trigger endpoint processing </summary>
        public static readonly string ProcessEndpoint = "processendpoint";
        ///<summary>Simple Value for confidence - no ranging</summary>
        public int SimpleValue { get; set; }

        ///<summary>Return the Action string - COULD BE NULL</summary>
        public string Action { get; set; }
        /// <summary>
        /// No default empty constructor, must supply a value to start.
        /// Figure out a better typesafe mechanism - class or enum
        /// </summary>
        protected SSOConfidence()
        {
        }
        /// <summary>
        /// Copy Constructor - Be sure to add any new properties/fields!!!
        /// </summary>
        /// <param name="confidenceIn"></param>
        public SSOConfidence(SSOConfidence confidenceIn)
        {
            SimpleValue = confidenceIn.SimpleValue;
            Action = confidenceIn.Action;
        }
        ///<summary>Initialized with simple confidence value, default:No-Confidence</summary>
        public SSOConfidence(int initialConfidence = 0)
        {
            SimpleValue = initialConfidence;
        }
        /// <summary>
        /// Aggregate confidence and weights....
        /// </summary>
        /// <param name="confidence"></param>
        /// <returns></returns>
        public SSOConfidence Accumulate(SSOConfidence confidence)
        {
            /* clever algorithm for aggregating confidence and weights  
		    from external configuration goes here... */
            confidence.SimpleValue += this.SimpleValue;

            return confidence;
        }
        /// <summary>
        /// convenience
        /// </summary>
        /// <returns>true if BadRequest in Action </returns>
        public bool IsBadRequest()
        {
            if (SSOConfidence.BadRequest == this.Action)
                return true;
            else
                return false;
        }
    }
}