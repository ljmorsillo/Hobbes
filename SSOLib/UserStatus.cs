using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ircda.hobbes
{
    public class UserStatus
    {
        SSOConfidence Confidence { get; set; }
        bool Authenticated { get; set; }
        
        /// <summary>
        ///Create a user status with the authentication 
        /// </summary>
        /// <param name="authenticated"></param>
        public UserStatus(bool authenticated)
        {
            Authenticated = authenticated;
        }

        /// <summary>
        /// Check if the user is in a particular role
        /// </summary>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public bool IsInRole(string rolename)
        {
            bool retval = false;

            return retval;
        }

        public bool IsSessionValid()
        {
            bool retval = false;

            return retval;
        }
    }
}
