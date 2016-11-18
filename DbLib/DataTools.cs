using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using ExtensionMethods;

namespace Scamps
{
    /// <summary>
    /// Add, edit, delete, and return data from Oracle
    /// </summary>
    public class DataTools : IDisposable
    {

        public string ConnectionString = "";
        public DataTable ResultSet = new DataTable();

        public int identity = 0;

        public string errors = "";
        public bool haserrors = false;

        public string messages = "";
        public bool hasmessages = false;

        public int rowcount = 0;
        public int colcount = 0;

        private OracleConnection connection = null;
        private SqlConnection sql_connection = null;
        private IDbDataAdapter iAdapter = null;
        private IDataReader iReader = null;

        private string _provider = "sql";
        private int _xrowpointer = 0;
        private bool _xEOF = true;
        private bool disposed = false;

        public string Provider
        {
            get { return _provider; }
            set
            {
                //TODO: make sure it is a supported provider, spelled correctly, etc.

                switch (value.ToLower())
                {
                    case "oracle.manageddataaccess.client":
                    case "oracle":
                    case "ora":
                    case "o":
                        _provider = "oracle";
                        break;

                    case "oledb":
                    case "ole":
                        _provider = "oledb";
                        break;
                    default:
                        _provider = "sql";
                        break;
                }
            }
        }

        public DataTools() { }
        public DataTools(System.Configuration.ConnectionStringSettings cstring)
        {
            if (cstring.ProviderName.IndexOf("oracle", StringComparison.OrdinalIgnoreCase) != -1)
            {
                Provider = "oracle";
            }
            else if (cstring.ProviderName.IndexOf("oledb", StringComparison.OrdinalIgnoreCase) != -1)
            {
                Provider = "oledb";
            }
            else
            {
                _provider = "sql";
            }
            this.OpenConnection(cstring.ConnectionString);
        }

        /// <summary>
        /// Resets the object
        /// </summary>
        public void Clear()
        {
            errors = "";
            haserrors = false;

            messages = "";
            hasmessages = false;
            ConnectionString = "";

            ResultSet.Clear();
            _xEOF = true;
            _xrowpointer = 0;

        }


        private void Error(string message)
        {
            haserrors = true;
            errors += message + System.Environment.NewLine;
        }

        private void Message(string message)
        {
            hasmessages = true;
            messages += message + System.Environment.NewLine;
        }
        /// <summary>
        /// Close the current connection to the database, if open.
        /// </summary>
        public void CloseConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Is the current connection open and active?
        /// </summary>
        public bool IsActive
        {
            get { return _connectionopen(); }
        }
        private bool _connectionopen()
        {
            bool ok = false;
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    ok = true;
                }

            }
            catch (OracleException ex)
            {
                Error("IsActive.Oracle:" + ex.Message);
            }
            catch (SqlException ex)
            {
                Error("IsActive.SQL:" + ex.Message);
            }
            catch (OleDbException ex)
            {
                Error("IsActive.OleDB:" + ex.Message);
            }

            return ok;
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

            if (ResultSet != null)
                ResultSet.Dispose();
            if (connection != null)
                connection.Dispose();
            if (sql_connection != null)
                sql_connection.Dispose();
            if (iReader != null)
                iReader.Dispose();

            this.disposed = true;
        }

        /// <summary>
        /// Opens a connection to specified provider database
        /// </summary>
        /// <param name="Connect_String">Optional connection string to pass in.  Alternately, set the property ConnectionString 
        /// before calling.  If you pass in a connection string, the ConnectionString property will be set.</param>
        /// <returns>True if the connection was successfully opened, False if not.</returns>
        public bool OpenConnection(string Connect_String = "")
        {
            try
            {
                if (Connect_String.Length > 0)
                {
                    ConnectionString = Connect_String;
                }
                if(_provider.IsNullOrEmpty())
                {
                    _provider = Tools.StringPart(ConnectionString.ToLower(), "provider=", ";");
                    if (_provider.IsNullOrEmpty()) { _provider = ConnectionString.Substring(ConnectionString.ToLower().IndexOf("provider=")+8); }
                }
                switch (_provider)
                {
                    case "oracle":
                        connection = new OracleConnection(ConnectionString);
                        connection.Open();
                        break;

                    case "sql":
                        sql_connection = new SqlConnection(ConnectionString);
                        sql_connection.Open();
                        break;

                    default:
                        sql_connection = new SqlConnection(ConnectionString);
                        sql_connection.Open();
                        break;
                }

            }
            catch (OracleException ex)
            {
                Error("OpenConneciton.Oracle:" + ex);
                return false;
            }
            catch (SqlException ex)
            {
                Error("OpenConneciton.SQL:" + ex);
                return false;
            }
            catch (Exception ex)
            {
                Error("OpenConneciton:" + ex);
                return false;
            }

            return true;
        }


        public void Log(string logname, string zone, string type, string message, int severity = 0, string data = "")
        {
            Dictionary<string, string> entry = new Dictionary<string, string>();

            if (logname.Length == 0) { logname = "general"; }
            if (zone.Length == 0) { zone = "global"; }
            if (type.Length == 0) { type = "system"; }

            entry.Add("logname", logname);
            entry.Add("zone", zone);
            entry.Add("type", type);
            entry.Add("message", message);
            entry.Add("severity", severity.ToString());
            entry.Add("data", data);
            entry.Add("created", DateTime.Now.ToString());

            int i = Insert(entry, "log");
        }
        public void Log(string message)
        {
            Dictionary<string, string> entry = new Dictionary<string, string>();

            entry.Add("logname", "general");
            entry.Add("zone", "global");
            entry.Add("type", "system");
            entry.Add("message", message);
            entry.Add("created", DateTime.Now.ToString());

            int i = Insert(entry, "log");
        }
        /// <summary>
        /// Fill the class DataTable object 'ResultSet' with the results of the passed SQL statement
        /// </summary>
        /// <param name="SQLStatement">Fully qualified SQL Statement</param>
        public void GetResultSet(string SQLStatement)
        {
            try
            {
                ResultSet.Clear();
                switch (_provider)
                {
                    case "oracle":
                        OracleDataAdapter da = new OracleDataAdapter(SQLStatement, connection);
                        da.Fill(ResultSet);
                        break;

                    default:
                        SqlDataAdapter sda = new SqlDataAdapter(SQLStatement, sql_connection);
                        sda.Fill(ResultSet);
                        break;
                }

                rowcount = ResultSet.Rows.Count;
                colcount = ResultSet.Columns.Count;
                _xrowpointer = 0;
                if (rowcount > 0)
                {
                    _xEOF = false;
                }
                else
                {
                    _xEOF = true;
                }

            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }

        }

        /// <summary>
        /// Fill the class DataTable object 'ResultSet' with the results of the passed SQL statement
        /// </summary>
        /// <param name="SQLStatement">Fully qualified SQL Statement</param>
        public void GetResultSet(string SQLStatement, Dictionary<string,object> parameters)
        {
            try
            {
                ResultSet.Clear();
                DbDataAdapter db = null;


                if (_provider == "oracle")
                {
                    // dirty little swapout
                    // TODO: move this to a managed function
                    SQLStatement = SQLStatement.Replace(" @", " :");
                    OracleCommand cmd = new OracleCommand(SQLStatement,connection);
                    SetParameters(ref cmd, parameters);
                    db = new OracleDataAdapter((OracleCommand)cmd);
                }
                else
                {
                    SqlCommand cmd = new SqlCommand(SQLStatement,sql_connection);
                    SetParameters(ref cmd, parameters);
                    db = new SqlDataAdapter((SqlCommand)cmd);
                }
 
                db.Fill(ResultSet);

                rowcount = ResultSet.Rows.Count;
                colcount = ResultSet.Columns.Count;
                _xrowpointer = 0;
                if (rowcount > 0)
                {
                    _xEOF = false;
                }
                else
                {
                    _xEOF = true;
                }

            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }

        }

        /// <summary>
        /// Fill the class DataTable object 'ResultSet' with the results of the passed SQL statement
        /// </summary>
        /// <param name="SQLStatement">Fully qualified SQL Statement</param>
        public void GetResultSet(string SQLStatement, params object[] parameters)
        {
            try
            {
                ResultSet.Clear();
                DbDataAdapter db = null;
                DbCommand cmd = null;
                switch (_provider)
                {
                    case "oracle":
                        cmd = connection.CreateCommand();
                        db = new OracleDataAdapter((OracleCommand)cmd);
                        break;

                    default:
                        cmd = sql_connection.CreateCommand();
                        db = new SqlDataAdapter((SqlCommand)cmd);
                        break;
                }

                cmd.CommandText = SQLStatement;

                if (parameters != null)
                {
                    foreach (var para in parameters)
                    {
                        var pm = cmd.CreateParameter();
                        pm.Value = para;
                        cmd.Parameters.Add(pm);
                    }
                }

                db.Fill(ResultSet);

                rowcount = ResultSet.Rows.Count;
                colcount = ResultSet.Columns.Count;
                _xrowpointer = 0;
                if (rowcount > 0)
                {
                    _xEOF = false;
                }
                else
                {
                    _xEOF = true;
                }

            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }

        }

        public int ExecuteNonQuery(string SQLstatement, params object[] parameters)
        {
            int affected = 0;

            try
            {
                DbDataAdapter db = null;
                DbCommand cmd = null;
                DbConnection conn = null;

                switch (_provider)
                {
                    case "oracle":
                        cmd = connection.CreateCommand();
                        conn = connection;
                        db = new OracleDataAdapter((OracleCommand)cmd);
                        break;

                    default:
                        cmd = sql_connection.CreateCommand();
                        conn = sql_connection;
                        db = new SqlDataAdapter((SqlCommand)cmd);
                        break;
                }

                cmd.CommandText = SQLstatement;

                if (parameters != null && parameters.Length > 0)
                {
                    foreach (var param in parameters)
                    {
                        var dbp = cmd.CreateParameter();
                        dbp.Value = param;
                        cmd.Parameters.Add(dbp);
                    }
                }

                affected = cmd.ExecuteNonQuery();

            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }
            return affected;

        }

        public int BulkQuery(string SQLstatement, params object[][] parameters)
        {
            int affected = 0;

            if (parameters == null || parameters.Length <= 0)
                return affected;
            try
            {
                DbDataAdapter db = null;
                DbCommand cmd = null;
                DbConnection conn = null;
                switch (_provider)
                {
                    case "oracle":
                        cmd = connection.CreateCommand();
                        conn = connection;
                        db = new OracleDataAdapter((OracleCommand)cmd);
                        break;

                    default:
                        cmd = sql_connection.CreateCommand();
                        conn = sql_connection;
                        db = new SqlDataAdapter((SqlCommand)cmd);
                        break;
                }

                using (var tr = conn.BeginTransaction())
                {
                    cmd.CommandText = SQLstatement;
                    cmd.Prepare();

                    //define size of parameter array
                    var dbp = new DbParameter[parameters[0].Length];
                    //initialize parameter array with parameter object
                    for (var i = 0; i < parameters[0].Length; i++)
                        cmd.Parameters.Add(cmd.CreateParameter());

                    //for each set of parameters, bind them to the parameter object and then execute the query
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        for (var j = 0; j < parameters[i].Length; j++)
                        {
                            cmd.Parameters[j].Value = parameters[i][j];
                        }
                        affected += cmd.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }

            return affected;
        }

        /// <summary>
        /// Return the current ResultSet row as a dictionary of string>object values.
        /// </summary>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetRow()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (ResultSet.Rows.Count == 0)
            {
                //no data to row out.  Is this an error or a message?
                Message("GetRow:Result Set returned zero rows.  No data to output.");
            }
            else
            {
                for (int i = 0; i < ResultSet.Columns.Count; i++)
                {
                    data.Add(ResultSet.Columns[i].ColumnName.ToLower(), ResultSet.Rows[_xrowpointer].ItemArray[i]);
                }
            }
            return data;
        }

        /// <summary>
        /// Return the current ResultSet row as a dictionary of string values.
        /// </summary>
        /// <returns>Dictionary string,string</returns>
        public Dictionary<string, string> GetRowStrings()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (ResultSet.Rows.Count == 0)
            {
                //no data to row out.  Is this an error or a message?
                Message("GetRowStrings:Result Set returned zero rows.  No data to output.");
            }
            else
            {
                for (int i = 0; i < ResultSet.Columns.Count; i++)
                {
                    data.Add(ResultSet.Columns[i].ColumnName.ToLower(), ResultSet.Rows[_xrowpointer].ItemArray[i].ToString());
                }
            }
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

        public string Get(string ColumnName)
        {
            try
            {
                int c = ResultSet.Rows[_xrowpointer].Table.Columns.IndexOf(ColumnName);
                return ResultSet.Rows[_xrowpointer].ItemArray[c].ToString();
            }
            catch
            {
                Error("Get.Column." + ColumnName + ":Not Found");
                return "";
            }
        }

        public string Get(int ColumnIndex)
        {
            try
            {
                return ResultSet.Rows[_xrowpointer].ItemArray[ColumnIndex].ToString();
            }
            catch
            {
                Error("Get.Column.ByIndex[" + ColumnIndex + "]:Not Found");
                return "";
            }
        }

        public bool Exists(string SQLStatement)
        {
            bool OK = false;
            DataTable results = new DataTable();

            try
            {
                if (_provider == "oracle")
                {
                    OracleDataAdapter da = new OracleDataAdapter(SQLStatement, connection);
                    da.Fill(results);
                    if(results.Rows.Count>0)
                    {
                        OK = true;
                    }
                    results.Clear();
                    da.Dispose();

                }
                else
                {
                    SqlDataAdapter sda = new SqlDataAdapter(SQLStatement, sql_connection);
                    sda.Fill(results);
                    if (results.Rows.Count > 0)
                    {
                        OK = true;
                    }
                    results.Clear();
                    sda.Dispose();

                }
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (OleDbException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }

            return OK;
        }

        /// <summary>
        /// Fill the class DataTable object 'ResultSet' with the results of the passed SQL statement
        /// </summary>
        /// <param name="SQLStatement">Fully qualified SQL Statement</param>
        public object GetValue(string SQLStatement)
        {
            object val = null;
            try
            {
                if (_provider == "oracle")
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = SQLStatement;
                    val = command.ExecuteScalar();
                    command.Dispose();
                }
                else
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = sql_connection;
                    command.CommandText = SQLStatement;
                    val = command.ExecuteScalar();
                    command.Dispose();
                }
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (OleDbException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }
            return val;
        }


        /// <summary>
        /// Fill the class DataTable object 'ResultSet' with the results of the passed SQL statement
        /// </summary>
        /// <param name="SQLStatement">Fully qualified SQL Statement</param>
        /// <param name="parameters">Parameter dictionary containing statement parameters</param>
        public object GetValue(string SQLStatement,Dictionary<string,object> parameters)
        {
            object val = null;
            try
            {
                if (_provider == "oracle")
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = SQLStatement;
                    SetParameters(ref command, parameters);
                    val = command.ExecuteScalar();
                    command.Dispose();
                }
                else
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = sql_connection;
                    command.CommandText = SQLStatement;
                    SetParameters(ref command, parameters);
                    val = command.ExecuteScalar();
                    command.Dispose();
                }
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (OleDbException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }
            return val;
        }
        private void SetParameters(ref OracleCommand cmd, Dictionary<string, object> pValues)
        {
            foreach (KeyValuePair<string, object> item in pValues)
            {
                cmd.Parameters.Add(item.Key, item.Value);
            }
        }
        private void SetParameters(ref OleDbCommand cmd, Dictionary<string, object> pValues)
        {
            foreach (KeyValuePair<string, object> item in pValues)
            {
                cmd.Parameters.AddWithValue(item.Key, item.Value);
            }
        }
        private void SetParameters(ref SqlCommand cmd, Dictionary<string, object> pValues)
        {
            foreach (KeyValuePair<string, object> item in pValues)
            {
                if (item.Value != null)
                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                else
                    cmd.Parameters.AddWithValue(item.Key, DBNull.Value);
            }
        }

        //=========================== SQL Server Schema Utilities ===========================

        public string IdentityColumn(string TableName)
        {
            string results = "";
            if (_provider != "sql")
            {
                results = "Not Supported";
                Message("IdentityColumn is not supported by this provider");
            }
            else
            {
                try
                {
                    string sql = "select column_name from information_schema.columns where table_schema = 'dbo' ";
                    sql += "and columnproperty(object_id(table_name), column_name,'IsIdentity') = 1 and table_name = '" + TableName + "'";
                    results = Tools.AsText(GetValue(sql));
                }
                catch (Exception ex)
                {
                    Error("IdentityColumn:" + ex.Message);
                }
            }
            return results;
        }

        public DataTable Schema(string TableName)
        {
            string sql = "";
            DataTable dt = new DataTable();
            if (_provider != "sql")
            {
                Message("Schema: Not supported by this data provider");
            }
            else
            {
                sql += "select TABLE_NAME as tablename,COLUMN_NAME as columnnname,COLUMN_DEFAULT as defaultvalue,";
                sql += "DATA_TYPE as datatype,CHARACTER_MAXIMUM_LENGTH as maxlength,ORDINAL_POSITION as position,IS_NULLABLE as nullable ";
                sql += "from INFORMATION_SCHEMA.COLUMNS order by TABLE_NAME,ORDINAL_POSITION";
                try
                {
                    ResultSet.Clear();
                    switch (_provider)
                    {
                        case "oracle":
                            OracleDataAdapter da = new OracleDataAdapter(sql, connection);
                            da.Fill(dt);
                            break;

                        default:
                            SqlDataAdapter sda = new SqlDataAdapter(sql, sql_connection);
                            sda.Fill(dt);
                            break;
                    }

                }

                catch (OracleException ex)
                {
                    Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
                }
                catch (SqlException ex)
                {
                    Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
                }
                catch (Exception ex)
                {
                    Error(MethodBase.GetCurrentMethod().Name + ex);
                }
            }
            return dt;
        }


        // ============ SCAMP Utilities ===============



        /// <summary>
        /// Return a string containing a list file XML contents.  A list file is the format lists are distributed in.
        /// </summary>
        /// <param name="listname">Name of the list; the list_cd</param>
        /// <returns>STRING</returns>
        public string ListFile(string listname)
        {
            StringBuilder sb = new StringBuilder();
            string results = "";
            string list = "";
            Dictionary<string, string> item = new Dictionary<string, string>();
            listname = Tools.SQLSafe(listname.Clean()).ToUpper();
            string sql = "select * from admin_element_list where UPPER(list_cd) = '" + listname + "' order by seq_num";
            GetResultSet(sql);
            while (!EOF)
            {
                item.Clear();
                item = GetRowStrings();

                if(item.getValueOrDefault("visible_abbr") == "N")
                {
                    sb.AppendLine("\t\t" + item.getValueOrDefault("value_txt") + "|N" + ":" + item.getValueOrDefault("description_txt").AsList() + ",");
                }
                else
                {
                    sb.AppendLine("\t\t" + item.getValueOrDefault("value_txt") + ":" + item.getValueOrDefault("description_txt").AsList() + ",");
                }
                NextRow();
            }

            list = sb.ToString();
            if (list.EndsWith("," + System.Environment.NewLine)) { list = list.Substring(1, list.Length - (("," + System.Environment.NewLine).Length +1)); }

            results = "\t" + Tools.XWrap(listname, "name",true) + System.Environment.NewLine;

            results += "\t" + "<elements>" + System.Environment.NewLine + "\t" + list + System.Environment.NewLine + "\t</elements>";
            results = "<list>" + System.Environment.NewLine + results + System.Environment.NewLine + "</list>";
            return results;
        }


        /// <summary>
        /// Returns a SQL insert statement for a row in a table.
        /// </summary>
        /// <param name="hd">Dictionary object containing keys and values representing column names and data</param>
        /// <param name="TableName">Name of the target table to insert the new row into.</param>
        /// <returns>Number of rows affected by insert, which should always be 1 if no error or -1 if an error occurs.  Examine the property Errors to view any generated errors from the operation.</returns>
        public string InsertStatement(Dictionary<string, string> hd, string TableName)
        {
            string sql = _assembleInsert(hd, TableName, "insert");
            return sql;
        }
        /// <summary>
        /// Insert a row in a table.
        /// </summary>
        /// <param name="hd">Dictionary object containing keys and values representing column names and data</param>
        /// <param name="TableName">Name of the target table to insert the new row into.</param>
        /// <returns>Integer. The number of rows affected by insert, which should always be 1 if no error or -1 if an error occurs. 
        /// In SQL Server, the row ID just added can be found in the IDENTITY property of the object. Examine the property Errors to view any generated errors from the operation.</returns>
        public int Insert(Dictionary<string, string> hd, string TableName)
        {
            Dictionary<string, object> pValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            int i = 0;
            string sql = _assembleParams(hd, ref pValues, TableName, "insert");
            if (sql.Length > 0)
            {
                try
                {
                    if (_provider == "oracle")
                    {
                        OracleCommand command = new OracleCommand();
                        command.Connection = connection;
                        command.CommandText = sql;
                        SetParameters(ref command, pValues);
                        i = command.ExecuteNonQuery();
                        command.Dispose();
                        return i;
                    }
                    else
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = sql_connection;
                        command.CommandText = sql;
                        SetParameters(ref command, pValues);
                        i = command.ExecuteNonQuery();

                        //sql server identity
                        command.CommandText = "select @@identity as ID from " + TableName;
                        identity = Tools.AsInteger(command.ExecuteScalar());
                        command.Dispose();
                        return i;
                    }

                }
                catch (OracleException ex)
                {
                    Error("Oracle.Insert:" + ex.Message);
                }
                catch (OleDbException ex)
                {
                    Error("SQL.Insert:" + ex.Message);
                }
                catch (Exception ex)
                {
                    Error("Insert:" + ex.Message);
                }
            }
            else
            {
                return -1;
            }
            return i;
        }

        /// <summary>
        /// Returns SQL update statement for an existing row or rows in a table to be updated.
        /// </summary>
        /// <param name="hd">Dictionary of keys and values representing table column names and data</param>
        /// <param name="TableName">Target table where the row will be updated</param>
        /// <param name="IdentityCriteria">Criteria that uniquely identifies the row or rows.  Example 'id=3429'</param>
        /// <returns>Number of rows affected by update or -1 if an error occurs. </returns>
        public string UpdateStatement(Dictionary<string, string> hd, string TableName, string IdentityCriteria)
        {
            string sql = _assembleInsert(hd, TableName, "update", IdentityCriteria);
            return sql;
        }


        /// <summary>
        /// Update an existing row or rows in a table
        /// </summary>
        /// <param name="hd">Dictionary of keys and values representing table column names and data</param>
        /// <param name="TableName">Target table where the row will be updated</param>
        /// <param name="IdentityCriteria">Criteria that uniquely identifies the row or rows.  Example 'id=3429'</param>
        /// <returns>Number of rows affected by update or -1 if an error occurs. </returns>
        public int Update(Dictionary<string, string> hd, string TableName, string IdentityCriteria)
        {
            Dictionary<string, object> pValues = new Dictionary<string, object>();
            int i = 0;
            string sql = _assembleParams(hd, ref pValues, TableName, "update", IdentityCriteria);

            if (sql.Length > 0)
            {
                if (_provider == "oracle")
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = sql;
                    SetParameters(ref command, pValues);
                    i = command.ExecuteNonQuery();
                    command.Dispose();
                    return i;
                }
                else
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = sql_connection;
                    command.CommandText = sql;
                    SetParameters(ref command, pValues);
                    i = command.ExecuteNonQuery();
                    command.Dispose();
                    return i;
                }

            }
            else
            {
                Error("Update resulted in 0 length update statement.");
                return -1;
            }
        }
        /// <summary>
        /// Delete an existing row or rows in a table
        /// </summary>
        /// <param name="TableName">Target table where the row will be deleted</param>
        /// <param name="IdentityCriteria">Criteria that uniquely identifies the row or rows.  Example 'id=3429'</param>
        /// <returns>Number of rows affected by the delete or -1 if there is an error.  A delete statement can execute successfully 
        /// even if no rows were actually deleted, as in the case of identityCriteria that does not resolve to an actual row. 
        /// In this case the returned value will be 0</returns>
        public int Delete(string TableName, string identityCriteria)
        {

            identityCriteria = identityCriteria.Trim();
            if (identityCriteria.ToLower().StartsWith("where ")) { identityCriteria = identityCriteria.Substring(6).Trim(); }

            string sql = "delete from " + TableName + " where " + identityCriteria;

            return _executeSQL(sql);

        }

        /// <summary>
        /// Execute a non-query SQL statement
        /// </summary>
        /// <param name="SQLStatement">SQL Statement to execute</param>
        /// <returns>Number of rows affected by the query</returns>
        public int Execute(string SQLStatement)
        {
            return _executeSQL(SQLStatement);
        }


        public DbDataReader getDataReader(string SQLStatement)
        {
            try
            {
                DbDataAdapter db = null;
                DbCommand cmd = null;
                switch (_provider)
                {
                    case "oracle":
                        cmd = connection.CreateCommand();
                        db = new OracleDataAdapter((OracleCommand)cmd);
                        break;

                    default:
                        cmd = sql_connection.CreateCommand();
                        db = new SqlDataAdapter((SqlCommand)cmd);
                        break;
                }

                cmd.CommandText = SQLStatement;

                var reader = cmd.ExecuteReader();
                return reader;
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
            }
            return null;
        }


        private int _executeSQL(string SQLStatement)
        {

            int i = 0;
            try
            {
                if (_provider == "oracle")
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = SQLStatement;
                    i = command.ExecuteNonQuery();
                    command.Dispose();
                }
                else
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = sql_connection;
                    command.CommandText = SQLStatement;
                    i = command.ExecuteNonQuery();
                    command.Dispose();
                }
            }
            catch (OracleException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".Oracle:" + ex);
                return -1;
            }
            catch (SqlException ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ".SQL:" + ex);
                return -1;
            }
            catch (Exception ex)
            {
                Error(MethodBase.GetCurrentMethod().Name + ex);
                return -1;
            }


            return i;
        }


        private string _assembleParams(Dictionary<string, string> hd, ref Dictionary<string, object> ParameterValues, string tablename, string iMOD, string identityCriteria = "")
        {
            //create insert statement for the tablename specified from the passed dictionary
            string results = "";

            identityCriteria = identityCriteria.Trim();
            if (identityCriteria.ToLower().StartsWith("where ")) { identityCriteria = identityCriteria.Substring(6).Trim(); }

            if ((iMOD != "insert") && (identityCriteria.Length == 0))
            {
                Error("Identity Criteria is required for an update statement");
                return "";
            }

            // support for different parameter markers
            string pmark = "@";
            if (_provider == "oracle")
            {
                pmark = ":";
            }

            // row flags we need to set...
            string fieldName = "";
            int fieldSize = 0;
            bool readOnly = false;
            string dataType = "";

            //dictionary value, which will be a string...
            string thisValue = "";
            string valuesText = "";
            string updateText = "";
            string fieldsText = "";

            //For this method, which does insert or update based on parameters, we need an object dictionary
            Dictionary<string, object> pValues = new Dictionary<string, object>();

            IDataReader r = null;
            DataTable dt;
            if (_provider == "oracle")
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandText = "select * from " + tablename + " where rownum<=1";
                r = cmd.ExecuteReader();
                dt = r.GetSchemaTable();
            }
            else
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sql_connection;
                cmd.CommandText = "select top 1 * from " + tablename;
                r = cmd.ExecuteReader();
                dt = r.GetSchemaTable();
            }

            foreach (DataRow oROW in dt.Rows)
            {
                fieldName = "";
                dataType = "";
                fieldSize = 1;
                readOnly = false;
                foreach (DataColumn oProp in dt.Columns)
                {
                    switch (oProp.ColumnName.ToString().ToLower())
                    {
                        case "columnname":
                            fieldName = oROW[oProp].ToString().ToLower();
                            break;

                        case "datatype":
                            dataType = oROW[oProp].ToString().ToLower();
                            break;

                        case "columnsize":
                            try
                            {
                                fieldSize = Convert.ToInt32(oROW[oProp]);
                            }
                            catch
                            {
                                fieldSize = 1;
                            }
                            break;

                        case "isreadonly":
                            readOnly = Convert.ToBoolean(oROW[oProp]);
                            break;

                        default:

                            break;
                    }
                }

                string _thisvalue = "";
                string _thisupdate = "";
                if (!readOnly)
                {
                    try
                    {
                        thisValue = hd[fieldName];

                        //handle sql inject tick trick, but it could have already been prepped.
                        //so undo and redo so as not to make a mess of it...
                        //thisValue = thisValue.Replace("''", "'");
                        //thisValue = thisValue.Replace("'", "''");

                        try
                        {

                            //set expected defaults...
                            _thisvalue = pmark + fieldName;
                            _thisupdate = fieldName + " = " + pmark + fieldName;

                            switch (dataType)
                            {
                                case "system.string":
                                    //modify for size
                                    if (thisValue.Length > fieldSize)
                                    {
                                        thisValue = thisValue.Substring(0, fieldSize - 1);
                                        Message(fieldName + ".DataTrunc: The data length exceeded the defined field size of " + fieldSize + ". The data was truncated.");
                                    }

                                    //thisValue = Tools.SQLSafe(thisValue);
                                    pValues.Add(fieldName, Tools.AsText(thisValue));

                                    break;

                                case "system.double":
                                    if (_isNumeric(thisValue))
                                        pValues.Add(fieldName, Tools.AsDouble(thisValue));
                                    else
                                        pValues.Add(fieldName, null);
                                    Message(fieldName + ".Null: The data could not be converted to a number.");
                                    break;

                                case "system.decimal":

                                    if (_isNumeric(thisValue))
                                        pValues.Add(fieldName, Tools.AsDecimal(thisValue));
                                    else
                                        pValues.Add(fieldName, null);
                                    Message(fieldName + ".Null: The data could not be converted to a number.");
                                    break;

                                case "system.float":

                                    if (_isNumeric(thisValue))
                                        pValues.Add(fieldName, Tools.AsFloat(thisValue));
                                    else
                                        pValues.Add(fieldName, null);
                                    Message(fieldName + ".Null: The data could not be converted to a number.");
                                    break;

                                case "system.int64":
                                case "system.uint64":

                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            pValues.Add(fieldName, Tools.AsLong(thisValue));
                                        }
                                        catch
                                        {
                                            pValues.Add(fieldName, null);
                                            Message(fieldName + ".Null: The data could not be converted to an long integer/INT64.");
                                        }
                                    }
                                    else
                                    {
                                        pValues.Add(fieldName, null);
                                        Message(fieldName + ".Null: The data could not be converted to an long integer/INT64.");
                                    }
                                    break;

                                case "system.int32":
                                case "system.uint32":
                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            pValues.Add(fieldName, Tools.AsInteger(thisValue));
                                        }
                                        catch
                                        {
                                            pValues.Add(fieldName, null);
                                            Message(fieldName + ".Null: The data could not be converted to an integer/INT32.");
                                        }
                                    }
                                    else
                                    {
                                        pValues.Add(fieldName, null);
                                        Message(fieldName + ".Null: The data could not be converted to an integer/INT32.");
                                    }

                                    break;

                                case "system.int16":
                                case "system.uint16":

                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            pValues.Add(fieldName, Tools.AsInteger(thisValue));
                                        }
                                        catch
                                        {
                                            pValues.Add(fieldName, null);
                                            Message(fieldName + ".Null: The data could not be converted to an integer/INT16.");
                                        }
                                    }
                                    else
                                    {
                                        pValues.Add(fieldName, null);
                                        Message(fieldName + ".Null: The data could not be converted to an integer.");
                                    }
                                    break;

                                case "system.boolean":

                                    //not actually supported by Oracle. Meh.
                                    switch (thisValue.ToLower())
                                    {
                                        case "yes":
                                        case "true":
                                        case "1":
                                        case "-1":
                                        case "on":
                                        case "checked":
                                        case "selected":
                                        case "x":
                                            pValues.Add(fieldName, Tools.AsBoolean(true));
                                            break;
                                        default:
                                            pValues.Add(fieldName, Tools.AsBoolean(false));
                                            break;
                                    }
                                    break;

                                case "system.datetime":

                                    thisValue = Tools.AsDate(thisValue);
                                    if (thisValue.Length == 0)
                                    {
                                        pValues.Add(fieldName, null);
                                        Message(fieldName + ".Null: The data could not be cast to a valid date.");
                                    }
                                    else
                                    {
                                        pValues.Add(fieldName, DateTime.Parse(thisValue));
                                        //_thisvalue = "to_date(" + pmark + fieldName + ")";
                                        //_thisupdate = fieldName + " = " + "to_date(" + pmark + fieldName +")";

                                    }


                                    break;
                                default:
                                    if (thisValue.Length > fieldSize)
                                    {
                                        thisValue = thisValue.Substring(0, fieldSize - 1);
                                        Message(fieldName + ".DataTrunc: The data length exceeded the defined field size of " + fieldSize + ". The data was truncated.");
                                    }

                                    //thisValue = Tools.SQLSafe(thisValue);
                                    pValues.Add(fieldName, Tools.AsText(thisValue));

                                    break;
                            }

                            valuesText += _thisvalue + ",";
                            fieldsText += fieldName + ",";
                            updateText += _thisupdate + ",";
                        }
                        catch (Exception ex)
                        {
                            //process error
                            Error("Assembly Operation Exception:" + ex.Message);
                        }
                    }
                    catch
                    {
                        //key no present in the hash.  'tis OK, we can just skip it.

                    }
                }


            }

            r.Close();


            if (valuesText.Length > 0)
            {
                valuesText = junkTrim(valuesText);
                fieldsText = junkTrim(fieldsText);
                updateText = junkTrim(updateText);
            }

            if (iMOD.ToLower() == "insert")
            {
                results = "Insert into " + tablename + "(" + fieldsText + ") Values(" + valuesText + ")";
            }
            else
            {
                results = "Update " + tablename + " SET " + updateText + " WHERE " + identityCriteria;
            }

            ParameterValues = pValues;

            return results;
        }

        /// <summary>
        /// Creates an insert of update statement based on the passed dictionary object
        /// </summary>
        /// <param name="hd">The dictionary object model</param>
        /// <param name="tablename">The target table</param>
        /// <param name="iMOD">'insert' or 'update'</param>
        /// <param name="identityCriteria">If 'update', the unique criteria identifing the row or rows</param>
        /// <returns>String containing the insert statement</returns>
        /// <remarks>This method matches the database definition of the target table with the keys and values in the dictionary to build an 
        /// appropriate insert or update statement.  String data that is too large for the specified column is truncated to fit, date columns that cannot 
        /// cast to a date are set to null, numeric data is validated, etc. so the insert or update has the best chance of success.  Examine messages to 
        /// determine if any data was modified.
        /// </remarks>
        private string _assembleInsert(Dictionary<string, string> hd, string tablename, string iMOD, string identityCriteria = "")
        {
            //create insert statement for the tablename specified from the passed dictionary
            string results = "";

            identityCriteria = identityCriteria.Trim();
            if (identityCriteria.ToLower().StartsWith("where ")) { identityCriteria = identityCriteria.Substring(6).Trim(); }

            if ((iMOD != "insert") && (identityCriteria.Length == 0))
            {
                Error("Identity Criteria is required for an update statement");
                return "";
            }

            // row flags we need to set...
            string fieldName = "";
            int fieldSize = 0;
            bool readOnly = false;
            string dataType = "";

            //dictionary value, which will be a string...
            string thisValue = "";
            string valuesText = "";
            string updateText = "";
            string fieldsText = "";

            IDataReader r = null;
            DataTable dt;
            if (_provider == "oracle")
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandText = "select * from " + tablename + " where rownum<=1";
                r = cmd.ExecuteReader();
                dt = r.GetSchemaTable();
            }
            else
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sql_connection;
                cmd.CommandText = "select top 1 * from " + tablename;
                r = cmd.ExecuteReader();
                dt = r.GetSchemaTable();
            }

            foreach (DataRow oROW in dt.Rows)
            {
                fieldName = "";
                dataType = "";
                fieldSize = 1;
                readOnly = false;

                foreach (DataColumn oProp in dt.Columns)
                {
                    switch (oProp.ColumnName.ToString().ToLower())
                    {
                        case "columnname":
                            fieldName = oROW[oProp].ToString().ToLower();
                            break;

                        case "datatype":
                            dataType = oROW[oProp].ToString().ToLower();
                            break;

                        case "columnsize":
                            try
                            {
                                fieldSize = Convert.ToInt32(oROW[oProp]);
                            }
                            catch
                            {
                                fieldSize = 1;
                            }
                            break;

                        case "isreadonly":
                            readOnly = Convert.ToBoolean(oROW[oProp]);
                            break;

                        default:

                            break;
                    }
                }

                //we have our column properties set.

                if (!readOnly)
                {
                    try
                    {
                        thisValue = hd[fieldName];

                        //handle sql inject tick trick
                        thisValue = thisValue.Replace("''", "'");
                        thisValue = thisValue.Replace("'", "''");

                        try
                        {
                            switch (dataType)
                            {
                                case "system.string":
                                    //modify for size
                                    if (thisValue.Length > fieldSize)
                                    {
                                        thisValue = thisValue.Substring(0, fieldSize - 1);
                                        Message(fieldName + ".DataTrunc: The data length exceeded the defined field size of " + fieldSize + ". The data was truncated.");
                                    }

                                    thisValue = Tools.SQLSafe(thisValue);
                                    valuesText = valuesText + "'" + thisValue + "'";
                                    updateText = updateText + fieldName + " = " + "'" + thisValue + "'";


                                    break;


                                case "system.double":
                                case "system.decimal":
                                case "system.float":
                                    if (_isNumeric(thisValue))
                                    {

                                        valuesText = valuesText + thisValue;
                                        updateText = updateText + fieldName + " = " + thisValue;

                                    }
                                    else
                                    {
                                        valuesText = valuesText + "null";
                                        updateText = updateText + fieldName + " = " + "null";
                                        Message(fieldName + ".Null: The data could not be converted to a number.");
                                    }

                                    break;

                                case "system.int64":
                                case "system.uint64":

                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            thisValue = Convert.ToInt64(thisValue).ToString();
                                            valuesText = valuesText + thisValue;
                                            updateText = updateText + fieldName + " = " + thisValue;
                                        }
                                        catch
                                        {
                                            valuesText = valuesText + "null";
                                            updateText = updateText + fieldName + " = " + "null";
                                            Message(fieldName + ".Null: The data could not be converted to an long integer/INT64.");
                                        }
                                    }
                                    else
                                    {
                                        valuesText = valuesText + "null";
                                        updateText = updateText + fieldName + " = " + "null";
                                        Message(fieldName + ".Null: The data could not be converted to an long integer/INT64.");
                                    }
                                    break;

                                case "system.int32":
                                case "system.uint32":
                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            thisValue = Convert.ToInt32(thisValue).ToString();
                                            valuesText = valuesText + thisValue;
                                            updateText = updateText + fieldName + " = " + thisValue;
                                        }
                                        catch
                                        {
                                            valuesText = valuesText + "null";
                                            updateText = updateText + fieldName + " = " + "null";
                                            Message(fieldName + ".Null: The data could not be converted to an integer/INT32.");
                                        }
                                    }
                                    else
                                    {
                                        valuesText = valuesText + "null";
                                        updateText = updateText + fieldName + " = " + "null";
                                        Message(fieldName + ".Null: The data could not be converted to an integer/INT32.");
                                    }

                                    break;

                                case "system.int16":
                                case "system.uint16":

                                    if (_isNumeric(thisValue))
                                    {
                                        try
                                        {
                                            thisValue = Convert.ToInt16(thisValue).ToString();
                                            valuesText = valuesText + thisValue;
                                            updateText = updateText + fieldName + " = " + thisValue;
                                        }
                                        catch
                                        {
                                            valuesText = valuesText + "null";
                                            updateText = updateText + fieldName + " = " + "null";
                                            Message(fieldName + ".Null: The data could not be converted to an integer/INT16.");
                                        }
                                    }
                                    else
                                    {
                                        valuesText = valuesText + "null";
                                        updateText = updateText + fieldName + " = " + "null";
                                        Message(fieldName + ".Null: The data could not be converted to an integer.");
                                    }
                                    break;
                                case "system.boolean":
                                    switch (thisValue.ToLower())
                                    {
                                        case "yes":
                                        case "true":
                                        case "1":
                                        case "-1":
                                        case "on":
                                        case "checked":
                                        case "selected":
                                        case "x":
                                            valuesText = valuesText + "1";
                                            updateText = updateText + fieldName + " = " + "1";
                                            break;
                                        default:
                                            valuesText = valuesText + "0";
                                            updateText = updateText + fieldName + " = " + "0";
                                            break;
                                    }
                                    break;

                                case "system.datetime":
                                    //TODO: preparse the date and validate
                                    thisValue = Tools.AsDate(thisValue);
                                    if (thisValue.Length == 0)
                                    {
                                        valuesText = valuesText + "null";
                                        updateText = updateText + fieldName + " = " + "null";
                                        Message(fieldName + ".Null: The data could not be cast to a valid date.");
                                    }
                                    else
                                    {
                                        valuesText = valuesText + "TO_DATE('" + thisValue + "', 'DD/MM/YYYY HH24:MI:SS')";
                                        updateText = updateText + fieldName + " = " + "'" + "TO_DATE('" + thisValue + "', 'DD/MM/YYYY HH24:MI:SS')" + "'";
                                    }


                                    break;
                                default:
                                    if (thisValue.Length > fieldSize)
                                    {
                                        thisValue = thisValue.Substring(0, fieldSize - 1);
                                        Message(fieldName + ".DataTrunc: The data length exceeded the defined field size of " + fieldSize + ". The data was truncated.");
                                    }
                                    valuesText = valuesText + "'" + Tools.SQLSafe(thisValue) + "'";
                                    updateText = updateText + fieldName + " = " + "'" + thisValue + "'";

                                    break;

                            }

                            valuesText = valuesText + ",";
                            fieldsText = fieldsText + fieldName + ",";
                            updateText = updateText + ",";
                        }
                        catch (Exception ex)
                        {
                            //process error
                            Error("Assembly Operation Exception:" + ex.Message);
                        }
                    }
                    catch
                    {
                        //key no present in the hash.  'tis OK, we can just skip it.

                    }
                }


            }

            r.Close();

            if (valuesText.Length > 0)
            {
                valuesText = junkTrim(valuesText);
                fieldsText = junkTrim(fieldsText);
                updateText = junkTrim(updateText);
            }

            if (iMOD.ToLower() == "insert")
            {
                results = "Insert into " + tablename + "(" + fieldsText + ") Values(" + valuesText + ")";
            }
            else
            {
                results = "Update " + tablename + " SET " + updateText + " WHERE " + identityCriteria;
            }
            return results;
        }

        /// <summary>
        /// Imports an XML document into a database table
        /// </summary>
        /// <param name="source">XML or fully qualified path to an XML file</param>
        /// <param name="TableName">Target table name - i.e. where the processed data goes</param>
        /// <param name="NodeName">The name of the XML node that represents an element of data to import.</param>
        /// <param name="NodeParent">Optional string parent node name in cases where node data is contained in a parent node.</param>
        /// <returns>Boolean, True if the import was successful, False if the import failed.  In cases where the entire import fails, some rows may 
        /// have already been inserted before the failure point.  Examine messages for additional details.</returns>
        public bool Import(string source, string TableName, string NodeName, string NodeParent = "")
        {
            DateTime starttime = DateTime.Now;

            bool ok = true;
            string[] nodes = null;
            string thisnode = "";
            string tag = "";
            int x = 0;
            int c = 0;
            Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            //get source node collection...
            try
            {
                string tmp = source;
                if (NodeParent.Length > 0)
                {
                    tmp = Tools.XNode(ref tmp, NodeParent);
                }
                nodes = tmp.Split(new string[] { "<" + NodeName + ">" }, StringSplitOptions.None);
                for (int i = 1; i < nodes.Length; i++)
                {
                    thisnode = nodes[i];

                    tag = Tools.NextNode(thisnode, ref x);
                    while (tag.Length > 0)
                    {
                        if (!data.ContainsKey(tag))
                        {
                            data.Add(tag, Tools.XNode(ref thisnode, tag).Trim());
                        }
                        else
                        {
                            Message("Import.DuplicateElement: A node '" + tag + "' already exists for this element.");
                            tag = "";
                            continue;
                        }
                        tag = Tools.NextNode(thisnode, ref x);
                        if (tag.StartsWith("/")) { tag = ""; }
                    }
                    //node loaded, save it...
                    if (!data.ContainsKey("created")) { data.Add("created", DateTime.Now.ToString()); }
                    if (!data.ContainsKey("edited")) { data.Add("edited", DateTime.Now.ToString()); }
                    x = Insert(data, TableName);
                    data.Clear();
                    c++;
                }           //each node

            }
            catch (Exception ex)
            {
                Error("Import:" + ex.Message);
                ok = false;
            }
            TimeSpan ts = new TimeSpan();
            ts = DateTime.Now - starttime;
            Message("Import complete. " + c + " rows inserted into table " + TableName + " in " + ts.TotalSeconds + " seconds.");
            return ok;
        }

        public class Export : DataTools
        {
            public string Format = "XML";
            public string RecordNode = "record";
            public string WrapperNode = "records";

            public bool CSVColumnHeadings = true;
            //other options and what not...
            public Dictionary<string, string> ColumnTranslations = new Dictionary<string, string>();


            public void XMLFile(string sqlstatement, string filename)
            {
                string results = XML(sqlstatement);
                Tools.WriteFile(filename, results);
            }
            public string XML(string sqlstatement)
            {
                string wrapper = WrapperNode;
                string node = RecordNode;
                string key = "";
                string value = "";

                if (wrapper.IsNullOrEmpty())
                {
                    wrapper = "records";
                }
                if (node.IsNullOrEmpty())
                {
                    node = "record";
                }
                Dictionary<string, string> data = new Dictionary<string, string>();
                Dictionary<string, string> translated = new Dictionary<string, string>();
                StringBuilder sb = new StringBuilder();
                GetResultSet(sqlstatement);
                while (!EOF)
                {
                    data.Clear();
                    translated.Clear();
                    data = GetRowStrings();
                    foreach (KeyValuePair<string, string> item in data)
                    {
                        key = item.Key;
                        value = item.Value;
                        if (ColumnTranslations.ContainsKey(key))
                        {
                            //transform the key to the value in translations dictionary
                            key = ColumnTranslations[key];
                        }
                        translated.Add(key, value);
                    }
                    sb.Append(Tools.ToXML(translated, node));
                }

                string results = Tools.XWrap(sb.ToString(), WrapperNode);
                return results;
            }

            public void CSVFile(string sqlstatement, string filename)
            {
                string results = CSV(sqlstatement);
                Tools.WriteFile(filename, results);
            }
            public string CSV(string sqlstatement)
            {

                GetResultSet(sqlstatement);
                StringBuilder sb = new StringBuilder();

                if (CSVColumnHeadings)
                {
                    IEnumerable<string> columns = ResultSet.Columns.Cast<DataColumn>().Select(column => ColumnTranslations.ContainsKey(column.ColumnName.ToLower()) ? ColumnTranslations[column.ColumnName] : column.ColumnName);
                    sb.AppendLine(string.Join(",", columns));
                }
                foreach (DataRow row in ResultSet.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                string results = sb.ToString();
                return results;
            }

        }
        //helper methods...

        private bool _isNumeric(string thevalue)
        {
            float result;
            return float.TryParse(thevalue, out result);
        }

        private string junkTrim(string source)
        {
            char[] junk = { ' ', ',', ';', '&', '.' };
            return source.Trim(junk);
        }
    }

}
