using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scamps;
using System.Configuration;
using System.Security.Cryptography;

namespace ircda.hobbes
{
    /// <summary>
    /// This is a somewhat horrific object that isolates interaction with the user database
    /// Not a high level of abstration, collects methods that create, update, retreive, delete users
    /// </summary>
    public class UserManager
    {
        public static string UsernameCol = "username"; //!!! Use cleverer way to make easily configuarble...
        public static string HashCol = "hash";
        public static string SaltCol = "salt";
        public static string UsersTableName = "users";

        protected string provider;
        protected string connectionString;

        //$$$ extern the SQL
        string findUserQuery = "select username from users where username = @nametofind;";
        string getUserRecordQuery = "select * from users where username = @nametofind";
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
            string[] parameter = {UsernameCol, username};
            //!!! What is wrong with parameterized query?
            //dt.GetResultSet(findUserQuery, parameter);
            dt.GetResultSet(string.Format(findUserQueryTok, username));

            while (!dt.EOF)
            {
                var row = dt.GetRow();
                var nm = row[UsernameCol];
                retVal.Add(UsernameCol, nm.ToString());
                dt.NextRow();
            }
            return retVal;
        }
        /// <summary>
        /// Given username & other information authenticate the user from local db
        /// </summary>
        /// <param name="username">domain\user name or just name</param>
        /// <param name="password"></param>
        /// <param name="authmode"></param>
        /// <returns> 0 - authenticated, 1 - user doesnt exist, 2 - user info populated but no data, 
        /// 3 - invalid password, 4 - user is set to inactive/no_access</returns>
        public int AuthenticateUser(string username, string password, int authmode)
        {
            int retval = -1; //Unintialized - undefined user
            authmode = -1;
            //Get the data first
            //$$$ Centalize this - without locking up a connection 
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();

            string[] parameter = { "@username", username };
            //!!! What is wrong with parameterized query?
            dt.GetResultSet(getUserRecordQuery, parameter);

            //Given we have data, verify secret

            //get the hash and salt 

            //hash the salt and the password passed in

            //compare the hash in the db from the computed value


            // return appropriate information - including authmode
            //  -1 - uninitialized or undefined authentication profile
            //   0 - active directory user
            //   1 - local database user

            return retval;
        }
        /// <summary>
        /// Create a new user - base level simple insertion
        /// uses salted hash for password authentication
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int CreateNewUser(string username, string password, Dictionary<string,string>parameters = null)
        {
            int retval = -1;
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();
            Dictionary<string, string> items = null;
            if (parameters != null)
            {
                items = new Dictionary<string, string>(parameters);
            }
            else
            {
                items = new Dictionary<string, string>();
            }

            items.Add(UsernameCol, username);
            byte[] salt = GenerateSalt();
            string saltString = Convert.ToBase64String(salt);
            string hash = HashPassword(password, salt);
            items.Add(HashCol, hash);
            items.Add(SaltCol, saltString);

            string insertStatement = dt.InsertStatement(items,UsersTableName); ;

            return retval;
        }
        ///$$$ This might better be in auth module
        /// <summary>
        /// return salted hash of password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password, byte[] salt)
        {
            string retval = "";
            
            var hashedPassword = HashPasswordWithSalt(Encoding.UTF8.GetBytes(password), salt);
            retval = Convert.ToBase64String(hashedPassword);
            return retval;
        }
        /// <summary>
        /// Create a 32 byte random salt
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] GenerateSalt()
        {
            const int saltLength = 32;
            using (var randomNumberGenerator = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[saltLength];
                randomNumberGenerator.GetBytes(randomNumber);
                return randomNumber;
            }
        }
        /// <summary>
        /// Utility function - append byte array to another
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>new array first followed by second</returns>
        private byte[] Combine(byte[] first, byte[] second)
        {
            var ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }

        public byte[] HashPasswordWithSalt(byte[] toBeHashed, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedHash = Combine(toBeHashed, salt);
                return sha256.ComputeHash(combinedHash);
            }
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
