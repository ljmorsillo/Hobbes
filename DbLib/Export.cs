using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
// using System.Data.EntityClient;
// using System.Data.Metadata.Edm;
// using System.Data.Objects;
// using System.Data.Mapping;
// using System.Data.Common;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;

namespace Scamps
{
    public static class qXMLextension
    {
        public static qXML WriteStartDoc(this qXML w)
        {
            w.WriteStartDocument();
            return w;
        }
        public static qXML WriteStartEle(this qXML w, string name)
        {
            w.WriteStartElement(name);
            return w;
        }
        public static qXML WriteEndEle(this qXML w)
        {
            w.WriteEndElement();
            return w;
        }
        public static qXML WriteEleVal(this qXML w, string name, string val)
        {
            w.WriteElementValue(name, val);
            return w;
        }
        public static qXML WriteIFEleVal(this qXML w, bool condition, string name, string val)
        {
            if (condition)
                return w.WriteEleVal(name, val);
            return w;
        }
    }

    public class qXML
    {
        Stack<string> lastEle;
        StreamWriter w;

        public qXML(StreamWriter t)
        {
            lastEle = new Stack<string>();
            this.w = t;
        }

        public void WriteStartDocument()
        {
            w.Write(string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
            w.WriteLine();
        }

        public void WriteStartElement(string e)
        {
            for (var i = 0; i < lastEle.Count; i++)
                w.Write("\t");
            lastEle.Push(e);
            w.Write(string.Concat("<", e, ">"));
            w.WriteLine();
        }
        public void WriteEndElement()
        {
            var str = lastEle.Pop();
            for (var i = 0; i < lastEle.Count; i++)
                w.Write("\t");
            w.Write(string.Concat("</", str, ">"));
            w.WriteLine();
        }
        public void WriteElementValue(string name, string val)
        {
            for (var i = 0; i < lastEle.Count; i++)
                w.Write("\t");
            w.Write(string.Concat("<", name, ">", System.Security.SecurityElement.Escape(val), "</", name, ">"));
            w.WriteLine();
        }
        public void Close()
        {
            w.Close();
        }
    }

    public static class Exports
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in col.AllKeys)
            {
                dict.Add(k, col[k]);
            }
            return dict;
        }

        public static string getValueOrDefault(this IDictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return null;
        }

        public static T getValueOrOriginal<T>(this IDictionary<string, string> dict, string key, T startingValue)
        {
            if (dict.ContainsKey(key))
            {
                //TODO: Revisit this. (null)/Default values/etc. http://stackoverflow.com/questions/3531318/convert-changetype-fails-on-nullable-types
                var coerced = (T)Convert.ChangeType(dict[key], Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
                return coerced;
            }
            else
                return startingValue;
        }

        public static void TryDelegate(Action func, Action<Exception> fail)
        {
            try{
                func();
            }
            catch(Exception ex)
            {
                fail(ex);
            }
        }
    } 
}
