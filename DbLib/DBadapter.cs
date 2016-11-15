using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace Scamps
{
    /// <summary>
    /// TypeConversion is used to either implicitly or explicitly convert a type to another
    /// </summary>
    public static class TypeConversion
    {
        //typecode definitions
        public static readonly Dictionary<Type, int> TypeDict = new Dictionary<Type, int>(){
            {typeof(sbyte),1},
            {typeof(byte),2},
            {typeof(short),4},
            {typeof(ushort),8},
            {typeof(int),16},
            {typeof(uint),32},
            {typeof(long),64},
            {typeof(char),128},
            {typeof(float),256},
            {typeof(ulong),512},
            {typeof(decimal),1024}
        };

        //translation dictionary for determining what source type (key) implicitly converts to another type.
        // key is bitwise sum (e.g. type 1 has convertible sum 3412 -- this means it is implicitly convertable to 4(short), 16(int), 64(long), 256(float), and 512(ulong))
        public static readonly Dictionary<int, int> ImplicitConv = new Dictionary<int, int>(){
            {1,3412},
            {2,3964},
            {4,3408},
            {8,3952},
            {16,3392},
            {32,3904},
            {64,3328},
            {128,3960},
            {256,2048},
            {512,3328}
        };
        //translation dictionary for explicit conversion
        public static readonly Dictionary<int, int> ExplicitConv = new Dictionary<int, int>()
        {
            {1,682},
            {2,129},
            {4,683},
            {8,135},
            {16,687},
            {32,159},
            {64,703},
            {512,255},
            {128,7},
            {256,1791},
            {2048,2047},
            {1024,3071}
        };

        /// <summary>
        /// Determines if a type is implicitly convertable to another.
        /// e.g. an int is implicitly convertable to another int, short, long, float, ...
        /// </summary>
        /// <param name="source">type to implicitly convert</param>
        /// <param name="Target">type to convert to</param>
        /// <returns>True if implicitly convertable</returns>
        public static bool isImplicitlyConvertable(Type source, Type Target)
        {
            if (!TypeDict.ContainsKey(source) || !TypeDict.ContainsKey(Target))
                return false;

            var scode = TypeDict[source];
            var tcode = TypeDict[Target];

            if (ImplicitConv.ContainsKey(tcode) && ((ImplicitConv[tcode] & scode) == scode))
                return true;
            return false;
        }

        /// <summary>
        /// Determines if a type is explicitly convertable to another.
        /// </summary>
        /// <param name="source">type to explicitly convert</param>
        /// <param name="Target">type to convert to</param>
        /// <returns>True if explicitly convertable</returns>
        public static bool isExplicitlyConvertable(Type source, Type Target)
        {
            if (!TypeDict.ContainsKey(source) || !TypeDict.ContainsKey(Target))
                return false;

            var scode = TypeDict[source];
            var tcode = TypeDict[Target];

            if (ExplicitConv.ContainsKey(tcode) && ((ExplicitConv[tcode] & scode) == scode))
                return true;
            return false;
        }
    }

    /// <summary>
    /// DBadapter is a generic database provider which can be used to instantiate DB connections via DbProviderFactory
    /// </summary>
    public class DBadapter : IDisposable
    {
        private static ConnectionStringSettings defaultConnectionSettings = null;
        private bool disposed = false;
        private DbProviderFactory dbfactory;
        private DbConnection conn;

        private int _xrowpointer;
        private bool _xEOF;

        public DataTable ResultSet;

        public int rowcount
        {
            get
            { return (ResultSet == null) ? 0 : ResultSet.Rows.Count; }
        }

        public int colcount
        {
            get
            { return (ResultSet == null) ? 0 : ResultSet.Columns.Count; }
        }

        public DBadapter()
        {
            if (defaultConnectionSettings == null)
                defaultConnectionSettings = ConfigurationManager.ConnectionStrings["SCAMPs"];
            this.dbfactory = DbProviderFactories.GetFactory(defaultConnectionSettings.ProviderName);
            this.conn = dbfactory.CreateConnection();
            this.conn.ConnectionString = defaultConnectionSettings.ConnectionString;
        }
        /// <summary>
        /// Initialize a db connection given a web/app.config connectionstringsetting object
        /// </summary>
        /// <param name="cs">connectionstringsetting object which contains a connection string and provider invariant name</param>
        public DBadapter(ConnectionStringSettings cs)
        {
            this.dbfactory = DbProviderFactories.GetFactory(cs.ProviderName);
            this.conn = dbfactory.CreateConnection();
            this.conn.ConnectionString = cs.ConnectionString;
        }

        public void Open()
        {
            this.conn.Open();
        }

        private void OpenIfClosed()
        {
            if (this.conn != null && this.conn.State == ConnectionState.Closed)
                this.conn.Open();
        }

        public DbDataReader getDataReader(string SQLStatement) { return getDataReader(SQLStatement, null); }
        public DbDataReader getDataReader(string SQLStatement, IEnumerable<object> ObjectCollection)
        {
            DbDataReader reader = null;

            OpenIfClosed();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SQLStatement;

                if (ObjectCollection != null && ObjectCollection.Count() > 0)
                {
                    foreach (var obj in ObjectCollection)
                    {
                        var param = cmd.CreateParameter();
                        param.Value = obj;
                        cmd.Parameters.Add(param);
                    }
                }

                reader = cmd.ExecuteReader();
            }

            return reader;
        }

        #region DataTableMethods
        public void GetResultSet(string SQLStatement) { GetResultSet(SQLStatement, null); }
        public void GetResultSet(string SQLStatement, params object[] parameters)
        {
            if (ResultSet == null)
                ResultSet = new DataTable();
            else
                ResultSet.Clear();

            var dbAdapter = dbfactory.CreateDataAdapter();

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLStatement;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        var currParam = cmd.CreateParameter();
                        currParam.Value = p;
                        cmd.Parameters.Add(currParam);
                    }
                }

                dbAdapter.SelectCommand = cmd;
                dbAdapter.Fill(ResultSet);
                this._xrowpointer = 0;
                this._xEOF = (ResultSet == null || ResultSet.Rows.Count == 0);
            }
        }

        /// <summary>
        /// Return the current ResultSet row as a dictionary of string values.
        /// </summary>
        /// <returns>Dictionary string,string</returns>
        public Dictionary<string, string> GetRowStrings()
        {
            var data = new Dictionary<string, string>();
            if (this.ResultSet == null || ResultSet.Rows.Count == 0)
                return data;
            foreach (var i in Enumerable.Range(0, this.ResultSet.Columns.Count))
                data.Add(this.ResultSet.Columns[i].ColumnName.ToLower(), this.ResultSet.Rows[this._xrowpointer].ItemArray[i].ToString());
            return data;
        }

        public Dictionary<string,object> GetRow(){
            var data = new Dictionary<string, object>();

            if (ResultSet.Rows.Count == 0)
                return data;
            foreach (var i in Enumerable.Range(0, ResultSet.Columns.Count))
                data.Add(ResultSet.Columns[i].ColumnName.ToLower(), ResultSet.Rows[_xrowpointer].ItemArray[i]);
            return data;
        }
        public void NextRow()
        {
            if (_xrowpointer < ResultSet.Rows.Count - 1)
            {
                _xrowpointer++;
            }
            else
            {
                _xEOF = true;
            }
        }

        public void PreviousRow()
        {
            if (_xrowpointer > 0)
            {
                _xrowpointer--;
            }
            else
            {
                _xEOF = true;
            }
        }

        public int RowPointer
        {
            get
            {
                return this._xrowpointer;
            }
        }

        public bool EOF
        {
            get
            {
                return this._xEOF;
            }
        }

        #endregion

        //public int ExecuteNonQuery(string SQLstatement) { return ExecuteNonQuery(SQLstatement, null); }

        public int ExecuteNonQuery(string SQLstatement, Dictionary<string, object> paramDict)
        {
            int affected = 0;

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLstatement;
                foreach (var kv in paramDict){
                    var param = cmd.CreateParameter();
                    param.ParameterName = kv.Key;
                    param.Value = kv.Value;
                    cmd.Parameters.Add(param);
                }

                affected = cmd.ExecuteNonQuery();
            }

            return affected;
        }
        
        /*
        public int ExecuteNonQuery(string SQLstatement, params object[] parameters)
        {
            int affected = 0;
            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLstatement;

                if (parameters != null && parameters.Length > 0){
                    foreach (var p in parameters){
                        var po = cmd.CreateParameter();
                        po.Value = p;
                        cmd.Parameters.Add(po);
                    }
                }

                affected = cmd.ExecuteNonQuery();
            }
            return affected;

        }
         */

        public int BulkQuery(string SQLstatement, IDataReader dbReader)
        {
            int affected = 0;

            if (string.IsNullOrEmpty(SQLstatement) || dbReader == null || !dbReader.Read())
                return affected;

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLstatement;

                do
                {
                    if (cmd.Parameters.Count == 0 || dbReader.FieldCount != cmd.Parameters.Count)
                    {
                        cmd.Parameters.Clear();
                        for (int i = 0; i < dbReader.FieldCount; i++)
                            cmd.Parameters.Add(cmd.CreateParameter());
                    }

                    for (int i = 0; i < cmd.Parameters.Count; i++)
                        cmd.Parameters[i].Value = dbReader.GetValue(i);

                    affected += cmd.ExecuteNonQuery();

                } while (dbReader.Read());

            }
            return affected;
        }

        /// <summary>
        /// executes a parameterized sql statement for each set of enumerable objects
        /// </summary>
        /// <param name="SQLstatement">parameterized sql statement to execute</param>
        /// <param name="ObjectCollections">collection which provides the parameters for each execution</param>
        /// <returns>records affected</returns>
        public int BulkQuery(string SQLstatement, IEnumerable<IEnumerable<object>> ObjectCollections)
        {
            int affected = 0;
            if (string.IsNullOrEmpty(SQLstatement) || ObjectCollections == null)
                return affected;

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLstatement;

                foreach (var iobj in ObjectCollections)
                {
                    if (cmd.Parameters.Count == 0 || ObjectCollections.Count() != cmd.Parameters.Count)
                    {
                        cmd.Parameters.Clear();
                        for (int i = 0; i < ObjectCollections.Count(); i++)
                            cmd.Parameters.Add(cmd.CreateParameter());
                    }

                    int idx = 0;
                    foreach (var o in iobj)
                    {
                        cmd.Parameters[idx].Value = o;
                        idx++;
                    }

                    affected += cmd.ExecuteNonQuery();
                }
            }

            return affected;
        }

        /// <summary>
        /// Corner case for an an oracle provider is used, determining a primary key is different.
        /// </summary>
        /// <param name="tableName">tablename to check</param>
        /// <returns>column name of primary key</returns>
        private string _oracleIdentityColumn(string tableName)
        {
            var key = string.Empty;
            var indexName = string.Empty;

            var pkeyrestrictions = new string[3];
            var indexrestrictions = new string[4];
            pkeyrestrictions[1] = tableName;

            //get identity column in oracle, primary key has to be cross referenced with the index name .. which isn't a table's column name
            OpenIfClosed();
                
            var dtable = this.conn.GetSchema("PrimaryKeys", pkeyrestrictions);
            if (dtable.Rows.Count > 0)
            {
                //index name is at ordinality of 15 of PrimaryKeys Schema
                indexName = dtable.Rows[0][15].ToString();
            }

            indexrestrictions[1] = indexName;
            indexrestrictions[3] = tableName;
            dtable = this.conn.GetSchema("IndexColumns", indexrestrictions);
            //name of primary key column at ordinality of 4 of IndexColumns Schema
            key = dtable.Rows[0][4].ToString();

            return key;

        }

        /// <summary>
        /// get column name of a primary key given a table name
        /// </summary>
        /// <param name="tableName">tablename to check</param>
        /// <returns>column name</returns>
        public string IdentityColumn(string tableName)
        {
            if (this.conn.GetType().ToString().IndexOf("oracle", StringComparison.OrdinalIgnoreCase) != -1)
                return _oracleIdentityColumn(tableName);

            var key = string.Empty;

            OpenIfClosed();

            var cmd = this.conn.CreateCommand();

            var restrictions = new string[4];
            restrictions[3] = tableName;

            var dtable = this.conn.GetSchema("IndexColumns", restrictions);
            var keycol = dtable.PrimaryKey.FirstOrDefault();
            if (keycol != null)
                key = keycol.ColumnName;

            return key;
        }

        /// <summary>
        /// Returns schema information for a datasource given a collection name (e.g. 'Columns') and a string array of restrictions
        /// </summary>
        /// <param name="collectionName">collection to get information about</param>
        /// <param name="restrictions">restrictions for what collection is returned</param>
        /// <returns>schema information</returns>
        public DataTable getSchema(string collectionName, string[] restrictions)
        {
            OpenIfClosed();
            var dtable = this.conn.GetSchema("Columns", restrictions);
            return dtable;
        }

        /// <summary>
        /// Deprecated, used for compatibility with 'DataTools', returns datatable which provides the column schema for a database table
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <returns></returns>
        public DataTable Schema(string tableName)
        {
            var restrictions = new string[]{"",tableName,"",""};
            return getSchema("Columns",restrictions);
        }

        /// <summary>
        /// returns a single value from a SQL statement
        /// </summary>
        /// <param name="SQLStatement">SQL statement that provides a scalar value</param>
        /// <returns>value from statement</returns>
        public object getScalar(string SQLStatement)
        {

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLStatement;
                var obj = cmd.ExecuteScalar();
                return obj;
            }
        }

        /// <summary>
        /// Deprecated: used for backwards compatibility with 'DataTools'. see 'getScalar'
        /// </summary>
        /// <param name="SQLStatement"></param>
        /// <returns></returns>
        public object getValue(string SQLStatement)
        {
            return getScalar(SQLStatement);
        }

        /// <summary>
        /// Strongly typed version of getScalar, will attempt to either explicitly or implicitly convert a primitive type of a returned result if the conversion is possible.
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="SQLStatement">SQL statement that returns a scalar</param>
        /// <returns></returns>
        public T getScalar<T>(string SQLStatement)
        {
            var currType = typeof(T);
            if (currType.IsGenericType && currType.GetGenericTypeDefinition() == typeof(Nullable<>))
                currType = Nullable.GetUnderlyingType(currType);

            OpenIfClosed();

            using (var cmd = this.conn.CreateCommand())
            {
                cmd.CommandText = SQLStatement;
                var obj = cmd.ExecuteScalar();

                if (currType.IsAssignableFrom(obj.GetType().UnderlyingSystemType))
                    return (T)obj;
                else if (currType == typeof(string) || TypeConversion.isExplicitlyConvertable(currType, obj.GetType().UnderlyingSystemType))
                    return (T)Convert.ChangeType(obj, currType);
            }

            return default(T);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (conn != null)
                conn.Dispose();
            if (ResultSet != null)
                ResultSet.Dispose();

            this.disposed = true;
        }
    }
}
