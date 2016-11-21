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
    //  Example: Confidence high = readConfigValue("HighConfidence") 
    /// partialChallenge=readConfigValue("PartialConfidence"), fullChallenge=readConfigValue("NoConfidence")
    /// </summary>
    public class SSOConfidence
    {
        internal static readonly int NoConfidence = 0;
        internal static readonly int Complete = 100;

        public int SimpleValue { get; set; }
        public string Action { get; set; }
        /// <summary>
        /// No default empty constructor, must supply a value to start.
        /// Figure out a better typesafe mechanism - class or enum
        /// </summary>
        protected SSOConfidence()
        {
        }
        public SSOConfidence(int initialConfidence = 0)
        {
            SimpleValue = initialConfidence;
        }
        /// <summary>
        /// Aggregate confidence and weights....
        /// </summary>
        /// <param name="confidence"></param>
        /// <returns></returns>
        static public SSOConfidence Accumulate(SSOConfidence confidence)
        { 
            /* clever algorithm for aggregating confidence and weights  
		    from external configuration goes here... */
            return confidence;
        }
    }
}