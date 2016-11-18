using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Data;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtensionMethods
{

    public static class CustomExtensionMethods
    {
        public static DateTime Tomorrow(this DateTime date)
        {
            return date.AddDays(1);
        }
        public static DateTime Yesterday(this DateTime date)
        {
            return date.AddDays(-1);
        }

        /// <summary>
        /// Returns the index of the position of the first matched character in characterlist.  
        /// </summary>
        /// <param name="source">The string being searched</param>
        /// <param name="characterlist">Optional - the string of characters to be checked.  If no list is supplied, the first non-letter,non-number, including a space character found is returned.</param>
        /// <param name="startIndex">Optional - Character index in source to search from</param>
        /// <returns>Integer, -1 if no match is found.</returns>
        public static int IndexOfCharacter(this string source,string characterlist = "",int startIndex =0)
        {
            int i = -1;
            int c = 0;
            
            string clist = @"!@#$%^&*()_+-={}[]\|:;''<>?/ `";
            if (characterlist != "") { clist = characterlist;  }
            c = clist.Length;
            char[] stoplist = new char[c];
            for (int n = 0; n < clist.Length; n++)
            {
                stoplist[n] = Convert.ToChar(clist.Substring(n, 1));
            }
            i = source.IndexOfAny(stoplist, startIndex);
            return i;
        }
        public static string TokenOf(this string source)
        {
            string results = source;
            int i = source.IndexOf(":");
            if(i>-1)
            {
                results = source.Substring(0, i);
            }
            return results;
        }
        public static string Clean(this string source, string badcharacters = "")
        {

            string results = source;
            results = results.Replace("\n", "");
            results = results.Replace("\r", "");
            results = results.Replace("\t", "");

            if (badcharacters == "") { badcharacters = "~`;!@#$%^&*()={}[]|\\/<>.,\""; }

            for (int i = 0; i < badcharacters.Length; i++)
            {
                results = results.Replace(badcharacters.Substring(i, 1), "");
            }

            //deal with double spaces...
            results = results.Replace("  ", " ");
            //and because they can be compound...
            results = results.Replace("  ", " ");
            results = results.Replace("--", "-");
            results = results.Replace("--", "-");

            return results;

        }



        /// <summary>
        /// Examine if source has punctuation characters
        /// </summary>
        /// <param name="source">the string to examine</param>
        /// <returns>Boolean</returns>
        public static bool hasPunctuation(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsPunctuation(source[i]))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Examine if the source string contains only letters
        /// </summary>
        /// <param name="source">string to examine</param>
        /// <returns>Boolean</returns>
        public static bool hasLetters(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsLetter(source[i]))
                {
                    return true;
                }
            }
            return results;
        }
        /// <summary>
        /// Examine if the source string contains only letters
        /// </summary>
        /// <param name="source">string to examine</param>
        /// <returns>Boolean</returns>
        public static bool isLetters(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLetter(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        /// <summary>
        /// Add a CSS class to an element identified by existing class (.class), ID (#id), or an HTML element.
        /// </summary>
        /// <param name="source">The HTML string to insert the class reference into.</param>
        /// <param name="theclass">The class name to insert</param>
        /// <param name="selector">The selector.  Indicate a class with a dot (.class) and an ID with #.</param>
        /// <returns>String modified with the new class is any matching selector is found.</returns>
        /// <remarks>HTML or class selectors will match ALL instances within the source string.  IDs will match only one (there should only be one).</remarks>
        public static string AddClass(this string source, string theclass, string selector)
        {

            string results = source;
            string substring = "";
            string pat = "";
            string tmp = "";

            string pattern = "";
            Regex ck = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = ck.Match("");


            if (selector.StartsWith("."))
            {
                //class
                pat = selector.Substring(1);
                pattern = @"class\s*?=\s*?[\""\'].*?" + pat + @".*?[\""\']";

                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(results);
                while (match.Success)
                {
                    //Make sure we don't dupe the class.
                    if (!Regex.IsMatch(match.Value, "[\\\" ]+" + theclass + "[\\\" ]+"))
                    {
                        //got it, fix it
                        tmp = match.Value.ToString();
                        tmp = tmp.Replace(pat, pat + " " + theclass);
                        results = results.Replace(match.Value, tmp);
                    }
                    match = match.NextMatch();
                }
            }
            else if (selector.StartsWith("#"))
            {
                //ID
                pat = selector.Substring(1);
                pattern = @"id\s*?=\s*?[\""']" + pat + @"[\""\']";
                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(source);
                if (match.Success)
                {
                    int p = source.IndexOf(match.Value);   //match position...
                    if (p > -1)
                    {
                        tmp = source.Substring(0, p);
                        int s = tmp.LastIndexOf("<");
                        int e = -1;
                        if (s > -1)
                        {
                            e = source.Substring(s).IndexOf(">");
                        }
                        if (s > -1 & e > -1)
                        {
                            //extract just the HTML element with the matched ID...
                            tmp = source.Substring(s, e + 1);
                            pattern = @"class\s*?=\s*?[\""\\'].*?[\""\']";
                            ck = new Regex(pattern, RegexOptions.IgnoreCase);
                            match = ck.Match(tmp);
                            if (match.Success)
                            {
                                //found existing class in substring tmp
                                //Make sure we don't dupe the class.
                                if (!Regex.IsMatch(match.Value, "[\\\" ]+" + theclass + "[\\\" ]+"))
                                {
                                    substring = tmp.Replace(match.Value, match.Value.Substring(0, match.Value.Length - 1) + " " + theclass + match.Value.Substring(match.Value.Length - 1, 1));
                                    results = results.Replace(tmp, substring);
                                }
                            }
                            else
                            {
                                //add a class parameter to element...
                                p = tmp.Length - 1;
                                if (tmp.Substring(p - 1, 1) == "/") { p = p - 1; }
                                substring = tmp.Substring(0, p) + " class=\"" + theclass + "\"" + tmp.Substring(p);
                                results = results.Replace(tmp, substring);
                            }
                        }
                    }
                }

            }
            else
            {
                //HTML element
                int p = 0;
                pattern = @"<" + selector + @".*?>";

                Regex innerck = new Regex(pattern);
                Match m = innerck.Match("");
                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(source);
                while (match.Success)
                {
                    //extract just the HTML element...
                    tmp = match.Value;
                    pattern = @"class\s*?=\s*?[\""\\'].*?[\""\']";
                    innerck = new Regex(pattern, RegexOptions.IgnoreCase);
                    m = innerck.Match(tmp);
                    if (m.Success)
                    {
                        //found existing class in substring tmp
                        substring = tmp.Replace(m.Value, m.Value.Substring(0, m.Value.Length - 1) + " " + theclass + m.Value.Substring(m.Value.Length - 1, 1));
                        results = results.Replace(tmp, substring);
                    }
                    else
                    {
                        //add a class parameter to element...
                        p = tmp.Length - 1;
                        if (tmp.Substring(p, 1) == "/") { p = p - 1; }
                        substring = tmp.Substring(0, p) + " class=\"" + theclass + "\"" + tmp.Substring(p);
                        results = results.Replace(tmp, substring);
                    }

                    match = match.NextMatch();
                }
            }
            return results;
        }

        /// <summary>
        /// Extends strings to replace field tokens [token] in a string with the passed value
        /// </summary>
        /// <param name="source">The target string</param>
        /// <param name="token">the token to be replaced, without square brackets.  The token will be treated as lowercase</param>
        /// <param name="value">The value do you want to replace the token with</param>
        /// <returns></returns>
        public static string ReplaceToken(this string source, string token, string value)
        {
            string results = source.Replace("[" + token.Trim().ToLower() + "]", value);
            return results;
        }

        /// <summary>
        /// Merge the contents of dictionary hd into dictionary source, replacing any values in source found in hd.
        /// </summary>
        /// <param name="source">The target dictionary.  Combined values from hd will be added to source</param>
        /// <param name="hd">The dictionary to combine into source</param>
        public static void Combine(this Dictionary<string, string> source, Dictionary<string, string> hd)
        {

            foreach (KeyValuePair<string, string> item in hd)
            {
                source.Set(item.Key, item.Value);
            }
        }
        /// <summary>
        /// Replaces tokens in source with the values found in the data dictionary
        /// </summary>
        /// <param name="source">Source text to perform the replacement on.  Contains 0+ tokens [key]</param>
        /// <param name="data">Dictionary containing replacement value where the tokens in source = [key] in dictionary</param>
        /// <returns>string</returns>
        public static string Merge(this string source, Dictionary<string, string> data)
        {

            string results = source;
            string key = "";
            string value = "";

            foreach (KeyValuePair<string, string> item in data)
            {
                key = item.Key.ToLower();
                value = item.Value;
                results = results.Replace("[" + key + "]", value);
            }

            return results;
        }

        // String transforms
        public static string Quoted(this string source, char quotecharacter = '"')
        {
            return quotecharacter + source + quotecharacter;
        }

        // Dictionary Helpers

        /// <summary>
        /// Adds a new key value or replaces an existing key with the passed value
        /// </summary>
        /// <param name="key">The dictionary key to either add or update</param>
        /// <param name="value">The value of the element you want to change or add</param>
        /// <param name="data">a dictionary of strings, passed by reference</param>
        public static void Set(this Dictionary<string, string> data, string key, string value)
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }
        }

        /// <summary>
        /// Remove an item from a dictionary
        /// </summary>
        /// <param name="data">The dictionary reference containing the target key</param>
        /// <param name="key">The key name to remove</param>
        public static void Drop(this Dictionary<string, string> data, string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }
        }


        public static void AddEdit(this Dictionary<string, string> data, string key, string value, string defaultifempty = "")
        {
            key = key.ToLower();
            
            if(value.Length ==0) { value = defaultifempty; }
            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            } 
        }

        /// <summary>
        /// Get a dictionary items value based on a key
        /// </summary>
        /// <param name="data">The target dictionary</param>
        /// <param name="key">Key name to return the value of</param>
        /// <param name="defaultifempty">Default string value to return if the key is not found</param>
        /// <returns>String, either the key value or the default value passed in defaultifempty</returns>
        public static string Get(this Dictionary<string, string> data, string key,  string defaultifempty = "")
        {
            string results = "";
            if (data.ContainsKey(key))
            {
                results = data[key];
                //FoundIt = true;
                
                if (string.IsNullOrEmpty(results)  && !string.IsNullOrEmpty(defaultifempty))
                {
                    results = defaultifempty;
                }
            }
            return results;
        }

        public static void ParseQueryString(this Dictionary<string, string> hd, string querystring)
        {
            string[] aZ = querystring.Split('&');
            string[] kv;
            string tmp = "";
            try
            {
                for (int i = 0; i < aZ.Length; i++)
                {
                    kv = aZ[i].Split('=');
                    if (kv.Length == 1)
                    {
                        tmp = Uri.UnescapeDataString(kv[0]);
                        if (hd.ContainsKey(tmp.ToLower()))
                        {
                            hd[tmp.ToLower()] = tmp;
                        }
                        else
                        {
                            hd.Add(tmp.ToLower(), tmp);
                        }
                    }
                    else
                    {
                        if (kv.Length > 1)
                        {

                            tmp = Uri.UnescapeDataString(kv[0]);
                            if (hd.ContainsKey(tmp.ToLower()))
                            {
                                hd[tmp.ToLower()] = Uri.UnescapeDataString(kv[1]);
                            }
                            else
                            {
                                hd.Add(tmp.ToLower(), Uri.UnescapeDataString(kv[1]));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Logging 
            }
        }
        public static string ToXML(this Dictionary<string, string> data, string NodeWrapper = "")
        {
            string results = "";
            try
            {
                foreach (KeyValuePair<string, string> node in data)
                {
                    Scamps.Tools.AppendLine(ref results, "\t<" + node.Key.ToLower() + ">" + node.Value + "</" + node.Key.ToLower() + ">");
                }
                if (NodeWrapper.Length > 0)
                {
                    results = "<" + NodeWrapper + ">" + System.Environment.NewLine + results + "</" + NodeWrapper + ">";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ToXML.Exception:" + ex.Message, ex);
            }

            return results;
        }

        // valid checks

        public static bool ValidateMailFormat(this string sEmail)
        {

            Regex mailCK = new Regex("\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
            Match mat = mailCK.Match(sEmail);
            if (mat.Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        // case/contents of a string

        public static bool IsUpperCase(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsLower(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        /// <summary>
        /// Convert a string to an HTML encoded version.
        /// </summary>
        /// <param name="source">The original "source" string</param>
        /// <returns>STRING</returns>
        public static string AsHTML(this string source)
        {
            return System.Net.WebUtility.HtmlEncode(source);
        }

        /// <summary>
        /// Process common list entities like comma and colon that are escaped with either a forward or back slash into HTML entities
        /// </summary>
        /// <param name="list">The list string containing possible escaped entities.  Example \, would indicate a comma that is part of the data, not a delimiter.</param>
        public static string escapeList(this string list)
        {
            //escape list characters used for delimiters using html entities...
            //unfortunately the traditional backslash gives us trouble because C#
            //tries to escape the escape, so here we use the forward slash...
            list = list.Replace("/,", "&#44;");
            list = list.Replace("/:", "&#58;");
            list = list.Replace("/|", "&#124;");

            list = list.Replace(@"\,", "&#44;");
            list = list.Replace(@"\:", "&#58;");
            list = list.Replace(@"\|", "&#124;");
            return list;
        }

        public static string AsList(this string list)
        {
            list = list.Replace(",", @"\,");
            list = list.Replace("<", "&lt;");
            list = list.Replace(">", "&gt;");
            list = list.Replace("&", "&amp;");

            return list;
        }

        public static bool IsLowerCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLower(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        /// <summary>
        /// Evaluate if the source string contains digits
        /// </summary>
        /// <param name="source">String to examine</param>
        /// <returns>Boolean</returns>
        public static bool HasDigits(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsDigit(source[i]))
                {
                    results = true;
                    break;
                }
            }
            return results;
        }
        /// <summary>
        /// Examine the string to see if it is made of of only character in validcharacters
        /// </summary>
        /// <param name="source">the string to examine</param>
        /// <param name="validcharacters">a string containing all the valid characters to check against</param>
        /// <returns>Boolean</returns>
        public static bool IsOnly(this string source,string validcharacters)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (validcharacters.IndexOf(source[i]) == -1)
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool IsDigits(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsDigit(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool IsLettersOrDigits(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLetterOrDigit(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool IsLetters(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLetter(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool IsPunctuation(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsPunctuation(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool HasUpperCase(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsUpper(source[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasPunctuation(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsPunctuation(source[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static int AsInteger(this string source)
        {
            return Scamps.Tools.AsInteger(source);
        }
        public static string TrimTo(this string source,int length)
        {
            return Scamps.Tools.TrimTo(source,length);
        }
        //==========================

    }
}
namespace Scamps
{
    //class mostly lifted from Bittype library tools (c)2010
    //with slight mods for IRCDA/Scamps specific utility

    public class Tools
    {

        public static bool NodeFound = false;
        public static bool FoundIt = false;
        public static string ResultComment = "";
        public static bool HasMessages = false;
        public static string Messages = "";

        //private static string _encryptionkey = "hcxilkqbeultbhczfgbskdmaunivmfuo";
        private static string _encryptionkey = "amvnquatbehvgtwoioxfokhwnweisojy";
        private static string _salt = "mxdjvrkpygalrzlo";

        private int RED = 100;
        private int GREEN = 160;
        private int BLUE = 255;

        private static bool _haserrors = false;
        private static string _errors = "";
        public static Random RandomNumber = new Random();

        public static Dictionary<string, string> RegexPatterns = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { 
            { "email", @"^[a-zA-Z0-9_\.\+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-\.]+$" }, /* Matches upper/lower and allows for google + sign emails. It's a bit more strict than I'd like. */
            { "phone9digit", @"^(?:\+[\d]{0,3}[\s\-])?[\d]{3}([\D]?)[\d]{3}\1?[\d]{4}$"}, /* Matches 9 digit phone numbers (allowing for +1) and will allow for any non-number delimiter ex: 617.919.1234 or 617 919 1234 are both technically valid. */
            { "phoneinternational", @"^(?:\+[\d]{0,4}[\s\-]?)?(?:[\d]{1,}([\D]?))(?:[\d]{1,}\1?)+$" }/* Will match international numbers, only qualifier is the character between sets is consistent. IE: 011 64 3 477 4000 and 011.64.2.512.1234 */
        };

        public static bool haserrors
        {
            get
            {
                return _haserrors;
            }
            set
            {
                _haserrors = value;
            }
        }

        public static string errors
        {
            get
            {
                return _errors;
            }
            set
            {
            }
        }

        /// <summary>
        /// 32 byte encryption key.
        /// </summary>
        public static string EncryptionKey
        {
            set
            {

                if (value.Length < 32)
                {
                    Error("EncryptionKey: The key is too short.  Use a 32+ byte key.");
                }
                else
                {
                    _encryptionkey = value.Substring(0, 32);
                }
            }
        }

        /// <summary>
        /// 16 byte initialization vector to use.
        /// </summary>
        public static string EncryptionSalt
        {
            set
            {
                if (value.Length < 16)
                {
                    Error("EncryptionSalt: salt is too short.  Use a 16+ byte salt");
                }
                else
                {
                    _salt = value.Substring(0, 16);
                }
            }
        }

        private static void Message(string message)
        {
            HasMessages = true;
            Messages += message + System.Environment.NewLine;
        }

        private static void Error(string errormessage)
        {
            _errors += errormessage + System.Environment.NewLine;
            _haserrors = true;
        }


        public void Clear()
        {
            _haserrors = false;
            _errors = "";
            NodeFound = false;
            FoundIt = false;
            HasMessages = false;
            Messages = "";
            Array.Clear(listcache, 0, listcache.Length);
        }

        private static string[] listcache = { };

        //==============================================================================
        //                         F I L E S
        //==============================================================================
        /// <summary>
        /// Return the file extension of the passed file name
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns>String</returns>
        public static string ExtensionOf(string strPath)
        {
            if (System.IO.File.Exists(strPath))
            {
                return Path.GetExtension(strPath).ToLower();
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Append a string to a file
        /// </summary>
        /// <param name="strPath">Fully qualified path and file name.</param>
        /// <param name="sText"></param>
        public static void AppendFile(string strPath, string sText)
        {
            try
            {
                System.IO.StreamWriter sr = new System.IO.StreamWriter(strPath, true);
                sr.Write(sText);
                sr.Close();
                sr.Dispose();
            }
            catch (IOException e)
            {
                Error("AppendFile.IOException: " + e.Message);
            }

        }

        /// <summary>
        /// Write a new file strPath containing sText
        /// </summary>
        /// <param name="strPath">Full path and file name of the file to write</param>
        /// <param name="sText">Contents of the file to write</param>
        public static void WriteFile(string strPath, string sText)
        {
            try
            {
                System.IO.StreamWriter sr = new System.IO.StreamWriter(strPath,false);
                sr.Write(sText);
                sr.Close();
                sr.Dispose();
            }
            catch (IOException e)
            {
                Error("WriteFile.IOException: " + e.Message);
            }

        }

        /// <summary>
        /// Read a text file from a fully qualified path
        /// </summary>
        /// <param name="filepath">fully qualified path and file name to the file</param>
        /// <returns>The target file's contents</returns>
        public static string ReadFile(string strPath)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(AsText(strPath)))
            {
                Error("ReadFile.Path: Path cannot be an empty string");
            }
            else
            {
                try
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(strPath);
                    while (sr.Peek() != -1)
                    {
                        sb.Append(sr.ReadLine() + System.Environment.NewLine);
                    }
                    sr.Close();
                    sr.Dispose();
                }
                catch (IOException e)
                {
                    Error("ReadFile.IOException: " + e.Message);
                }
            }
            return sb.ToString();
        }

        public static string FileName(string strPath)
        {
            string functionReturnValue = null;
            //returns the filename portion of the passed path. 

            functionReturnValue = "";
            try
            {
                return Path.GetFileName(strPath);
            }
            catch (Exception ex)
            {
                Error("FileName.IOException: " + ex.Message);
            }
            return functionReturnValue;

        }


        public static void Kill(string strPath)
        {
            try
            {
                System.IO.File.Delete(strPath);
            }
            catch (IOException ex)
            {
                Error("Kill.IOException: " + ex.Message);
            }
        }
        public static bool FileExists(string strPath)
        {
            if (System.IO.File.Exists(strPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool DeleteFile(string strPath)
        {
            try
            {
                File.Delete(strPath);
                return true;
            }
            catch (IOException ex)
            {
                Message("DeleteFile.IO:" + ex.Message);
                return false;
            }
            catch (Exception exc)
            {
                Message("DeleteFile:" + exc.Message);
                return false;
            }
        }
        public static bool DeleteFiles(string strPath, string pattern = "*.*")
        {
            try
            {
                string[] filePaths = Directory.GetFiles(strPath, pattern);
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
                return true;
            }
            catch (IOException ex)
            {
                Message("DeleteFiles.IO:" + ex.Message);
                return false;
            }
            catch (Exception exc)
            {
                Message("DeleteFiles:" + exc.Message);
                return false;
            }
        }

        public static string normalizePath(string path)
        {
            path.Replace("/", "\\");
            path.Replace(@"\\", @"\");
            return path;
        }
        public static bool createPath(string path)
        {
            // create all directories in a path if they do not already exist
            // c:\\this\\is\\the\\path
            path = normalizePath(path);
            bool ok = false;
            try
            {
                Directory.CreateDirectory(path);
                ok = true;
            }
            catch (IOException ex)
            {
                //log here
            }
            return ok;
        }


        /// <summary>
        /// Return data table of files in directory that match pattern
        /// </summary>
        /// <param name="directory">File directory to examine</param>
        /// <param name="pattern">File pattern to select</param>
        /// <returns>Data Table</returns>
        public static DataTable Files(string directory, string pattern = "*")
        {
            DataTable data = new DataTable();
            Dictionary<string, string> nodes = new Dictionary<string, string>();
            data = xFiles(nodes, directory, pattern);

            return data;
        }

        public static DataTable Files(string directory, Dictionary<string,string> info, string pattern = "*")
        {
            DataTable data = new DataTable();
            data = xFiles(info, directory, pattern);
            return data;
        }

        private static DataTable xFiles(Dictionary<string,string> nodes, string directory, string pattern = "*")
        {
            DataTable data = new DataTable();
            data.Clear();
            data.Columns.Add("filename");
            data.Columns.Add("filepath");
            data.Columns.Add("title");
            data.Columns.Add("name");
            data.Columns.Add("description");
            data.Columns.Add("entity");
            data.Columns.Add("major");
            data.Columns.Add("minor");
            data.Columns.Add("date");
            data.Columns.Add("bytes");

            foreach (KeyValuePair<string, string> node in nodes)
            {
                data.Columns.Add(node.Key);
            }

            string tmp = "";

            DataRow row = data.NewRow();
            try
            {
                DirectoryInfo d = new DirectoryInfo(directory);
                FileInfo[] files = d.GetFiles(pattern);

                foreach (FileInfo file in files)
                {
                    string contents = ReadFile(file.FullName);
                    row = data.NewRow();
                    row["filename"] = file.Name;
                    row["filepath"] = file.FullName;
                    row["title"] = XNode(ref contents, "title");
                    row["name"] = XNode(ref contents, "name");
                    row["description"] = XNode(ref contents, "description");
                    tmp = XNode(ref contents, "major");
                    if (tmp.IsNullOrEmpty()) { tmp = XNode(ref contents, "majorversion"); }
                    row["major"] = tmp;

                    tmp = XNode(ref contents, "minor");
                    if (tmp.IsNullOrEmpty()) { tmp = XNode(ref contents, "minorversion"); }
                    row["minor"] = tmp;

                    row["entity"] = XNode(ref contents, "entity");
                    tmp = XNode(ref contents, "date");
                    if (tmp == "") { tmp = file.LastWriteTime.ToString(); }
                    row["date"] = tmp;
                    row["bytes"] = file.Length;

                    foreach (KeyValuePair<string, string> node in nodes)
                    {
                        row[node.Key] = XNode(ref contents, node.Key); ;
                    }

                    data.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Error("Files.Exception:" + ex.Message);
            }

            return data;
        }
        /// <summary>
        /// How long since a file was last cached?
        /// </summary>
        /// <param name="filename">name of file to check (with or without extension)</param>
        /// <returns>minutes since last cache</returns>
        /// <remarks></remarks>
        public static TimeSpan TimespanSinceLastCache(string filename)
        {
            TimeSpan returnVal = default(TimeSpan);
            try
            {
                string directory = Directory.GetParent(filename).FullName;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (Directory.Exists(directory))
                {
                    string file = filename;
                    if (!file.Contains("."))
                        file += ".cache";
                    System.IO.FileInfo cache_info = new System.IO.FileInfo(file);
                    returnVal = DateTime.Now.Subtract(cache_info.LastWriteTime);
                }
            }
            catch (Exception ex)
            {
                Error("TimespanSinceLastCache.Exception:" + ex.Message);
            }
            return returnVal;
        }

        //get a block of text from a specifically formated string
        /// <summary>
        /// Return a named subblock in a string delimited by special formatting
        /// </summary>
        /// <param name="source">The block of text containing the target subblock.</param>
        /// <param name="blocktag">The name of the subblock being sought</param>
        /// <returns>String containing the subblock of text, if found.</returns>
        /// <remarks>The format of the sub block tags is the same as a resource file --[TheName]--
        /// This approach is used to externalize long strings and string parts but not compile them into the system. Perhaps not the best 
        /// format choice, but there is a lot of material already out there in this format.
        /// </remarks>
        public static string GetBlock(string source, string blockname)
        {
            string results = "";
            string fulltag = "--[" + blockname + "]--";
            int endidx = 0;
            int startidx = source.IndexOf(fulltag);
            if (startidx > -1)
            {
                startidx = startidx + fulltag.Length;
                endidx = source.IndexOf("--[", startidx);
                if (endidx > startidx)
                {
                    //substring start to end
                    results = source.Substring(startidx, endidx - startidx);
                }
                else
                {
                    results = source.Substring(startidx);
                }
            }
            return results.Trim();
        }

        public static string ReadBlock(string filepath, string blockname)
        {
            string results = "";
            if (FileExists(filepath))
            {
                results = ReadFile(filepath);
                results = GetBlock(results, blockname);
            }
            else
            {
                Error("ReadBlock:File does not exist");
            }
            return results;
        }


        /// <summary>
        /// Remove an item from a dictionary
        /// </summary>
        /// <param name="data">The dictionary reference containing the target key</param>
        /// <param name="key">The key name to remove</param>
        public static void Drop(ref Dictionary<string, string> data, string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }
        }
/*
        public static void UpdateEmpty(ref Dictionary<string, string> data, string key,string value)
        {
            if (data.getValueOrDefault(key).IsNullOrEmpty())
            {
                data.AddOrReplace(key, value);
            }
        }
*/
        /// <summary>
        /// Returns a string value from the passed dictionary based on the key, or returns an empty string if the key is not found.
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <param name="data">The dictionary to search</param>
        /// <returns>String - either the value of the found key, or an empty string if the key is not found.  Examine the property
        /// 'foundit'(T/F) to determine if the key was actually found.</returns>
        public static string GetValue(string key, Dictionary<string, string> data, string defaultifempty = "")
        {
            string results = "";
            if (data.ContainsKey(key))
            {
                results = data[key];
                FoundIt = true;
                if (results.IsNullOrEmpty() && !defaultifempty.IsNullOrEmpty())
                {
                    results = defaultifempty;
                }
            }
            return results;
        }
        /// <summary>
        /// Adds a new key value or replaces an existing key with the passed value
        /// </summary>
        /// <param name="key">The dictionary key to either add or update</param>
        /// <param name="value">The value of the element you want to change or add</param>
        /// <param name="data">a dictionary of strings, passed by reference</param>
        public static void AddOrReplace(string key, string value, ref Dictionary<string, string> data)
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }
        }

        //=============  encoding and decoding ========================
        public static string Base64Encode(string source)
        {
            var sourcebytes = System.Text.Encoding.UTF8.GetBytes(source);
            return System.Convert.ToBase64String(sourcebytes);
        }

        public static string Base64Decode(string base64source)
        {
            var sourcebytes = System.Convert.FromBase64String(base64source);
            return System.Text.Encoding.UTF8.GetString(sourcebytes);
        }

        /// <summary>
        /// Encrypt a string which can later be decrypted using Decrypt
        /// </summary>
        /// <param name="source">The string to encrypt</param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt(string source)
        {

            byte[] clearTextBytes = Encoding.UTF8.GetBytes(source);
            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            MemoryStream encryption = new MemoryStream();
            byte[] salt = Encoding.ASCII.GetBytes(_salt);
            byte[] key = Encoding.ASCII.GetBytes(_encryptionkey);
            CryptoStream s = new CryptoStream(encryption, rijn.CreateEncryptor(key, salt), CryptoStreamMode.Write);
            s.Write(clearTextBytes, 0, clearTextBytes.Length);
            s.Close();
            s.Dispose();
            return Convert.ToBase64String(encryption.ToArray());
        }

        /// <summary>
        /// Decrypt a previously encrypted string.
        /// </summary>
        /// <param name="encryptedtext">The encrypted text to decrypt</param>
        /// <returns>Decrypted text</returns>
        /// <remarks>This method requires the original 16 byte salt and 32 byte key.  If a custom private salt & key was used to encrypt the string, that salt & key must be set before Decrypt will be able to work.</remarks>
        public static string Decrypt(string encryptedtext)
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(encryptedtext);
            MemoryStream decrypt = new MemoryStream();
            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            byte[] salt = Encoding.ASCII.GetBytes(_salt);
            byte[] key = Encoding.ASCII.GetBytes(_encryptionkey);
            CryptoStream cs = new CryptoStream(decrypt, rijn.CreateDecryptor(key, salt), CryptoStreamMode.Write);
            cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
            cs.Close();
            cs.Dispose();
            return Encoding.UTF8.GetString(decrypt.ToArray());

        }

        public static string RandomDigits(int size)
        {

            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 0; i < size; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(Math.Floor(10 * RandomNumber.NextDouble() + 48)));
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string RandomSeed(int size, bool lowercase = false)
        {

            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 0; i < size; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * RandomNumber.NextDouble() + 65)));
                sb.Append(c);
            }
            if (lowercase)
                return sb.ToString().ToLower();
            else
                return sb.ToString();
        }


        public static string ToXML(Dictionary<string, string> data, string NodeWrapper = "")
        {
            string results = "";
            try
            {
                foreach (KeyValuePair<string, string> node in data)
                {
                    Tools.AppendLine(ref results, "\t<" + node.Key.ToLower() + ">" + node.Value + "</" + node.Key.ToLower() + ">");
                }
                if (NodeWrapper.Length > 0)
                {
                    results = "<" + NodeWrapper + ">" + System.Environment.NewLine + results + "</" + NodeWrapper + ">";
                }
            }
            catch (Exception ex)
            {
                Error("ToXML.Exception:" + ex.Message);
            }

            return results;

        }

        public static string ToXML(Dictionary<string, object> data, string NodeWrapper = "")
        {
            string results = "";
            try
            {
                foreach (KeyValuePair<string, object> node in data)
                {
                    Tools.AppendLine(ref results, "\t<" + node.Key.ToLower() + ">" + node.Value + "</" + node.Key.ToLower() + ">");
                }
                if (NodeWrapper.Length > 0)
                {
                    results = "<" + NodeWrapper + ">" + System.Environment.NewLine + results + "</" + NodeWrapper + ">";
                }
            }
            catch (Exception ex)
            {
                Error("ToXML.Exception:" + ex.Message);
            }

            return results;

        }

        public static Dictionary<string, string> ToDictionary(string[] list)
        {
            Dictionary<string, string> hd = new Dictionary<string, string>();

            for (int i = 0; i < list.Length; i++)
            {
                AddOrReplace(list[i], String.Empty, ref hd);
            }
            return hd;
        }

        public static string ToHTMLList(string source, bool orderedlist = false)
        {
            string results = "";
            string tmp = "";
            source = source.Replace("\r", "");
            string[] aM = source.Split('\n');
            for (int i = 0; i < aM.Length; i++)
            {
                tmp = aM[i].Trim();
                if (!tmp.IsNullOrEmpty()) { results += XWrap(aM[i].Trim(), "li"); }
            }

            if (orderedlist)
            {
                results = XWrap(results, "ol");
            }
            else
            {
                results = XWrap(results, "ul");
            }

            return results;

        }
        /// <summary>
        /// Add a CSS class to an element identified by existing class (.class), ID (#id), or an HTML element.
        /// </summary>
        /// <param name="source">The HTML string to insert the class reference into.</param>
        /// <param name="theclass">The class name to insert</param>
        /// <param name="selector">The selector.  Indicate a class with a dot (.class) and an ID with #.</param>
        /// <returns>String modified with the new class is any matching selector is found.</returns>
        /// <remarks>HTML or class selectors will match ALL instances within the source string.  IDs will match only one (there should only be one).</remarks>
        public static string AddClass(string source, string theclass, string selector)
        {

            string results = source;
            string substring = "";
            string pat = "";
            string tmp = "";

            string pattern = "";
            Regex ck = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = ck.Match("");


            if (selector.StartsWith("."))
            {
                //class
                pat = selector.Substring(1);
                pattern = @"class\s*?=\s*?[\""\'].*?" + pat + @".*?[\""\']";
                
                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(results);
                while (match.Success)
                {
                    //Make sure we don't dupe the class.
                    if (!Regex.IsMatch(match.Value, "[\\\" ]+" + theclass + "[\\\" ]+"))
                    {
                        //got it, fix it
                        tmp = match.Value.ToString();
                        tmp = tmp.Replace(pat, pat + " " + theclass);
                        results = results.Replace(match.Value, tmp);
                    }
                    match = match.NextMatch();
                }
            }
            else if (selector.StartsWith("#"))
            {
                //ID
                pat = selector.Substring(1);
                pattern = @"id\s*?=\s*?[\""']" + pat + @"[\""\']";
                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(source);
                if (match.Success)
                {
                    int p = source.IndexOf(match.Value);   //match position...
                    if (p > -1)
                    {
                        tmp = source.Substring(0, p);
                        int s = tmp.LastIndexOf("<");
                        int e = -1;
                        if (s > -1)
                        {
                            e = source.Substring(s).IndexOf(">");
                        }
                        if (s > -1 & e > -1)
                        {
                            //extract just the HTML element with the matched ID...
                            tmp = source.Substring(s, e + 1);
                            pattern = @"class\s*?=\s*?[\""\\'].*?[\""\']";
                            ck = new Regex(pattern, RegexOptions.IgnoreCase);
                            match = ck.Match(tmp);
                            if (match.Success)
                            {
                                //found existing class in substring tmp
                                //Make sure we don't dupe the class.
                                if (!Regex.IsMatch(match.Value, "[\\\" ]+" + theclass + "[\\\" ]+"))
                                {
                                    substring = tmp.Replace(match.Value, match.Value.Substring(0, match.Value.Length - 1) + " " + theclass + match.Value.Substring(match.Value.Length - 1, 1));
                                    results = results.Replace(tmp, substring);
                                }
                            }
                            else
                            {
                                //add a class parameter to element...
                                p = tmp.Length - 1;
                                if (tmp.Substring(p - 1, 1) == "/") { p = p - 1; }
                                substring = tmp.Substring(0, p) + " class=\"" + theclass + "\"" + tmp.Substring(p);
                                results = results.Replace(tmp, substring);
                            }
                        }
                    }
                }

            }
            else
            {
                //HTML element
                int p = 0;
                pattern = @"<" + selector + @".*?>";

                Regex innerck = new Regex(pattern);
                Match m = innerck.Match("");
                ck = new Regex(pattern, RegexOptions.IgnoreCase);
                match = ck.Match(source);
                while (match.Success)
                {
                    //extract just the HTML element...
                    tmp = match.Value;
                    pattern = @"class\s*?=\s*?[\""\\'].*?[\""\']";
                    innerck = new Regex(pattern, RegexOptions.IgnoreCase);
                    m = innerck.Match(tmp);
                    if (m.Success)
                    {
                        //found existing class in substring tmp
                        substring = tmp.Replace(m.Value, m.Value.Substring(0, m.Value.Length - 1) + " " + theclass + m.Value.Substring(m.Value.Length - 1, 1));
                        results = results.Replace(tmp, substring);
                    }
                    else
                    {
                        //add a class parameter to element...
                        p = tmp.Length - 1;
                        if (tmp.Substring(p, 1) == "/") { p = p - 1; }
                        substring = tmp.Substring(0, p) + " class=\"" + theclass + "\"" + tmp.Substring(p);
                        results = results.Replace(tmp, substring);
                    }

                    match = match.NextMatch();
                }
            }
            return results;
        }


        /// <summary>
        /// Quick tool to make string tokens safe for SELECT statements.  This is not a comprehensive injection safe method!
        /// </summary>
        /// <param name="source">Variable string to prep</param>
        /// <returns>String - escaped string.  Do not rely on this for preventing SQL injection!</returns>
        public static string SQLSafe(string source)
        {

            // this is NOT the preferred method by any means.  Parameterized queries are the way to go.
            // this is a hack to work around a couple of limitations.
            source = source.Replace("''", "'");
            source = source.Replace("'", "''");
            source = source.Replace("--", "");
            source = source.Replace(";", "");
            return source;
        }

        public static void AppendLine(ref string source, string newline)
        {
            source = source + newline + System.Environment.NewLine;
        }

        public static List<string> TokenList(string source)
        {
            int i = 0;
            int e = 0;
            string tmp = "";
            List<string> a = new List<string>();
            char[] stoplist = { '!', '@', '#', '$', '@', ' ', '%', '&', '*', '(', ')', '_', '-', '+', '=', ';', ':','"','<','>','?','|' };
            i = source.IndexOf("@");
            while(i>-1)
            {
                e = source.IndexOfAny(stoplist,i+1);
                if(e>-1)
                {
                    tmp = source.Substring(i + 1, e - (i + 1)).Trim();
                }
                else
                {
                    // no end character found...
                    // um... well, unexpected, but a token at the end of the string not unheard of
                    tmp = source.Substring(i + 1, source.Length - (i + 1)).Trim();
                }
                if(!tmp.IsNullOrEmpty()) { a.Add(tmp); }
                tmp = "";
                source = source.Remove(i, 1);
                i = source.IndexOf("@");
            }

            tmp = StringPart(source, "[", "]");
            while (tmp.Length > 0)
            {
                a.Add(tmp);
                source = source.Replace("[" + tmp + "]", "");
                tmp = StringPart(source, "[", "]");
            }
            return a;
        }


        /// <summary>
        /// Compares a List of tokens to the keys of a dictionary, returning True if all keys exist and False if they do not.  Optionally 
        /// also checks to make sure the value of matched keys is also not null or zero length.
        /// </summary>
        /// <param name="hd">The dictionary of key|value pairs</param>
        /// <param name="tokens">A list containing string tokens</param>
        /// <param name="ValueNotZeroLength">Optional boolean, check if the key value is non-null and not zero length.  Default is false.</param>
        /// <returns>Bool</returns>
        public static bool SignatureCompare(Dictionary<string, string> hd, List<string> tokens, bool ValueNotZeroLength = false)
        {
            bool match = true;
            string thistoken = "";
            int i = 0;
            foreach (string token in tokens)
            {
                thistoken = token.ToLower();
                i = token.IndexOf(":");
                if (i > -1)
                {
                    thistoken = token.Substring(0, i);
                }
                if (!hd.ContainsKey(thistoken))
                {
                    match = false;
                    break;
                }
                else if (ValueNotZeroLength)
                {
                    if (string.IsNullOrEmpty(hd[thistoken]))
                    {
                        match = false;
                        break;
                    }
                }
            }
            return match;
        }

        public static string[] SimpleList(string source = "", bool nocache = false)
        {

            string[] a = new string[] { };
            try
            {
                if (source.Length == 0 && !nocache)
                {
                    a = listcache;
                }
                else
                {
                    source = NormalString(source);
                    a = Array.ConvertAll(source.Split(','), p => p.Trim());
                    listcache = a;
                }
            }
            catch (Exception ex)
            {
                Error("SimpleList:" + ex.Message);
            }
            return a;
        }

        //double & triple delimited list items split out
        public static void splitList(string source, ref string key, ref string value, ref string group)
        {
            int i = 0;
            source = source.Trim();
            group = "";
            string[] aM = source.Split(':');
            if (aM.Length > 1)
            {
                key = aM[0].Trim();
                value = aM[1].Trim();
            }
            else
            {
                key = source;
                value = source;
            }

            //now look for triple percision list...
            //generally, the KEY is delimited with a pipe, so
            // key|group:value,
            //but we will accept the value delimited as well.
            i = key.IndexOf("|");
            if (i > 0) // there needs to be a key left, i.e. can't start with a pipe
            {
                //the group is after the pipe
                group = key.Substring(i + 1).Trim();
                key = key.Substring(0, i).Trim();
            }
            else if (value.IndexOf("|") > 0)
            {
                group = value.Substring(i + 1).Trim();
                group = group.Substring(0, i).Trim();
            }
        }
        public static int WordCount(string source = "")
        {
            string[] a = SimpleList(source);
            int i = a.Length;
            a = null;
            return i;
        }

        /// <summary>
        /// Returns the substring of a comma separated list
        /// </summary>
        /// <param name="index">Optiona integer - The 1 based index of the word in the list.  If not supplied, the first word is returned</param>
        /// <param name="source">Optional - list source string.  If not supplied, the previously used list is assumed.</param>
        /// <returns></returns>
        public static string Word(int index = 1, string source = "")
        {
            string results = "";
            index = index - 1;
            string[] a = SimpleList(source);
            int i = a.Length;
            if (index < i)
            {
                results = a[index];
            }
            else
            {
                Message("Word: Index " + (index + 1).ToString() + " out of bounds.");
            }
            a = null;
            return results;
        }

        public static T[] ConcatArrays<T>(params T[][] list)
        {
            var results = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(results, offset);
                offset += list[x].Length;
            }
            return results;
        }

        public static string ValueOf(int index = 1, string source = "")
        {
            string results = "";
            index = index - 1;
            string[] a = SimpleList(source);
            int i = a.Length;
            if (index < i)
            {
                results = a[index];
                i = results.IndexOf(":");
                if (i > -1)
                {
                    results = results.Substring(0, i);
                }
            }
            else
            {
                Message("Word: Index " + (index + 1).ToString() + " out of bounds.");
            }
            a = null;
            return results;
        }
        public static bool IsWord(string word, string source = "")
        {
            bool ok = false;
            string[] a = SimpleList(source);
            for (int i = 0; i < a.Length; i++)
            {
                if (string.Equals(word, a[i], StringComparison.OrdinalIgnoreCase))
                {
                    ok = true;
                    break;
                }
            }
            return ok;
        }
        /// <summary>
        /// Compares list A to list B and returns a list of all items in A not found in B
        /// </summary>
        /// <param name="listA">comma delimited list string</param>
        /// <param name="listB">comma delimited list string</param>
        /// <returns>String - comma delimited string of items</returns>
        public static string CompareList(string listA, string listB, bool ignoreCase = false)
        {

            string results = "";
            bool match = false;
            if (ignoreCase)
            {
                listA = listA.ToLower();
                listB = listB.ToLower();
            }
            string[] aA = listA.Split(',');
            string[] aB = listB.Split(',');

            for (int i = 0; i < aA.Length; i++)
            {
                match = false;
                for (int n = 0; n < aB.Length; n++)
                {
                    if (aA[i].Trim() == aB[n].Trim())
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    results += aA[i].Trim() + ",";
                }
            }
            results = NotEndWith(results, ",");
            return results;
        }
        /// <summary>
        /// Make sure source string does NOT end with a given string/character.
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="endpattern">the pattern to look for</param>
        /// <returns>string</returns>
        public static string NotEndWith(string source, string endpattern)
        {
            string results = source;
            while (results.EndsWith(endpattern))
            {
                results = results.Substring(0, source.Length - endpattern.Length);
            }
            return results;
        }
        public static string EndWith(string source, string endpattern)
        {
            if (source.EndsWith(endpattern))
                return source;
            else
                return source + endpattern;
        }

        public static string NotStartWith(string source, string startpattern)
        {
            string results = source;
            while (results.StartsWith(startpattern))
            {
                results = results.Substring(1);
            }
            return results;
        }

        public static string StartWith(string source, string startpattern)
        {
            if (source.StartsWith(startpattern))
                return source;
            else
                return startpattern + source;
        }

        public static string Q(string source, char quotecharacter = '"')
        {
            return quotecharacter + source + quotecharacter;
        }

        public static string UnQ(string source)
        {
            //remove single or double quotes from beginning and end of string.
            string results = source;
            if (results.StartsWith("\"") && results.EndsWith("\""))
            {
                results = results.Substring(1, results.Length - 2);
            }
            if (results.StartsWith("'") && results.EndsWith("'"))
            {
                results = results.Substring(1, results.Length - 2);
            }

            return results;
        }


        public static string StringOf(string Character, int Count)
        {
            string results = "";
            for (int i = 0; i < Count; i++)
            {
                results += Character;
            }
            return results;
        }

        /// <summary>
        /// Return a portion of a string from the start to a specific substring
        /// </summary>
        /// <param name="source">Substring pattern use as a terminator of the method</param>
        /// <param name="retainPattern">Do you want to retain the pattern in the source?</param>
        /// <param name="ignoreCase">Ignore case of the pattern/source when searching</param>
        /// <returns>String, and modified source</returns>
        public static string StringTo(string source, string pattern, bool ignoreCase = false)
        {
            int p = 0;
            string results = "";

            try
            {
                if (ignoreCase)
                {
                    p = source.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    p = source.IndexOf(pattern);
                }
                if (p == -1)
                {
                    //pattern not found
                    FoundIt = false;
                }
                else
                {
                    FoundIt = true;
                    results = source.Substring(0, p);
                }
            }
            catch (Exception ex)
            {
                Error("StringTo.Exception:" + ex.Message);
            }

            return results;
        }

        /// <summary>
        /// Return a portion of a string from the start to a specific substring
        /// </summary>
        /// <param name="source">Substring pattern use as a terminator of the method</param>
        /// <param name="retainPattern">Do you want to retain the pattern in the source?</param>
        /// <param name="ignoreCase">Ignore case of the pattern/source when searching</param>
        /// <returns>String, and modified source</returns>
        public static string StringTo(ref string source, string pattern, bool retainPattern = false, bool ignoreCase = false)
        {
            int p = 0;
            string results = "";

            try
            {
                if (ignoreCase)
                {
                    p = source.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    p = source.IndexOf(pattern);
                }
                if (p == -1)
                {
                    //pattern not found
                    FoundIt = false;
                }
                else
                {
                    FoundIt = true;
                    results = source.Substring(0, p);
                    if (!retainPattern)
                    {
                        source = source.Substring(p + pattern.Length);
                    }
                    else
                    {
                        source = source.Substring(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Error("StringTo.Exception:" + ex.Message);
            }

            return results;
        }

        /// <summary>
        /// Return a string starting with the end of the passed pattern, if found
        /// </summary>
        /// <param name="source">Source text to search</param>
        /// <param name="pattern">Pattern to find</param>
        /// <param name="retainPattern">Do you want to retain the pattern in the source?</param>
        /// <param name="ignoreCase">Ignore case of the pattern/source when searching</param>
        /// <returns>String and modified source</returns>
        public static string StringFrom(string source, string pattern, bool ignoreCase = false)
        {
            int p = 0;
            string results = "";
            try
            {
                if (ignoreCase)
                {
                    p = source.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    p = source.IndexOf(pattern);
                }
                if (p == -1)
                {
                    //pattern not found
                    FoundIt = false;
                }
                else
                {
                    FoundIt = true;
                    results = source.Substring(p + pattern.Length);
                }
            }
            catch (Exception ex)
            {
                Error("StringFrom.Exception:" + ex.Message);
            }

            return results;
        }
        /// <summary>
        /// Return a string starting with the end of the passed pattern, if found
        /// </summary>
        /// <param name="source">Source text to search</param>
        /// <param name="pattern">Pattern to find</param>
        /// <param name="retainPattern">Do you want to retain the pattern in the source?</param>
        /// <param name="ignoreCase">Ignore case of the pattern/source when searching</param>
        /// <returns>String and modified source</returns>
        public static string StringFrom(ref string source, string pattern, bool retainPattern = false, bool ignoreCase = false)
        {
            int p = 0;
            string results = "";
            try
            {
                if (ignoreCase)
                {
                    p = source.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    p = source.IndexOf(pattern);
                }
                if (p == -1)
                {
                    //pattern not found
                    FoundIt = false;
                }
                else
                {
                    FoundIt = true;
                    results = source.Substring(p + pattern.Length);
                    if (!retainPattern)
                    {
                        source = source.Substring(0, p);
                    }
                }
            }
            catch (Exception ex)
            {
                Error("StringFrom.Exception:" + ex.Message);
            }

            return results;
        }

        /// <summary>
        /// Colapse a string based on string patterns, generally single or double quotes
        /// </summary>
        /// <param name="sText">Source text to collapse</param>
        /// <param name="aZ">String array passed by reference.  This will contain the original string values removed from the source string</param>
        /// <param name="StartPattern">Optional start pattern, default is double quotes</param>
        /// <param name="EndPattern">Optional end pattern, default is double quotes</param>
        /// <returns>String and fills a passed string array</returns>
        /// <remarks>This method is used to quickly parse out blocks of text in the source string to allow for further text processing.  
        /// Generally used to remove string literals before, for example, the string is split on spaces.  Each block is replaced with 
        /// a marker in the form of <[n]> where 'n' is the index of the string in the array passed to the method.  The companion method is 
        /// ExpandString which restores the parsed out literals.
        /// </remarks>
        public static string CollapseString(string sText, ref string[] aZ, string StartPattern = "\"", string EndPattern = "\"")
        {

            //Array aZ that is passed to the function is appended to by redim
            //we find all instances of strings beginning with StartPattern and 
            //ending with EndPattern, we tokenize them in the form <[n]>
            //where n=the index of the array and store the original in the 
            //array for later re-insertion.

            try
            {
                int p = 0;
                //pointer
                int e = 0;
                int x = 0;
                string substr = "";

                p = sText.IndexOf(StartPattern, StringComparison.CurrentCultureIgnoreCase);
                while (!(p == -1))
                {
                    //we've found a starting match
                    if (p < sText.Length)
                    {
                        e = sText.IndexOf(EndPattern, p + 1, StringComparison.CurrentCultureIgnoreCase);
                        if (e > 0)
                        {
                            //end point found
                            //take the whold pattern, including the start and end strings
                            substr = sText.Substring(p, (e + EndPattern.Length) - p);
                            //this substring should not be a self reference.
                            //lets check that out.
                            if (substr.StartsWith(StartPattern + "<[") && substr.EndsWith("]>" + EndPattern))
                            {
                                //humm, not liking the looks of this, but to 
                                //be sure, we need to check out the inside value
                                //skip it if we find and existing embed marker
                                if (Tools.isNumeric(substr.Substring(StartPattern.Length + 2, substr.Length - (EndPattern.Length + (StartPattern.Length + 2 + 2)))))
                                {
                                    e = p + substr.Length;
                                    substr = "";
                                }
                            }

                            if (substr.Length > 0)
                            {
                                x = aZ.Length;
                                Array.Resize(ref aZ, x + 1);
                                aZ[x] = substr;
                                sText = sText.Replace(substr, "<[" + x + "]>");
                                //move the next search position to the last 
                                //start position + the length of the replace token (about)
                                e = p + 5;
                            }

                            if (e > sText.Length)
                            {
                                p = 0;
                            }
                            else
                            {
                                p = sText.IndexOf(StartPattern, StringComparison.CurrentCultureIgnoreCase);
                            }
                        }
                        else
                        {
                            //e is not found.  No end pattern, get out of here
                            break;
                        }
                    }
                    else
                    {
                        p = 0;
                    }
                }

            }
            catch (Exception ex)
            {
                Error("CollapseString:" + ex.Message);
            }
            return sText;
        }

        public static string ExpandString(string sText, string[] aZ, bool removeDoubleQuote = false, int StripForeAft = 0)
        {

            if ((aZ == null))
                return sText;
            int x = 0;
            StringBuilder sb = new StringBuilder();

            sb.Append(sText);

            for (x = 1; x < aZ.Length; x++)
            {
                if (removeDoubleQuote)
                    aZ[x] = aZ[x].Replace("\"", "");

                if (StripForeAft > 0)
                {
                    try
                    {
                        aZ[x] = aZ[x].Substring(StripForeAft, aZ[x].Length - StripForeAft * 2);
                    }
                    catch (Exception ex)
                    {
                        Message("ExpandString.StripForeAft:" + ex.Message + " on string " + aZ[x]);
                    }

                }
                sb.Replace("<[" + x + "]>", aZ[x]);
            }

            return sb.ToString();

        }

        /// <summary>
        /// Replaces tokens in source with the values found in the data dictionary
        /// </summary>
        /// <param name="source">Source text to perform the replacement on.  Contains 0+ tokens [key]</param>
        /// <param name="data">Dictionary containing replacement value where the tokens in source = [key] in dictionary</param>
        /// <returns>string</returns>
        public static string Merge(string source, Dictionary<string, string> data)
        {

            string results = source;
            string key = "";
            string value = "";

            foreach (KeyValuePair<string, string> item in data)
            {
                key = item.Key.ToLower();
                value = URLString(item.Value);
                results = results.Replace("[" + key + "]", value);
            }

            return results;
        }

        /// <summary>
        /// Clean up strings by removing extra spaces and padding key values as necessary for processing
        /// </summary>
        /// <param name="theString">The string to normalize</param>
        /// <returns>Normalized string</returns>
        public static string NormalString(string theString)
        {

            theString = theString.Trim();
            theString = theString.Replace("=", " = ");
            theString = theString.Replace("   ", " ");
            theString = theString.Replace("  ", " ");
            theString = theString.Replace("  ", " ");
            theString = theString.Replace("  ", " ");
            theString = theString.Replace(",, ", ",");
            theString = theString.Replace("\t", "");
            theString = theString.Replace("\n", "");
            theString = theString.Replace("\r", "");

            return theString;

        }

        public static string CleanString(string theString)
        {
            theString = URLString(theString);
            theString = NormalString(theString);
            return theString;
        }
        public static string StringPart(string sText, string strStart, string strEnd, bool Inclusive = false)
        {
            string functionReturnValue = null;
            //returns a sub string between strStart and strEnd from within sText

            int p = 0;
            int e = 0;
            string substr = "";
            int x = strStart.Length;
            functionReturnValue = "";

            if (x == 0)
                return functionReturnValue;

            p = sText.IndexOf(strStart, 0, StringComparison.CurrentCultureIgnoreCase);
            if (p > -1)
            {
                e = sText.IndexOf(strEnd, p + strStart.Length, StringComparison.CurrentCultureIgnoreCase);
                if (e > 0)
                {
                    //found it.  Yippie.....
                    substr = sText.Substring(p + x, e - (p + x));
                    if (substr.Length == 0)
                        substr = " ";
                }
                else
                {
                    //end not found, run to the end of the string
                    substr = sText.Substring(p + x);
                    if (substr.Length == 0)
                        substr = " ";
                }
                if (Inclusive)
                {
                    functionReturnValue = strStart + substr + strEnd;
                }
                else
                {
                    functionReturnValue = substr;
                }
            }
            return functionReturnValue;

        }

        public static string StringPart(int Index, string sText, string strStart, string strEnd, bool Inclusive = false)
        {
            string functionReturnValue = null;
            //returns a sub string between strStart and strEnd from within sText
            //where Index is a character position within the sub string
            functionReturnValue = "";

            int p = 0;
            int e = 0;
            string substr = "";
            //first, get the start position which is somewhere before
            //Index in the passed string.
            for (p = Index; p >= -1; p += -1)
            {
                try
                {
                    if (sText.Substring(p).StartsWith(strStart, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //swell, we found the starting position.
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Error("StringPart:" + ex.Message);
                    break;
                }
            }
            if (p == -1)
                return functionReturnValue;
            int x = strStart.Length;

            if (x == 0)
                return functionReturnValue;

            e = sText.IndexOf(strEnd, p + strStart.Length, StringComparison.CurrentCultureIgnoreCase);
            if (e > 0)
            {
                //found it.  Yippie.....
                substr = sText.Substring(p + x, e - (p + x));
            }
            else
            {
                substr = sText.Substring(p + x);
            }
            if (substr.Length == 0)
                substr = " ";
            if (Inclusive)
            {
                functionReturnValue = strStart + substr + strEnd;
            }
            else
            {
                functionReturnValue = substr;
            }
            return functionReturnValue;

        }


        //public static string Criteria(string select, string fields,string searchterm)
        //{
        //    string results = "";
        //    string tmp = "";

        //    tmp = "select 'MRN' as cat,p.* from pt_master p WHERE ";
        //    for (int i = 0; i < terms.Length; i++)
        //    {
        //        if (modifiers[i] == "word" || modifiers[i] == "alphanumeric" || modifiers[i] == "number")
        //        {
        //            tmp += "\tUPPER(mrn) like '%" + searchitems[i] + "%' OR";
        //        }
        //    }
        //    tmp = tmp.Trim();
        //    tmp = Tools.NotEndWith(tmp, "OR");
        //    tmp = "(" + tmp + ")" + Environment.NewLine + "UNION ALL ";
        //    sql = sql + tmp + Environment.NewLine;




        //    return results;
        //}



        /// <summary>
        /// Set querystring as the querystring portion of the source, and context as everything before the querystring
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="context">Resulting context, everything before the querystring, by reference</param>
        /// <param name="querystring">the querystring portion of source</param>
        public static void QSSplit(ref string source, ref string context, ref string querystring)
        {
            int i = source.IndexOf("?");
            if (i == -1)
            {
                context = source;
            }
            else
            {
                context = source.Substring(0, i);
                querystring = source.Substring(i + 1);
            }
        }
        public static void QSParse(ref Dictionary<string, string> hd, string querystring)
        {
            string[] aZ = querystring.Split('&');
            string[] kv;
            string tmp = "";
            try
            {
                for (int i = 0; i < aZ.Length; i++)
                {
                    kv = aZ[i].Split('=');
                    if (kv.Length == 1)
                    {
                        tmp = Uri.UnescapeDataString(kv[0]);
                        if (hd.ContainsKey(tmp.ToLower()))
                        {
                            hd[tmp.ToLower()] = tmp;
                        }
                        else
                        {
                            hd.Add(tmp.ToLower(), tmp);
                        }
                    }
                    else
                    {
                        if (kv.Length > 1)
                        {

                            tmp = Uri.UnescapeDataString(kv[0]);
                            if (hd.ContainsKey(tmp.ToLower()))
                            {
                                hd[tmp.ToLower()] = Uri.UnescapeDataString(kv[1]);
                            }
                            else
                            {
                                hd.Add(tmp.ToLower(), Uri.UnescapeDataString(kv[1]));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error("QueryString.Parse:" + ex.Message);
            }
        }

        public static Dictionary<string, string> QSParse(string querystring)
        {
            Dictionary<string, string> hd = new Dictionary<string, string>();
            string[] aZ = querystring.Split('&');
            string[] kv;

            try
            {
                for (int i = 0; i < aZ.Length; i++)
                {
                    kv = aZ[i].Split('=');
                    if (kv.Length == 1)
                    {
                        hd.Add(Uri.UnescapeDataString(kv[0]), Uri.UnescapeDataString(kv[0]));
                    }
                    else
                    {
                        if (kv.Length > 1)
                        {
                            hd.Add(Uri.UnescapeDataString(kv[0]), Uri.UnescapeDataString(kv[1]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error("QueryString.Parse:" + ex.Message);
            }

            return hd;
        }

        public static string QSSerialize(Dictionary<string, string> Data)
        {
            string results = "";

            foreach (KeyValuePair<string, string> item in Data)
            {
                results += "&" + item.Key + "=" + item.Value;
            }
            if (results.Length > 0)
            {
                return results.Substring(1);
            }
            else
            {
                return "";
            }
        }

        public static string ToJSON(Dictionary<string, string> Data)
        {
            StringBuilder sb = new StringBuilder();
            string results = "";
            foreach (KeyValuePair<string, string> item in Data)
            {
                if (isNumeric(item.Value))
                {
                    sb.Append(Q(item.Key) + " : " + item.Value + "," + System.Environment.NewLine);
                }
                else
                {
                    sb.Append(Q(item.Key) + " : " + Q(escapeJSON(item.Value)) + "," + System.Environment.NewLine);
                }
            }
            results = sb.ToString().Trim();
            if (results.EndsWith(",")) { results = results.Substring(0, results.Length - 1); }

            return "{" + System.Environment.NewLine + results + System.Environment.NewLine + "}";
        }
        public static string escapeJSON(string source)
        {
            source = source.Replace(@"""", @"\""");
            source = source.Replace(@"\", @"\\");
            return source;
        }

        // ====== NEW METHODS for VERSION 2 MODIFICATIONS =======
        /// <summary>
        /// Return the array item index or the modulas of index and length of source
        /// </summary>
        /// <param name="source">The string array to select items from</param>
        /// <param name="index">An integer representing an array index.  This index can be greater than the length of the array.</param>
        /// <returns>String, a single item from an array of strings</returns>
        /// <remarks>Given an array length of 3 items and an index of 4, array item 1 is returned as 4 MOD 3 = 1.</remarks>
        public static string SelectMOD(string[] source, int index)
        {
            string results = "";
            int i = index % source.Length;
            results = source[i];
            return results;
        }
        /// <summary>
        /// Select the item from the source string array indicated by index
        /// </summary>
        /// <param name="source">String array</param>
        /// <param name="index">Integer index to select</param>
        /// <param name="defaultvalue">Default string to return if the array is shorter than the index</param>
        /// <returns>String</returns>
        public static string SelectDefault(string[] source, int index, string defaultvalue)
        {
            string results = "";
            if (index < source.Length)
            {
                results = source[index];
            }
            else
            {
                results = defaultvalue;
            }
            return results;
        }

        /// <summary>
        /// Select the item from the source string array indicated by index
        /// </summary>
        /// <param name="source">String array</param>
        /// <param name="index">Integer index to select</param>
        /// <param name="defaultvalue">Default index from the array to return if index exceeds the length of source.  Also see SelectMOD for iterative index selection.</param>
        /// <returns>String</returns>
        public static string SelectDefault(string[] source, int index, int defaultindex)
        {
            string results = "";
            if (index < source.Length)
            {
                results = source[index];
            }
            else
            {
                if (defaultindex < source.Length)
                {
                    results = source[defaultindex];
                }
                else
                {
                    Message("SelectDefault: Default index exceeds length of source array");
                    results = "";
                }
            }
            return results;
        }

        /// <summary>
        /// Replace in a given string a sub string starting with strStart and ending with strEnd
        /// </summary>
        /// <param name="sText">Source text to modify</param>
        /// <param name="strStart">String to match as the starting point for the replacement</param>
        /// <param name="strEnd">String to match as the ending point for the replacement</param>
        /// <param name="strReplace">String to replace any match with.</param>
        /// <param name="MaxLength">Optional integer indicating the maximum length of the resulting substring to replace.</param>
        /// <param name="DidWeFindIt">Optional boolean by reference.  If you pass in a boolean variable, it will be set to True if any match is found, false if not.</param>
        /// <returns>String with replaced text</returns>
        /// <remarks>Replace ALL matches within the source string.</remarks>
        public static string ReplaceRange(string sText, string strStart, string strEnd, string strReplace = "", int MaxLength = 0, bool SingleInstance = false)
        {
            FoundIt = false;
            int s = 0;
            int e = 0;
            string tag = "";

            s = sText.IndexOf(strStart);
            while (!(s == -1))
            {
                e = sText.IndexOf(strEnd, s + 1);
                if (e > -1)
                {
                    e = (e + strEnd.Length) - s;
                    if (e > 0 && (MaxLength == 0 | (e <= MaxLength)))
                    {
                        tag = sText.Substring(s, e);
                        sText = sText.Replace(tag, strReplace);
                        FoundIt = true;
                        s = sText.IndexOf(strStart, s + strReplace.Length);
                        if (SingleInstance) { s = -1; }
                    }
                    else
                    {
                        //we are not replacing the tag
                        //so let's not find it again.
                        s = sText.IndexOf(strStart,s+1);
                    }
                }
                else
                {
                    s = -1;
                }
            }
            return sText;
        }
        /// <summary>
        /// Wrap a string in an XML node
        /// </summary>
        /// <param name="source">The string to wrap</param>
        /// <param name="tag">the node name of "tag" to wrap the string in, without the open/close markers - string name only</param>
        /// <param name="Neat">If true, return it all on a single line</param>
        /// <returns>String</returns>
        public static string XWrap(string source, string tag, bool Neat = false)
        {
            string linemark = System.Environment.NewLine;
            if (Neat) { linemark = ""; }

            string results = source;
            results = NotEndWith(results, System.Environment.NewLine);
            results = results.Trim();
            if (!tag.IsNullOrEmpty())
            {
                results = "<" + tag + ">" + linemark
                        + results
                        + linemark
                        + "</" + tag + ">";
            }
            return results;
        }


        public static int Bigger(int val1, int val2)
        {
            int results = val1;
            if (val2 > val1) { results = val2; }
            return results;
        }

        public static int Smaller(int val1, int val2)
        {
            int results = val1;
            if (val2 < val1) { results = val2; }
            return results;
        }


        public static string Attribute(string attribute_name, string attribute_value)
        {

            if (attribute_value.IsNullOrEmpty())
            {
                return "";
            }
            else
            {
                return " " + attribute_name + " = " + Q(attribute_value);
            }
        }
        /// <summary>
        /// Returns the inner contents of a referenced XML node in the passed string block.
        /// </summary>
        /// <param name="source">Text|HTML|XML block as a string</param>
        /// <param name="tag">The XML node name.</param>
        /// <param name="RetainTag">Optional boolean.  Default is false, indicating that the source string is consumed, i.e. the node is removed from the source.
        /// True indicates that the source string is not consumed and the node remains in the source.</param>
        /// <returns>String</returns>
        /// <remarks>LucyXML is completely string based, ignores schema, and does not support node attributes.  This makes it exceptionally fast. 
        /// It consumes the source string, returning the tag and modifying the source (optionally, the node can be retained).  This method always returns the first
        /// instance of the named node (tag) in the source string.</remarks>
        public static string XNode(ref string source, string tag, bool RetainTag = false)
        {
            //return the contents of the specified XML node/tag

            int s = 0;
            int e = 0;
            int i = 0;

            string strReturn = "";
            string strFullTag = "";
            NodeFound = false;
            tag = tag.ToLower();

            s = source.IndexOf("<" + tag + ">", StringComparison.CurrentCultureIgnoreCase);

            if (s > -1)
            {
                try
                {
                    e = source.IndexOf("</" + tag + ">", s + tag.Length, StringComparison.CurrentCultureIgnoreCase);
                }
                catch
                {
                    e = source.Length - 1;
                    ResultComment = "NET";   // "No End Tag" 
                }
                if (e > -1)
                {
                    i = e + tag.Length + 1;
                    strReturn = source.Substring(s + tag.Length + 2, (e - 1) - (s + tag.Length + 1));
                    NodeFound = true;
                    if (!RetainTag)
                    {
                        strFullTag = source.Substring(s, (e + (tag.Length + 3)) - s);
                        source = source.Replace(strFullTag, "");
                    }
                }
            }
            return cdata(strReturn);
        }

        private static string cdata(string source)
        {
            source = source.Trim();
            if (source.StartsWith("<![CDATA["))
            {
                source = source.Substring(9);
                if (source.EndsWith("]]>"))
                {
                    source = source.Substring(0, source.Length - 3);
                }
            }
            return source;
        }
        public static string NextNode(string source, ref int position)
        {
            int s = 0;
            int e = 0;

            string results = "";
            s = source.IndexOf("<", position);
            if (s > -1)
            {
                e = source.IndexOf(">", s);
                if (e > -1)
                {
                    position = s;
                    results = source.Substring(s + 1, e - (s + 1));
                }

            }
            return results;
        }
        /// <summary>
        /// Returns a string of the length specified, if the original string is longer than the specified length, otherwise, returns
        /// a white-space trimmed version of the source string.
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="Length">The returned length</param>
        /// <returns>String</returns>
        public static string TrimTo(string source, int Length)
        {
            source = source.Trim();
            if (source.Length > Length) { source = source.Substring(0, Length).Trim(); }
            return source;
        }
        /// <summary>
        /// Cleans a string of non-URL friendly characters
        /// </summary>
        /// <param name="thetext">The string you want to clean</param>
        /// <returns>String with offending characters removed.</returns>
        public static string URLString(string thetext,string badchars = "")
        {
            string results = thetext;
            results = results.Replace("\n", "");
            results = results.Replace("\r", "");
            results = results.Replace("\t", "");

            if(badchars == "") { badchars =  "~`!@#$%^&*()={}[]|\\/<>.,'\""; }

            for (int i = 0; i < badchars.Length; i++)
            {
                results = results.Replace(badchars.Substring(i, 1), "");
            }
            //deal with double spaces...
            results = results.Replace("  ", " ");
            //and because they can be compound...
            results = results.Replace("  ", " ");
            //and now replace space with dash
            results = results.Replace(" ", "-");
            results = results.Replace("--", "-");
            results = results.Replace("--", "-");

            return results;
        }


        /// <summary>
        /// Returns a date in the format of YYYYMMDD, url safe
        /// </summary>
        /// <param name="date">the date to reformat</param>
        /// <returns>string - the formatted date or and empty string if date is not a valid date</returns>
        public static string toURLDate(object date)
        {
            DateTime? dt = AsDateDate(date);
            if (dt == null)
            {
                return "";
            }
            return AsDate(date.ToString(), "yyyyMMdd");
        }


        public static string fromURLDate(string date)
        {
            string results = "";
            if (date.Length == 8 && isDigits(date))
            {
                int year = AsInteger(date.Substring(0, 4));
                int month = AsInteger(date.Substring(4, 2));
                int day = AsInteger(date.Substring(6, 2));
                results = month + "/" + day + "/" + year;
            }

            return results;
        }


        /// <summary>
        /// Returns a proper case (first letter of each word capitalized)
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns>String - first letter of each word is capitalized, except where the word was all caps in the source (indicating an anacronym) or where
        /// a letter follows a single quote or a dash (which will also be capitalized)</returns>
        public static string ProperCase(string source, bool isname = false)
        {
            if (source.IsNullOrEmpty()) { return ""; }

            int p = 0;
            string results = "";
            source = source.Replace("  ", " ").Trim();
            string[] aM = source.Split(' ');
            for (int i = 0; i < aM.Length; i++)
            {
                if (!isUpperCase(aM[i]) || isname)
                {
                    aM[i] = aM[i].Substring(0, 1).ToUpper() + aM[i].Substring(1, aM[i].Length - 1).ToLower();
                }
                results += aM[i] + " ";
            }

            p = results.IndexOf('-');
            while (p > -1)
            {
                try
                {
                    results = results.Substring(0, p + 1) + results.Substring(p + 1, 1).ToUpper() + results.Substring(p + 2);
                }
                catch
                { }  //nothing to see here
                p = results.IndexOf('-', p + 1);
            }

            p = results.IndexOf('\'');
            while (p > -1)
            {
                try
                {
                    results = results.Substring(0, p + 1) + results.Substring(p + 1, 1).ToUpper() + results.Substring(p + 2);
                }
                catch
                { }  //nothing to see here
                p = results.IndexOf('\'', p + 1);
            }
            return results.Trim();
        }



        public static bool isUpperCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsLower(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool isLowerCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLower(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool isDigits(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsDigit(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool isLettersOrDigits(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLetterOrDigit(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        /// <summary>
        /// Examine if the source string contains only letters
        /// </summary>
        /// <param name="source">string to examine</param>
        /// <returns>Boolean</returns>
        public static bool hasLetters(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsLetter(source[i]))
                {
                    return true;
                }
            }
            return results;
        }
        /// <summary>
        /// Examine if the source string contains only letters
        /// </summary>
        /// <param name="source">string to examine</param>
        /// <returns>Boolean</returns>
        public static bool isLetters(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsLetter(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool isPunctuation(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            bool results = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsPunctuation(source[i]))
                {
                    results = false;
                    break;
                }
            }
            return results;
        }

        public static bool hasUpperCase(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsUpper(source[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool hasPunctuation(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsPunctuation(source[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if all characters in source are also in master
        /// </summary>
        /// <param name="master">master pool of characters as a string</param>
        /// <param name="source">string you want to compare</param>
        /// <returns>Boolean - If all characters in source are also in master, return True, otherwise return false.</returns>
        public static bool Intersect(string master, string source, bool IgnoreCase = false)
        {
            int p = 0;

            if (IgnoreCase)
            {
                master = master.ToLower();
                source = source.ToLower();
            }
            for (int i = 0; i < source.Length; i++)
            {
                p = master.IndexOf(source.Substring(i, 1));
                if (p == -1)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Compare two dates (as strings)
        /// </summary>
        /// <param name="date1">first date to compare as a string</param>
        /// <param name="date2">second date to compare as a string</param>
        /// <returns>-1 if Date1 is earlier than Date2, 0 if Date1 is equal to Date2, and 1 if Date1 later than Date2.  Returns -3 if Date1 is invalid or null and -2 if Date2 is invalid or null</returns>
        public static int CompareDate(string date1, string date2)
        {

            DateTime d1;
            DateTime d2;
            int results = -9;

            try
            {
                d1 = DateTime.Parse(date1);
                try
                {
                    d2 = DateTime.Parse(date2);
                    //still here?  We have two valid dates...
                    if (d1 == d2) { results = 0; }
                    if (d1 < d2) { results = -1; }
                    if (d1 > d2) { results = 1; }
                }
                catch
                {
                    //d2 is null/invalid, but d1 is valid
                    results = -2;
                }
            }
            catch
            {
                //d1 is null/invalid
                results = -3;
            }

            return results;

        }


        /// <summary>
        /// Examines an object and returns True if the object is numeric, False if it is not.
        /// </summary>
        /// <param name="thevalue"></param>
        /// <returns></returns>
        public static bool isNumeric(object thevalue)
        {
            float result;
            return float.TryParse(Convert.ToString(thevalue), out result);
        }

        //-----------------------------------------------------------------
        //              CAST HANDLERS
        //-------------------------------------------------------------


        /// <summary>
        /// Cast an object as a double
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Double - Any non-numeric value will return 0</returns>
        public static double AsDouble(object thevalue)
        {
            double results = 0;
            try
            {
                results = Convert.ToDouble(thevalue);
            }
            catch
            {
                results = 0;
            }
            return results;

        }

        /// <summary>
        /// Cast an object as a decimal
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Decimal - Any non-numeric value will return 0</returns>
        public static decimal AsDecimal(object thevalue)
        {
            decimal results = 0;
            try
            {
                results = Convert.ToDecimal(thevalue);
            }
            catch
            {
                results = 0;
            }
            return results;

        }

        /// <summary>
        /// Cast an object as a float
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Double - Any non-numeric value will return 0</returns>
        public static float AsFloat(object thevalue)
        {
            float results = 0;
            try
            {
                results = (float)thevalue;
            }
            catch
            {
                results = 0;
            }
            return results;

        }


        public static bool isOdd(int value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// Cast an object to boolean
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Boolean</returns>
        /// <remarks>Values of '1'|1, 'y', 'yes','true' (case insensitive) return True. A boolean True returns true.  All other values return False</remarks>
        public static bool AsBoolean(object thevalue)
        {
            bool functionReturnValue = false;

            if (thevalue == null) { thevalue = ""; }
            if (thevalue.ToString().Length == 0)
            {
                functionReturnValue = false;
            }
            else
            {
                switch (thevalue.ToString().ToLower())
                {
                    case "1":
                    case "y":
                    case "yes":
                    case "true":
                        functionReturnValue = true;
                        break;
                    case "no":
                    case "false":
                        functionReturnValue = false;
                        break;
                    default:
                        try
                        {
                            functionReturnValue = Convert.ToBoolean(thevalue);
                        }
                        catch
                        {
                            functionReturnValue = false;
                        }
                        break;
                }

            }
            return functionReturnValue;

        }



        /// <summary>
        /// Forces and explicit cast of a string to a date and either returns a date as a string or returns an empty string.
        /// </summary>
        /// <param name="thedate">String date</param>
        /// <param name="formatstring">Optional string specifying the format for the returned date string.  Default is 'dd/MM/yyyy HH:mm:ss'</param>
        /// <returns>Either a confirmed date/time as a string or an empty string if the original parameter failed explicit date conversion.</returns>
        public static string AsDate(string thedate, string formatstring = "")
        {
            string results = "";
            try
            {
                var dt = DateTime.Parse(thedate);
                if(!formatstring.IsNullOrEmpty())
                {
                    results = dt.ToString(formatstring);
                }
                else
                {
                    results = dt.ToString();
                }
                return results;
            }
            catch
            {
                //not a date, return empty string, to be handled up stream...
            }

            return "";
        }

        /// <summary>
        /// Returns a date as a formatted string, returns an empty string if the formatstring is invalid.
        /// </summary>
        /// <param name="thedate">String date</param>
        /// <param name="formatstring">Optional string specifying the format for the returned date string.  Default is 'dd/MM/yyyy HH:mm:ss'</param>
        /// <returns>Either a confirmed date/time as a string or an empty string if the original parameter failed explicit date conversion.</returns>
        public static string AsDate(DateTime thedate, string formatstring = "")
        {

            if (formatstring.Length == 0)
            {
                formatstring = "MM/dd/yyyy";
            }

            try
            {
                return thedate.ToString(formatstring);
            }
            catch
            {
                //not a valid format string, return empty string, to be handled up stream...
            }

            return "";
        }

        /// <summary>
        /// Returns a date and time as a formatted string, returns an empty string if the formatstring is invalid.
        /// </summary>
        /// <param name="thedate">String date</param>
        /// <param name="formatstring">Optional string specifying the format for the returned date string.  Default is 'dd/MM/yyyy HH:mm:ss'</param>
        /// <returns>Either a confirmed date/time as a string or an empty string if the original parameter failed explicit date conversion. Note that the format string may result in a date excluding time if the format string so specifies</returns>
        public static string AsDateTime(DateTime thedate, string formatstring = "")
        {

            if (formatstring.Length == 0)
            {
                formatstring = "MM/dd/yyyy hh:mm:ss tt";
            }

            try
            {
                return thedate.ToString(formatstring);
            }
            catch
            {
                //not a valid format string, return empty string, to be handled up stream...
            }

            return "";
        }

        /// <summary>
        /// Casts and object to a DateTime or null if the object is not a date.
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>DateTime or null</returns>
        public static DateTime? AsDateDate(object thevalue)
        {
            DateTime? dt = null;
            try
            {
                dt = DateTime.Parse(thevalue.ToString());
            }
            catch
            {
                //it will just return null
            }

            return dt;

        }


        public static string DateStamp()
        {
            string results = AsDate(DateTime.Now.ToString(), "yyyyMMddhhmmss");
            return results;
        }
        /// <summary>
        /// Casts an object to an integer(32)
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Integer (Int32)</returns>
        /// <remarks>Any non-numeric value returns 0</remarks>
        public static int AsInteger(object thevalue)
        {
            int results = 0;
            if (thevalue == null) { thevalue = 0; }
            if (thevalue.ToString().Length == 0)
            {
                results = 0;
            }
            else
            {
                try
                {
                    results = Convert.ToInt32(thevalue);
                }
                catch
                {
                    results = 0;
                }
            }
            return results;

        }

        /// <summary>
        /// Casts an object to a long integer(64)
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>Integer (Int64)</returns>
        /// <remarks>Any non-numeric value returns 0</remarks>
        public static long AsLong(object thevalue)
        {
            long results = 0;
            try
            {
                results = Convert.ToInt64(thevalue);
            }
            catch
            {
                results = 0;
            }
            return results;

        }

        /// <summary>
        /// Casts an object to a string
        /// </summary>
        /// <param name="thevalue">The object to cast</param>
        /// <returns>String - works much like .ToString() except handles null references, returning an empty string.</returns>
        public static string AsText(object thevalue)
        {
            if ((thevalue == null))
            {
                return "";
            }
            else if (thevalue == null)
            {
                return "";
            }
            else
            {
                try
                {
                    return Convert.ToString(thevalue);
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Examines an object and returns True if the object is a date and False if the object is not a date
        /// </summary>
        /// <param name="thedate">The object to examine</param>
        /// <returns>Boolean</returns>
        public static bool IsDate(object thedate)
        {
            bool OKdate = true;
            try
            {
                DateTime dt = DateTime.Parse(thedate.ToString());
            }
            catch
            {
                OKdate = false;
            }

            return OKdate;
        }

        public static string YearsAgo(double years)
        {
            string results = "";

            double days = years*365.2;
            double months = days/30.43;

            int y = AsInteger(Math.Truncate(months/12));
            int m = AsInteger(Math.Truncate((months - (y * 12)+0.25)));
            // because of rounding, above, we might have 12 months...
            if(m==12)
            {
                y++;
                m = 0;
            }
            int d = AsInteger(days);

            string ys = "";
            string ms = "";
            string ds = "";

            if(y==1) { ys = "1 year"; }
            if (y > 1) { ys = y.ToString() + " years"; }

            if (m == 1) { ms = "1 month"; }
            if (m > 1) { ms = m.ToString() + " months"; }

            if (d == 1) { ds = "~1 day"; }
            if (d > 1) { ds = d.ToString() + " days"; }
            if (d < 1) { ds = "<1 day"; }


            results = (ys + " " + ms).Trim();
            if (results.IsNullOrEmpty()) { results = ds; }
            return results.Trim();
        }
        public static string Ago(string thedate)
        {
            string results = "";
            try
            {
                DateTime dt = DateTime.Parse(thedate.ToString());
                TimeSpan ts = (DateTime.Now - dt);
                int totaldays = ts.Days;
                int i = 0;
                int y = 0;
                if (totaldays > 30)
                {
                    i = AsInteger((totaldays / 30) + .45);
                    if (i == 1)
                    {
                        results = "1 month ago";
                    }
                    else
                    {
                        if (i == 12)
                        { results = "1 year ago"; }
                        else
                        {
                            if (i > 12)
                            {
                                y = Tools.AsInteger(i / 12);
                                i = i - (y * 12);
                                if (y == 1 && i == 1)
                                {
                                    results = "1 year 1 month ago";
                                }
                                else
                                {
                                    if (y == 1)
                                    {
                                        results = "1 year " + i.ToString() + " months ago";
                                    }
                                    else
                                    {
                                        results = y.ToString() + " years " + i.ToString() + " months ago";
                                    }
                                }
                            }
                            else
                            {
                                results = i.ToString() + " months ago";
                            }

                        }
                        
                    }
                }
                else if (totaldays > 7)
                {
                    i = AsInteger((totaldays / 7) + .45);
                    if (i == 1)
                    {
                        results = "1 week ago";
                    }
                    else
                    {
                        results = i.ToString() + " weeks ago";
                    }
                }
                else if (ts.Days > 0)
                {
                    if (ts.Days == 1)
                    {
                        results = "1 day ago";
                    }
                    else
                    {
                        results = ts.Days.ToString() + " days ago";
                    }
                }
                else if (ts.Hours > 0)
                {
                    if (ts.Hours == 1)
                    {
                        results = "1 hour ago";
                    }
                    else
                    {
                        results = ts.Hours.ToString() + " hours ago";
                    }
                }
                else
                {
                    if (ts.Minutes <= 1)
                    {
                        results = "about 1 minute ago";
                    }
                    else
                    {
                        results = ts.Minutes.ToString() + " minutes ago";
                    }
                }
            }
            catch
            {
                results = "unknown";
            }
            return results;
        }

        /// <summary>
        /// Returns a string of bit values, 1,2,4,8,16, etc. based on the integer value provided
        /// </summary>
        /// <param name="value">Integer value to evaluate. Max integer value 2,147,483,647 results in a max 31 bits returned</param>
        /// <returns>String list</returns>
        public static string BitList(int value)
        {
            string results = "";
            
            int keys = value;
            int test = 0;

            for (int i = 0; i < 31; i++)
            {
                test = Convert.ToInt32(Math.Pow(2.0, Convert.ToDouble(i)));

                if ((test & keys) == test)
                {
                    results += test.ToString() + ",";
                }
            }
            results = NotEndWith(results, ",");
            return results;
        }

        // ====================  Random Data =========================

        public string NextColor()
        {
            RED = Math.Abs(RED + 4) % 220;
            GREEN = (Math.Abs(GREEN - 16) % 180) + 60;
            BLUE = (Math.Abs(BLUE + 24) % 180) + 40;
            return (RED.ToString("X") + GREEN.ToString("X") + BLUE.ToString("X") + "00000F").Substring(0, 6);

        }

        public static string Girl()
        {
            string results = "";
            string[] pool = "Abbey,Abigail,Alexia,Allison,Alycia,Amanda,Amy,Angela,Anna,Anne,Annette,Annie,Arabella,Ariel,Arya,Ashley,Audrey,Autumn,Ava,Avery,Ayla,Ayo,Bea,Beatrice,Beth,Bethany,Blake,Bonnie,Bonny,Bree,Brenda,Bridget,Brittany,Brooke,Caitlin,Carissa,Carla,Carley,Carmen,Carol,Carolina,Caroline,Carolyn,Casandra,Casey,Casey Ann,Catalina,Catherine,Cecily,Chase,Cheryl,Chloe,Christina,Christine,Christy,Chrystal,Cindy,Claire,Clara,Clare,Claudia,Constance,Cora,Courtney,Daisy,Dakota,Daphne,Darci,Darla,Darlene,Dawn,Deanna,Deborah,Diana,Diane,Doreen,Doris,Dorothy,Eda,Elaine,Elana,Eleanor,Elizabeth,Ellie,Elyse,Elyssa,Emily,Emma,Emmie,Emmy,Eren,Erica,Estelle,Esther,Eva,Eve,Evelyn,Faith,Felicity,Finnley,Fiona,Gabriella,Gabrielle,Geena,Gertrude,Gianna,Gillian,Gina,Glenda,Gloria,Grace,Gracie,Greer,Greta,Gretchen,Gwen,Gwendolyn,Gwenyth,Hailey,Halley,Hannah,Hayley,Hazel,Heather,Heidi,Helen,Helena,Hilary,Inga,Ingrid,Irene,Iris,Isabel,Isabela,Isabell,Isabella,Isabelle,Isadora,Ivy,Jacquelin,Jacqueline,Jan,Jana,Jane,Janet,Jayne,Jenet,Jenice,Jenna,Jennifer,Jenny,Jessica,Jilian,Joannie,Jodi,Jody,Josephine,Joy,Jude,Judith,Judy,Julia,Juliana,Julianna,Julianne,Julie,Julieanna,Julienne,Juliet,June,Justine,Kacie,Kara,Karen,Kari,Kate,Kathleen,Kathy,Katie,Kayden,Kayla,Kellianne,Kelsey,Kendall,Kerri,Kiley,Kimberley,Kira,Krista,Kristen,Kristi,Kushi,Kylee,Lacey,Laura,Laurel,Lauren,Lea,Leah,Leigh,Leila,Leona,Leora,Leslie,Lexi,Li,Lia,Lianna,Lilah,Lillian,Lillyana,Lily,Linda,Lindsay,Lola,Loren,Loretta,Lu,Luciana,Lucie,Lucille,Lucy,Lydia,Lyla,Lynden,Lyndsay,Lyndsey,Lynn,Lynne,Mabel,Macy,Maddie,Maddison,Madelaine,Madeleine,Madeline,Madison,Mae,Maggie,Makena,Maleah,Malia,Malika,Mallory,Maori,Mara,Margaret,Margo,Margot,Mariam,Marian,Marie,Marilyn,Marissa,Martha,Mary,Maryanne,Marybeth,Mattie,Maud,Maura,Maureen,May,Mea,Meagan,Meaghan,Meckenzie,Meg,Megan,Meghan,Melanie,Melinda,Melissa,Melodie,Mercedes,Meredith,Merilyn,Merissa,Mia,Michelle,Mira,Miriam,Moira,Molly,Mona,Monica,Monique,Mora,Morgan,Nadia,Nancy,Naomi,Natalie,Nathalie,Nelle,Neve,Nichole,Nicole,Nikki,Nina,Noelle,Noemi,Nora,Noreen,Norma,Olga,Olive,Olivia,Paige,Pamela,Patricia,Paula,Paulina,Phoebe,Piper,Priscilla,Priya,Puja,Quinn,Rachael,Rachel,Rebecca,Reese,Reilly,Reily,Renee,Romy,Rory,Rosa,Rose,Roselyn,Rosemarie,Rosemary,Rosie,Roslyn,Rossana,Ruby,Ruth,Rylee,Ryleigh,Sabrina,Sadie,Sally,Samantha,Sara,Sarah,Sasha,Savanna,Savannah,Scarlett,Shannon,Shayla,Shayna,Sheri,Sheryl,Shirley,Simenesh,Simone,Skye,Sofia,Sonja,Sonya,Sophia,Sophie,Stacie,Stacy,Stella,Stephanie,Summer,Susan,Susana,Suzanne,Sydney,Sylvia,Tabitha,Tammi,Tanya,Tara,Tegan,Terri,Tess,Tessa,Theresa,Tifanny,Tiffany,Tina,Tracey,Tracy,Tricia,Uma,Vanessa,Velma,Vilmarie,Violet,Virginia,Vivian,Wan,Wilhemina,Willa,Willow,Yvette,Zoe,Zoeanne,Zoey".Split(',');
            int i = RandomNumber.Next(0, pool.Length);
            results = pool[i];
            pool = null;
            return results;
        }
        public static string Boy()
        {
            string results = "";
            string[] pool = "Aaron,Abraham,Abram,Adam,Adrian,Alan,Albert,Alec,Alex,Alexander,Alfonso,Alfred,Allan,Allen,Andre,Andrew,Andy,Anthony,Antoine,Archer,Arthur,Asher,Ashton,Ben,Bentley,Bernard,Billy,Bobby,Boris,Brendan,Brett,Brian,Brice,Brock,Bruce,Bruno,Bryan,Bryant,Cam,Carl,Carter,Charles,Charlie,Chris,Clarence,Clark,Clint,Cody,Colby,Cole,Conrad,Craig,Cruz,Dale,Dalton,Dan,Daniel,Danny,Darren,Dave,David,Dean,Dennis,Derek,Desmond,Dillon,Domingo,Donald,Douglas,Duke,Duncan,Dustin,Dwight,Dylan,Eddie,Edgar,Edmund,Eduardo,Edward,Elijah,Elliot,Elliott,Emanuel,Emerson,Emery,Emmett,Eric,Erickson,Ernie,Errol,Ethan,Eugene,Everett,Fahad,Faisal,Felipe,Felix,Fernando,Finley,Finn,Fletcher,Flynn,Forest,Francis,Franco,Frank,Frankie,Franklin,Franz,Fraser,Fray,Fred,Gabriel,Garret,Garrett,Garrison,Garry,Gary,Gavin,Gentz,Geoffrey,George,Gerald,Gerard,Gideon,Gilbert,Glen,Glenn,Gordon,Grady,Graham,Grant,Grayson,Gregory,Griffin,Grover,Gunnar,Gus,Hans,Harold,Harper,Harrison,Harry,Hayden,Hector,Henry,Herbert,Herman,Howard,Hugh,Hugo,Hunter,Ian,Isaac,Jack,Jackson,Jacob,James,Jarred,Jason,Jasper,Jay,Jeffrey,Jerome,Jerry,Jesse,Jim,Jimmy,Joel,John,Johnny,Jonathan,Jorge,Jose,Joseph,Joshua,Juan,Jude,Justin,Keith,Ken,Kenneth,Kenny,Kevin,Kyle,Lance,Larry,Lawrence,Lee,Lenny,Leo,Leonard,Leroy,Logan,Louis,Lucas,Luis,Luke,Manuel,Marc,Marcos,Marcus,Mario,Mark,Marshall,Martin,Marvin,Matthew,Max,Melvin,Michael,Miguel,Miles,Milo,Milton,Mitch,Muhammad,Murphy,Nathan,Neal,Ned,Neil,Nicolas,Nigel,Noel,Nolan,Oliver,Orlando,Oscar,Otis,Owen,Parker,Pascal,Patrick,Paul,Pedro,Perry,Peter,Philip,Pierre,Preston,Randy,Raphael,Ray,Raymond,Reid,Remington,Renaldo,Rhys,Ricardo,Richard,Rick,Ricky,Riley,Robert,Roberto,Roger,Ron,Rowan,Roy,Ruben,Ryan,Salvador,Salvatore,Sam,Samuel,Santiago,Saul,Sawyer,Scott,Seamus,Sean,Serge,Seth,Shaun,Shawn,Sheldon,Sidney,Simon,Snider,Spencer,Stanley,Stephan,Stephen,Steve,Steven,Stewart,Taylor,Terrence,Terry,Theo,Theodore,Thomas,Tim,Timothy,Tobias,Toby,Todd,Tomas,Travis,Trent,Trevor,Tristan,Truman,Tucker,Tyler,Tyrone,Vance,Victor,Walter,Wayne,Wesley,Wilfred,Will,William,Wyatt,Xavier,Yong,Yoseff,Yung,Yuri,Zachary".Split(',');
            int i = RandomNumber.Next(0, pool.Length);
            results = pool[i];
            pool = null;
            return results;
        }

        public static string LastName()
        {
            string results = "";
            string[] pool = "Abbott,Abdella,Abdullah,Abear,Abel,Abraham,Ackley,Acorn,Acquah,Adam,Adamchek,Adams,Adkins,Adler,Agar,Akers,Alba,Albano,Alberto,Albertson,Alden,Alexander,Alexis,Alford,Alfred,Alfredo,Allman,Allo,Ames,Andrews,Applbaum,Ashton,Ashwell,Atherton,Auerbach,Austin,Avery,Axford,Ayer,Ayers,Babbin,Babcock,Bace,Bachand,Bachrach,Backus,Baer,Baez,Bagge,Bagley,Bailey,Bain,Baird,Baker,Balch,Ball,Balling,Balter,Bamford,Bancroft,Banks,Banning,Bannon,Banville,Barber,Barclay,Barker,Barlow,Barnes,Barnett,Baron,Barr,Barrack,Barrett,Barrie,Barrios,Barron,Barros,Barrow,Barrows,Barry,Bartlett,Barton,Baskin,Baslow,Basque,Bass,Bassett,Bates,Battcock,Battles,Batts,Bauch,Baudouin,Bauer,Bauermeister,Baughman,Baum,Bauman,Baumer,Baxley,Baxter,Bayer,Beach,Beader,Beal,Beals,Beaman,Beams,Bean,Beane,Beard,Beardsworth,Beaton,Beatrice,Beatty,Beaupre,Beauregard,Bechard,Becht,Beck,Becker,Beckerman,Beckford,Beckham,Beckles,Beckman,Beevers,Begley,Bell,Belleville,Belmonte,Belsie,Bender,Benderson,Benedict,Benger,Benham,Benjamin,Bennett,Benoit,Benson,Bent,Bentley,Benton,Berger,Bergeron,Bergman,Bergus,Berk,Bernard,Berry,Betts,Bickford,Bickmore,Bigelow,Billings,Bingham,Bioski,Bird,Bishop,Bisson,Black,Blackburn,Blackman,Blake,Blakely,Blanchard,Blank,Bloom,Bloomfield,Blum,Blumberg,Blunt,Boardman,Bobbitt,Bogdanovitch,Bogrett,Boise,Bolton,Booker,Boone,Booth,Borg,Boscoe,Bouchard,Bourne,Bowe,Bowen,Bower,Bowers,Bowler,Bowling,Bowman,Bradlee,Bradley,Bradshaw,Brady,Brandt,Braun,Breen,Brennan,Brenner,Breslin,Brewer,Brewster,Briggs,Brill,Brimmer,Britt,Brock,Brodie,Brooke,Brooks,Brown,Browne,Brunell,Bryan,Bryant,Buchanan,Buchwald,Buckley,Bugler,Bunker,Bunsoeung,Burgess,Burke,Burns,Butcher,Butler,Cabot,Cadet,Cadman,Caezza,Cafazzo,Cafferky,Cahalin,Cahill,Cahoon,Cain,Calder,Calderon,Caldwell,Calhoun,Callaghan,Calvert,Calvin,Cambra,Cameron,Camillo,Campbell,Canfield,Canning,Cannon,Cantor,Cantwell,Capers,Caplin,Capone,Caponigro,Capozzi,Cappello,Capuano,Caraway,Carbone,Card,Carey,Carlson,Carmin,Carney,Carr,Carrier,Carrol,Carroll,Carson,Carter,Cartier,Cartwright,Carver,Casey,Cassell,Cassidy,Castillo,Castor,Catania,Catovic,Cavanaugh,Cederholm,Chadwick,Chaffee,Chamberlain,Chambers,Chandler,Chapman,Charleston,Cheney,Cheng,Chhuon,Childs,Cho,Choi,Chow,Chuang,Chung,Churchill,Cioffi,Civetti,Clancy,Clapp,Cleary,Clemens,Clement,Clifford,Cloutier,Cloutman,Cobb,Cobert,Coen,Coffey,Colasanti,Colbert,Colburn,Colby,Cole,Coleman,Collins,Combs,Compton,Conley,Connelly,Connor,Connors,Conolly,Consoli,Contillo,Conway,Coogan,Cook,Cooley,Coolidge,Cooper,Copeland,Copley,Corbett,Cormier,Cornell,Cortez,Cortina,Cosgrove,Costa,Cotes,Cottingham,Cotton,Covino,Cox,Coyle,Crandall,Cranson,Craven,Crawford,Crawley,Crocker,Crockett,Cromwell,Crooks,Crowley,Cruz,Cullen,Culpepper,Curiel,Curley,Curran,Currier,Curry,Curtin,Cushing,Cutter,Dacosta,Dailey,Dalton,Damato,Daniels,Danielson,Darcy,Davenport,Davis,Dawkins,Dawson,Decker,Deckert,Deegan,Deforest,Deforge,Defrancesco,Defranco,Delaney,Delarosa,Delgado,Delorenzo,Dempsey,Dentler,Desmond,Desousa,Desouza,Dewhurst,Dexter,Diaz,Dickerson,Diezel,Dimarco,Dingle,Diver,Dixon,Dobbs,Dobson,Dodd,Dodds,Dollard,Donnelly,Donoghue,Donohue,Donovan,Dooley,Dorr,Doubleday,Dougherty,Douglas,Dowd,Downing,Doyle,Drake,Draper,Driscoll,Dubinsky,Duchemin,Dudley,Duff,Duffy,Duke,Dumet,Dumont,Dunbury,Duncan,Dunham,Duprey,Duquette,Dutton,Duval,Dwyer,Eagen,Eagerman,Earley,Eason,Eastwood,Eaton,Eckhart,Edmundson,Ellenwood,Emery,Epstein,Espinosa,Esposito,Fanning,Farmer,Farrow,Felder,Ferguson,Fielding,Finley,Fisher,Fitzgerald,Fitzpatrick,Fitzwilliam,Fleming,Fletcher,Flint,Flowers,Foley,Fontaine,Forrester,Fowler,Fraser,Fredericks,Freeman,French,Friedman,Frost,Fryer,Gonzales,Goodman,Grafton,Graves,Gray,Greeley,Greenberg,Greene,Greenwood,Greer,Groves,Guevara,Guzman,Hansen,Harding,Harrington,Harris,Harrison,Haskell,Hastings,Hathaway,Hendrickson,Hernandez,Herrington,Hills,Hoffman,Holbrook,Holcomb,Hollander,Holm,Holman,Holzman,Hooks,Hopkins,Hopper,Hotchkiss,Howell,Hoyt,Hu,Hudson,Hughes,Hunt,Hurley,Ibrahim,Jackson,Jacobs,Jacobsen,Jacobson,Jaffe,Jankowski,Jarvis,Jefferson,Jennings,Jiang,Johnston,Jung,Juster,Keating,Kellogg,Kelly,Kim,Kirchner,Kline,Knapp,Knell,Krueger,Kwok,Kwong,Lake,Lamb,Lambert,Lancaster,Landau,Lander,Landis,Lane,Lawler,Lawrence,Lawson,Layman,Leach,Leary,Leland,Lincoln,Lindberg,Lindquist,Linton,Locke,Lockhart,Lockwood,Long,Lovell,Lush,Lyons,MacDonald,Macmillan,Mahony,Mallard,Mansfield,Marino,Markham,Martinez,Masters,Mateo,Mathews,Mattheson,Maurice,Mauricio,Maxwell,May,Mayberry,Mayer,Mayhew,McCulloch,McCutcheon,McDermott,McDonald,McDonnell,McLoughlin,McMullen,McNair,McNamara,McNeill,McNerney,McNulty,McPhee,McPherson,McQuade,Merrill,Merritt,Meyer,Meyers,Mickelson,Middleton,Miller,Mills,Moffitt,Montague,Montoya,Morgan,Mosley,Moyer,Moynihan,Mullen,Munroe,Murdoch,Murdock,Murphy,Myers,Nadir,Newbury,Nicholson,Nolan,Norbert,Norton,Nottingham,Noyes,Nye,O'Brien,Olsen,Olson,O'Neill,Osborne,O'Sullivan,O'Toole,Ouellet,Owens,Packard,Parsley,Patterson,Paulson,Perkins,Phelps,Phillips,Pickering,Pollack,Price,Purcell,Putnam,Rabinowitz,Rafferty,Reinhardt,Renfrow,Ricardo,Richards,Richardson,Rind,Robertson,Robins,Robinson,Roche,Rockwell,Rodgers,Rodriguez,Rogers,Romano,Rooney,Root,Roper,Rosales,Rosario,Rossetti,Rossini,Rudd,Russell,Russo,Rutherford,Rutland,Rutledge,Salazar,Sampson,Sanborn,Sanford,Santana,Santiago,Santo,Saunders,Sawyer,Scarborough,Schafer,Schmidt,Schubert,Seaberg,Segundo,Seidel,Seton,Sewell,Sexton,Shaffer,Shapiro,Shaughnessy,Shaw,Shea,Shear,Sheffield,Shelton,Shepard,Shockley,Silva,Silverbrand,Silverman,Skinner,Smith,Sosa,Spence,Spencer,Stackhouse,Stafford,Stanton,Stapleton,Stark,Stellman,Stephenson,Stockton,Stockwell,Stroman,Strongman,Styles,Sullivan,Summers,Sung,Supple,Sutherland,Sutter,Sutton,Swain,Swanson,Swartz,Sweeney,Swenson,Swift,Swindlehurst,Tabb,Taber,Tabor,Taft,Tam,Tate,Taylor,Terrell,Thompson,Thomson,Thornton,Thorpe,Thurman,Thurston,Tompkins,Torres,Townsend,Tran,Travers,Trudeau,Truman,Tu,Tucker,Turner,Tyler,Ulrich,Vance,Vasquez,Ventura,Vernon,Vicente,Vigorito,Vila,Villeneuve,Voltaire,Wager,Waldorf,Wallace,Wallenstein,Walters,Wang,Warwick,Washburn,Watson,Weathers,Weaver,Webb,Webber,Webster,Weinberg,Welsh,Werner,Wescott,Westland,Weston,Wharton,Wheeler,Whipple,Whitaker,Whitcomb,White,Whitlock,Whitman,Whitney,Whittaker,Whittier,Whittington,Whittle,Wickett,Wicks,Wiggins,Wight,Wilburn,Wilcox,Wilde,Wiley,Wilkins,Wilkinson,Williams,Williamson,Willis,Willoughby,Wills,Wilson,Winchell,Winchester,Winder,Winship,Winslow,Winsor,Winters,Winthrop,Witt,Witts,Wolfe,Wolfson,Wolowicz,Wolverton,Wong,Woodbury,Woodman,Woods,Woolard,Woolbert,Woolf,Workman,Wright,Wu,Wyman,Wynn,Xavier,Yale,Yang,Yankow,Yates,Ye,Yin,York,Yost,Yotts,Young,Younger,Zheng,Zhong,Zhou,Zhu,Zick,Zito,Zwicker".Split(',');
            int i = RandomNumber.Next(0, pool.Length);
            results = pool[i];
            pool = null;
            return results;
        }
        //---------------------------------------------------------

        public static bool ValidateMailFormat(string sEmail)
        {

            Regex mailCK = new Regex("\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
            Match mat = mailCK.Match(sEmail);
            if (mat.Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
