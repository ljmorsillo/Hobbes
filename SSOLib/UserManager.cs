﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scamps;
using System.Configuration;
using System.Security.Cryptography;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Configuration;

namespace ircda.hobbes
{
    /// <summary>
    /// This is a somewhat horrific object that isolates interaction with the user database
    /// Not a high level of abstration, collects methods that create, update, retreive, delete users
    /// </summary>
    public class UserManager
    {
        ///<summary>Column name for username</summary>
        public static string UsernameCol = "username"; //!!! Use cleverer way to make easily configuarble...
        ///<summary>Column name hash</summary>
        public static string HashCol = "hash";
        ///<summary>Column name salt</summary>
        public static string SaltCol = "salt";
        ///<summary>Table name for user information</summary>
        public static string UsersTableName = "users";

        public static readonly int USER_AUTHENTICATED = 0;
        public static readonly int USER_NONEXISTENT = 1;
        public static readonly int USER_NO_DATA = 2;
        public static readonly int USER_PW_FAIL = 3;
        public static readonly int USER_INACTIVE = 4;

        public static readonly int USER_NOT_UNIQUE = 10;

        ///<summary>Datatools provider</summary>
        protected string provider;
        ///<summary>Datatools connection string</summary>
        protected string connectionString;

        //$$$ extern the SQL
        string findUserQuery = "select username from users where username = @nametofind;";
        string getUserRecordQuery = "select * from users where username = @nametofind";
        string findUserQueryTok = "select username from users where username = '{0}';";
        string getHashQuery = "select hash, salt from users where username = @nametofind";
        string getHashQueryTok = "select * from users where username = '{0}'";
        string updateUserHashQueryTok = "update users set hash = '{0}', salt= '{1} where username = '{2}'";

        /// <summary>
        /// Responsible for tasks related to users and the DB, but not the UserStatus per-se
        /// primarily Functional, should not hold state
        /// </summary>
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
        /// Given username and other information authenticate the user from local db
        /// </summary>
        /// <param name="username">domain\user name or just name</param>
        /// <param name="password"></param>
        /// <param name="authmode">
        /// -1 - uninitialized or undefined authentication profile
        ///  0 - active directory user
        ///  1 - local database user</param>
        /// <returns> 0 - authenticated, 1 - user doesnt exist, 2 - user info populated but no data, 
        /// 3 - invalid password, 4 - user is set to inactive/no_access</returns>
        public int AuthenticateUser(string username, string password, out int authmode)
        {
            int retval = USER_NONEXISTENT; //Unintialized - undefined user
            authmode = -1;
            //Get the data first
            //$$$ Centalize this - without locking up a connection 
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();

            string[] parameter = { "@username", username };
            //!!! What is wrong with parameterized query?
            //dt.GetResultSet(getUserRecordQuery, parameter);
            string query = string.Format(getHashQueryTok, username);
            dt.GetResultSet(query);

            Dictionary<string, string> results = null;
            if (dt.rowcount < 1)
            {
                return retval;
            }
            //assume unique
            if (dt.rowcount == 1)
            {
                results = dt.GetRowStrings();
            }
            else
            {
                //TODO handle this situation - check email, 
                return USER_NOT_UNIQUE; 
            }
            
            //inactive?
            if (!results["active"].Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                retval = USER_INACTIVE;
            }

            //Active Directory returns....
            authmode = results["authmode"].IsNullOrEmpty() ? -1 : Convert.ToInt32(results["authmode"]);
             
            if (authmode == -1 || string.IsNullOrEmpty(results["hash"]))
            {
                return USER_NO_DATA;
            }
            //Given we have data, verify secret
            //get the hash and salt 
            string salt = results[SaltCol];
            byte[] saltAsBytes = Convert.FromBase64String(salt);

            string storedHash = results[HashCol];
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            //hash the salt and the password passed in
            byte[] PWtoBeHashed = Encoding.ASCII.GetBytes(password);
           
            var hashedPW = HashPasswordWithSalt(PWtoBeHashed,saltAsBytes);
            
            //compare the hash in the db from the computed value
            bool match = hashedPW.SequenceEqual(storedHashBytes);

            // return appropriate information - including authmode
            //  
            if (match)
            {
                return USER_AUTHENTICATED; //authenticated
            }
            else
            {
                return USER_PW_FAIL; //no match in local db
            }
            
            return retval;
        }
        /// <summary>
        /// Given a user with a given confidence, an authentication and role, 
        /// return a value for the access keys currently allowed
        /// </summary>
        /// <param name="username"></param>
        /// <param name="authenticated"></param>
        /// <param name="confidence"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public int AuthorizeUserRequest(string username, bool authenticated, SSOConfidence confidence, string reqRole)
        {
            int retVal = 0;
            Dictionary<string, string> userData = GetUser(username); //Data from DB
            UserStatus useStat = new UserStatus(username, authenticated, null, confidence);
            //check authentication
            if (!authenticated)
            {
                return retVal;
            }
            //check roles in DB against requested role
            if (!userData["role"].Equals(reqRole))
            {
                return retVal;
            } 
            //should this routine handle adminstrative levels and access keys
            else if ((confidence.SimpleValue <= SSOConfidence.CompleteConfidence) && 
                confidence.SimpleValue > SSOConfidence.NoConfidence)
            {
                retVal = 1;
            }
            // 0 = request denied
            // 1 = lowest authorization give requested role only
            // 2 = give requested role and add to other roles
            return retVal; 
        }
        /// <summary>
        /// Create a new user - base level simple insertion
        /// uses salted hash for password authentication
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">cleartext password</param>
        /// <param name="noDuplicates">Will we allow duplicates? Not usually...</param>
        /// <param name="parameters">Additional parameters to save in user record as Key,value pairs key must match db colname</param>
        /// <returns>numer of rows inserted or -1 means something bad happened</returns>
        public int CreateNewUser(string username, string password, bool noDuplicates = true, Dictionary<string,string>parameters = null)
        {
            int retval = -1;
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();

            string[] parameter = { UsernameCol, username };
            //!!! What is wrong with parameterized query?
            //dt.GetResultSet(findUserQuery, parameter);
            dt.GetResultSet(string.Format(findUserQueryTok, username));
            if (dt.rowcount > 0 && noDuplicates)
            {
                return retval;          //* EXIT
            }
            
            //optionally to fill other fields via parameters
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

            //string insertStatement = dt.InsertStatement(items,UsersTableName); ;
            int result = dt.Insert(items, UsersTableName);
            retval = result; //number of rows inserted...
            return retval;
        }


        /// <summary>
        /// Update a users hash and salt given a password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int UpdateUserHash(string username, string password, Dictionary<string,string>parameters = null)
        {
            int retval = -1;
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();
            Dictionary<string, string> items = new Dictionary<string, string>();
            byte[] salt = GenerateSalt();
            string saltString = Convert.ToBase64String(salt);
            string hash = HashPassword(password, salt);
            items.Add(HashCol, hash);
            items.Add(SaltCol, saltString);
            //TODO update to use userID (FindExactUser)
            retval = dt.Update(items, UsersTableName, string.Format("{0}='{1}'", UsernameCol, username));            
            return retval;
        }
        /// <summary>
        /// Delete a user by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>records deleted</returns>
        public int DeleteUser(string username)
        {
            int retval = -1;
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();

            retval = dt.Delete(UsersTableName, string.Format("{0}='{1}'", UsernameCol, username));
            return retval;
        }
        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="parameters"> key value pairs of (columnname,value) strings</param>
        /// <returns>number of items changed</returns>
        public int UpdateUserData(string username,Dictionary<string,string> parameters)
        {
            int retval = -1;
            DataTools dt = new DataTools();
            dt.Provider = provider;
            dt.ConnectionString = connectionString;
            dt.OpenConnection();

            //TODO update to use userID (FindExactUser)
            retval = dt.Update(parameters, UsersTableName, string.Format("{0}='{1}'", UsernameCol, username));
            return retval;
        }
        /// <summary>
        /// true if user login has expired
        /// </summary>
        /// <returns></returns>
        public bool Expired()
        {
            bool retVal = true;

            return retVal;
        }

        public Dictionary<string,string>GetSmartAuths()
        {

            return null;
        }
        ///$$$ This might better be in auth module
        /// <summary>
        /// return salted hash of password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt">Please use GenerateSalt()...</param>
        /// <returns></returns>
        /// <remarks>Using a cryptographic password hash and salt which is a crypto random number. 
        /// Take the password and and salt, hash them. Store hash and salt values.
        /// Given a password, take salt from db and the given password and hash them. Compare to hash stored in db
        /// If they match then the PW is correct.
        /// Each user record has unique salt and it changes any time the stored hash is changed (like when the pw is changed)
        /// Salting increases resistance to lookup table and rainbow table attacks</remarks>
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
        /// <summary>
        /// Hash the password with salt (from GenerateSalt())
        /// </summary>
        /// <param name="toBeHashed">usually the cleartext password</param>
        /// <param name="salt">should be cryptographic random number</param>
        /// <returns>the hash - should be run through Base64Encoding separately</returns>
        public byte[] HashPasswordWithSalt(byte[] toBeHashed, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedHash = Combine(toBeHashed, salt);
                return sha256.ComputeHash(combinedHash);
            }
        }
    }
    /// <summary>
    /// Tools and stuff to Authenticate against Active Directory
    /// </summary>
    public class ADAuthenticator
    {
        const string ADloginFormat = "{0}\\{1}";
        private string authDomain;
        private string authorizedGroup;
        private string ldapURL;
        private List<string> domains;
        /// <summary>Utility property...</summary>
        public UserPrincipal currentADUser { get; internal set; }
        /// <summary>
        /// Constructor - inits from config file
        /// </summary>
        public ADAuthenticator()
        {

            //Active Directory Authentication is configurable via web.config
            authDomain = WebConfigurationManager.AppSettings.Get("LDAPauthgroupdomain");
            authorizedGroup = WebConfigurationManager.AppSettings.Get("LDAPauthgroup");
            ldapURL = WebConfigurationManager.AppSettings.Get("LDAPurl");
            domains = WebConfigurationManager.AppSettings.Get("LDAPdomain").Split(',').ToList();
        }
        /// <summary>
        /// Take user info and authenticate
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns>0 is happiness, greater than 0 various suckages </returns>
        public int ADAuth(string username, string password, string domain)
        {
            if (!domains.Any(d => d.ToUpper() == domain.ToUpper()))
                return 1;

            var ADlogin = string.Format(ADloginFormat, domain, username);

            //below is code provided by MSDN for authorizing against active directory ... it is quirky but they have justification for the implementation
            var entry = new DirectoryEntry(this.ldapURL, ADlogin, password);
            try
            {
                var objnative = entry.NativeObject;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error connecting to active directory: " + e.ToString());
                return 3;
            }
            //thanks Bill Gates

            var context = new PrincipalContext(ContextType.Domain, domain);
            //var context = new PrincipalContext(ContextType.Domain, domain, username, password);
            if (context.ValidateCredentials(username, password))
            {
                currentADUser = UserPrincipal.FindByIdentity(context, username);
                if (isUserInGroup(username, password))
                    return 0;
                else
                    return 2;
            }
            return 3;
        }

        private bool isUserInGroup(string username, string password)
        {
            if (string.IsNullOrEmpty(this.authDomain) || string.IsNullOrEmpty(this.authorizedGroup) || this.currentADUser == null)
                return false;

            bool foundUser = false;
            using (var context = new PrincipalContext(ContextType.Domain, this.authDomain, username, password))
            {
                using (var group = GroupPrincipal.FindByIdentity(context, this.authorizedGroup))
                {
                    if (group != null)
                    {
                        // GetMembers(true) is recursive (groups-within-groups)
                        // this is awful as it is a linear traversal but checking if user's membership with group membership doesn't work as intended for whatever reason
                        foreach (var member in group.GetMembers(true))
                        {
                            foundUser = member.SamAccountName.Equals(username, StringComparison.OrdinalIgnoreCase);
                            if (foundUser)
                                break;
                        }
                    }
                }
            }
            return foundUser;
        }
    }
}
