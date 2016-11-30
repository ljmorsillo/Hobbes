using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scamps;
using System.Configuration;

namespace ircda.hobbes
{
    class UserManager
    {
        protected string provider;
        protected string connectionString;

        //$$$ extern the SQL
        string findUserQuery = "select username from users where username = @nametofind;";
        string findUserQueryTok = "select username from users where username = '{0}';";
        public UserManager()
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
                Console.WriteLine("UserManager: Error reading app settings");
            }
        }
        /// <summary>
        /// Simply get a user from the localDb
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetUser(string username)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            //$$$ Centalize this - without locking up a connection 
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();
            string[] parameter = {"username", username};
            //!!! What is wrong with parameterized query?
            //dt.GetResultSet(findUserQuery, parameter);
            //string.Format(findUserQueryTok, parameter);
            dt.GetResultSet(string.Format(findUserQueryTok, username));

            while (!dt.EOF)
            {
                var row = dt.GetRow();
                var nm = row["username"];
                retVal.Add("username", nm.ToString());
                dt.NextRow();
            }
            return retVal;
        }
        /*
        public static int authenticateUser(string username, string password, out int authmode)
        {
            //return codes
            // 0 - authenticated, 1 - user doesnt exist, 2 - user info populated but no data, 3 - invalid password, 4 - user is set to inactive/no_access
            authmode = -1;
            var t = SCAMPstore.Get();

            var provider = t.ADMIN_PROVIDER.FirstOrDefault(u => u.USER_NAME.ToLower() == username.ToLower());

            if (provider == null)
                return 1;
            if (!provider.ACTIVE_STATUS.Equals("Y", StringComparison.OrdinalIgnoreCase) || provider.PROFILE_NAME.Equals("NO_ACCESS", StringComparison.OrdinalIgnoreCase))
                return 4;
            authmode = provider.AUTHENT_MODE.HasValue ? provider.AUTHENT_MODE.Value : -1;

            if (authmode == -1 || string.IsNullOrEmpty(provider.PASSWORD_TXT))
                return 2;

            if (!BCrypt.Net.BCrypt.Verify(password, provider.PASSWORD_TXT))
                return 3;

            return 0;
        }
        */


    }
}
