using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CS2010.Common
{
	/// <summary>Represents a single connection to a database</summary>
	public class ClsConnection
	{
		#region Constants

		/// <summary>Placeholder to use when the user has to be specified in
		/// the connection string</summary>
		public const string UserPlaceHolder = "<user>";
		/// <summary>Placeholder to use when the password has to be specified in
		/// the connection string</summary>
		public const string PwdPlaceHolder = "<pwd>";

		/// <summary>Prefix expected by MS SqlServer when specifying SQL
		/// statements with parameters</summary>
		public const string SqlParameterPrexix = "@";
		/// <summary>Prefix expected by the OleDb data provider when specifying
		/// SQL statements with parameters</summary>
		public const string OleParameterPrexix = "?";
		/// <summary>Prefix expected by Oracle when specifying SQL
		/// statements with parameters</summary>
		public const string OracleParameterPrexix = ":";

		/// <summary>SQL statement for getting the system date from
		/// an Oracle DB</summary>
		public const string OracleDateSQL = "SELECT SYSDATE FROM DUAL";
		/// <summary>SQL statement for getting the system date from
		/// a SqlServer DB</summary>
		public const string SqlDateSQL = "SELECT GETDATE()";

		#endregion		// #region Constants

		#region Fields

		/// <summary>Storage for the DbUserName property</summary>
		protected string _UserName;
		/// <summary>Storage for the DbPassword property</summary>
		protected string _Password;
		/// <summary>Storage for the DbContext property</summary>
		protected string _DbContext;
		/// <summary>Storage for the DbConnectionKey property</summary>
		protected string _DbConnectionKey;
		/// <summary>Storage for the DbProvider property</summary>
		protected string _DataProvider;
		/// <summary>Storage for the ConnectionString property</summary>
		protected string _ConnectionString;

		/// <summary>.NET database connection object used by this class</summary>
		protected DbConnection theConnection;
		/// <summary>Storage for the DbFactory property</summary>
		protected DbProviderFactory _DbFactory;
		/// <summary>Currently executing command which we can use to cancel searches</summary>
		protected DbCommand _CurrentCommand;

		/// <summary>Storage for the TransactionIsolationLevel property</summary>
        protected IsolationLevel _TransactionIsolationLevel =
            IsolationLevel.ReadCommitted;

		/// <summary>List of DbTransaction objects used to control transactions.
		/// When TransactionBegin is called a new transaction will be added to
		/// the list, when TransactionCommit or TransactionRollback is called,
		/// the last transaction in the list is removed.</summary>
		protected Stack<DbTransaction> theTransactions =
			new Stack<DbTransaction>();

		protected int? _CommandTimeout;

		#endregion		// #region Fields

		#region Public Properties

		/// <summary>Used to create instances of a provider's implementation
		/// of the data source classes (i.e. DbConnection, DbDataAdapter).
		/// The DbProvider property is used to create the appropriate
		/// factory object.</summary>
		public DbProviderFactory DbFactory
		{
			get
			{
				if( _DbFactory == null )
					_DbFactory = DbProviderFactories.
						GetFactory(_DataProvider);
				return _DbFactory;
			}
		}

		/// <summary>This provides access to the currently executing command object which we can
		/// use to cancel search operations</summary>
		public DbCommand CurrentCommand
		{
			get { return _CurrentCommand; }
		}

		/// <summary>Gets the connection string</summary>
		public string DbConnectionString
		{
			get { return _ConnectionString; }
		}

		/// <summary>Gets the context specified for an Oracle database</summary>
		public string DbContext
		{
			get { return _DbContext; }
		}

		/// <summary>Gets/Sets the key used to add this connection to a
		/// connection manager class/collection</summary>
		public string DbConnectionKey
		{
			get { return _DbConnectionKey; }
			set { _DbConnectionKey = value; }
		}

		/// <summary>Gets the data provider that was specified</summary>
		public string DbProvider
		{
			get { return _DataProvider; }
		}

		/// <summary>Returns true if this instance represents a Sql Server
		/// database, meaning a Sql Server data provider was specified</summary>
		public bool IsSQL
		{
			get
			{
				return ( _DataProvider.IndexOf("sql",
					StringComparison.InvariantCultureIgnoreCase) > 0 );
			}
		}

		/// <summary>Returns true if an OLE Db provider was specified</summary>
		public bool IsOLE
		{
			get
			{
				return ( _DataProvider.IndexOf("ole",
					StringComparison.InvariantCultureIgnoreCase) > 0 );
			}
		}

		/// <summary>Returns true if this instance represents an Oracle database,
		/// meaning an Oracle data provider was specified</summary>
		public bool IsOracle
		{
			get { return ( IsOLE == false && IsOLE == false ); }
		}

		/// <summary>Gets the username used to log into the database</summary>
		public string DbUserName
		{
			get { return _UserName; }
		}

		/// <summary>Gets the password used to log into the database</summary>
		public string DbPassword
		{
			get { return _Password; }
		}

		/// <summary>Returns true if the database connection is open</summary>
		public bool IsOpen
		{
			get
			{
				return ( theConnection != null )
					? theConnection.State != ConnectionState.Closed
					: false;
			}
		}

		/// <summary>Returns the number of transactions currently open</summary>
		public int DbTransactionCount { get { return theTransactions.Count; } }

		/// <summary>Returns true if one or more transactions are open</summary>
		public bool IsInTransaction
		{
			get { return ( theTransactions.Count > 0 ); }
		}

		/// <summary>Returns the IsolationLevel used for transactions</summary>
		public IsolationLevel DbTransactionIsolationLevel
		{
			get { return _TransactionIsolationLevel; }
			set
			{
				if( theTransactions.Count > 0 ) return;
				_TransactionIsolationLevel = value;
			}
		}

		public int? CommandTimeout
		{
			get { return _CommandTimeout; }
			set { _CommandTimeout = value; }
		}
		#endregion		// #region Properties

		#region Local Properties

		/// <summary>Gets the current transaction (the last one added to the
		/// list) or null if there are no transactions</summary>
		protected DbTransaction CurrentTransaction
		{
			get
			{
				return ( theTransactions.Count > 0 )
					? theTransactions.Peek() : null;
			}
		}

		/// <summary>Gets the prefix to use for command parameters which varies
		/// based on the provider type</summary>
		protected string ParameterPrefix
		{
			get
			{
				if( IsOLE == true ) return OleParameterPrexix;
				if( IsSQL == true ) return SqlParameterPrexix;
				return OracleParameterPrexix;
			}
		}
		#endregion		// #region Local Properties

		#region Constructors

		/// <summary>Default constructor (connection string is set using the
		/// database properties of the ClsEnvironmentInfo class
		/// (ConnectionStringName, UserName, Password, Contract_Cd)
		/// with a call to SetConnectionString)</summary>
		public ClsConnection()
		{
			ConnectionStringSettings cnInfo = ConfigurationManager.
				ConnectionStrings[ClsEnvironment.ConnectionStringName];
			SetConnectionString(cnInfo.ConnectionString, cnInfo.ProviderName,
				ClsEnvironment.UserName, ClsEnvironment.Password,
				ClsEnvironment.Contract_Cd);
		}

		/// <summary>Constructor expecting a connection string and a data
		/// provider (i.e. "Oracle.DataAccess.Client")</summary>
		/// <param name="connectionString">Database connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		public ClsConnection(string connectionString, string dataProvider)
		{
			SetConnectionString(connectionString, dataProvider);
		}

		/// <summary>Constructor expecting a connection string, a data provider
		/// (i.e. "Oracle.DataAccess.Client"), username and password</summary>
		/// <param name="connectionString">Database connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		/// <param name="user">Database user name (will replace the user place
		/// holder in the connection string if any,
		/// i.e. user=&lt;user&gt;)</param>
		/// <param name="pwd">Database password (will replace the password place
		/// holder in the connection string if any,
		/// i.e. password=&lt;pwd&gt;)</param>
		public ClsConnection(string connectionString, string dataProvider,
			string user, string pwd)
		{
			SetConnectionString(connectionString, dataProvider, user, pwd);
		}

		/// <summary>Constructor expecting a connection string, a data provider
		/// (i.e. "Oracle.DataAccess.Client"), username/password, and
		/// an Oracle context string (i.e. "GPC2")</summary>
		/// <param name="connectionString">Database connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		/// <param name="user">Database user name (will replace the user place
		/// holder in the connection string if any,
		/// i.e. user=&lt;user&gt;)</param>
		/// <param name="pwd">Database password (will replace the password place
		/// holder in the connection string if any,
		/// i.e. password=&lt;pwd&gt;)</param>
		/// <param name="contextCd">Database context (i.e. "GPC2")</param>
		public ClsConnection(string connectionString, string dataProvider,
			string user, string pwd, string contextCd)
		{
			SetConnectionString(connectionString, dataProvider, user, pwd, contextCd);
		}
		#endregion		// #region Constructors

		#region Helper methods

		/// <summary>Connect to the database (creates the DB factory and
		/// Db connection objects if necessary)</summary>
		private void Connect()
		{
			if( IsOpen == false )
			{
				if( theConnection == null )
				{
					theConnection = DbFactory.CreateConnection();
					theConnection.ConnectionString = _ConnectionString;
				}

				theConnection.Open();

				if (ClsEnvironment.ConnectionKey == "CLASS")	SetContext();
			}
		}

		/// <summary>Used to set the oracle context, and has to be called every
		/// time we connect to the database)</summary>
		private void SetContext()
		{
			if( string.IsNullOrEmpty(_DbContext) == true ) return;

			DbCommand cmd = theConnection.CreateCommand();
			cmd.CommandText = "PKG_CLASS_ACCESS.P_SET_CLASS_ENV";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(GetParameter("I_CONTRACT_CD", _DbContext));

			cmd.ExecuteNonQuery();
		}

		/// <summary>Close the connection. This will rollback all uncommitted
		/// transactions and will free the connection and transaction objects</summary>
		private void Close()
		{
			while( theTransactions.Count > 0 )
			{
				DbTransaction tr = theTransactions.Pop();
				tr.Rollback();
				tr.Dispose();
			}
			if( theConnection != null )
			{
				theConnection.Close();
				theConnection.Dispose();
				theConnection = null;
			}
		}

		/// <summary>Create a command object from the given sql</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">Command type (i.e. SP vs sql text)</param>
		/// <param name="parameters">Array of parameters if any</param>
		/// <returns>The DbCommand object</returns>
		private DbCommand CreateCommand(string sql, CommandType cmdType,
			DbParameter[] parameters)
		{
			DbCommand cmd = theConnection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = cmdType;
			cmd.Transaction = CurrentTransaction;
			if( parameters != null && parameters.Length > 0 )
				cmd.Parameters.AddRange(parameters);

			PrepareCommand(cmd);

			_CurrentCommand = cmd;
			return cmd;
		}

		/// <summary>Modifies the syntax of any named parameters in the SQL
		/// statement for the given command to match the syntax required
		/// for the current data provider</summary>
		/// <param name="cmd">The command object to modify</param>
		private void PrepareCommand(DbCommand cmd)
		{
			if( IsSQL == true ) return;

			string s;
			if( IsOLE == true )
				s = Regex.Replace(cmd.CommandText, @"@\w*", "?");
			else if( IsOracle == true )
				s = cmd.CommandText.Replace('@', ':');
			else
				s = cmd.CommandText;

			cmd.CommandText = s.Replace("<DBLINK>", "@");
		}
		#endregion		// #region Helper methods

		#region Public connection string methods

		/// <summary>Sets the connection string to the given value and uses the
		/// specified data provider (i.e. "Oracle.DataAccess.Client"). Note: this
		/// connection string will not take effect until the next time a
		/// DbConnection object is instantiated, which will most likely be after
		/// the current connection (if any) is closed.</summary>
		/// <param name="connectionString">The connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		/// <returns>Returns the new connection string</returns>
		public string SetConnectionString(string connectionString,
			string dataProvider)
		{
			_DataProvider = dataProvider;
			_ConnectionString = connectionString;
			return _ConnectionString;
		}

		/// <summary>Sets the connection string to the given value and uses the
		/// specified data provider (i.e. "Oracle.DataAccess.Client"). The given
		/// username and password will be used to log into the database if and
		/// only if placeholders exist in the connection string for the two
		/// fields (i.e. user=&lt;user&gt;;password=&lt;pwd&gt;). Note: this
		/// connection string will not take effect until the next time a
		/// DbConnection object is instantiated, which will most likely be after
		/// the current connection (if any) is closed.</summary>
		/// <param name="connectionString">The connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		/// <param name="user">Database user name (will replace the user place
		/// holder in the connection string if any,
		/// i.e. user=&lt;user&gt;)</param>
		/// <param name="pwd">Database password (will replace the password place
		/// holder in the connection string if any,
		/// i.e. password=&lt;pwd&gt;)</param>
		/// <returns>Returns the new connection string</returns>
		public string SetConnectionString(string connectionString,
			string dataProvider, string user, string pwd)
		{
			_DataProvider = dataProvider;
			_UserName = user;
			_Password = pwd;

			StringBuilder sb = new StringBuilder(connectionString);
			sb.Replace(UserPlaceHolder, _UserName).Replace(PwdPlaceHolder, _Password);
			_ConnectionString = sb.ToString();
			return _ConnectionString;
		}

		/// <summary>Sets the connection string to the given value and uses the
		/// specified data provider (i.e. "Oracle.DataAccess.Client"). The given
		/// username and password will be used to log into the database if and
		/// only if placeholders exist in the connection string for the two
		/// fields (i.e. user=&lt;user&gt;;password=&lt;pwd&gt;). The contextCd
		/// is used to set a policy in an Oracle database. Note: this
		/// connection string will not take effect until the next time a
		/// DbConnection object is instantiated, which will most likely be after
		/// the current connection (if any) is closed.</summary>
		/// <param name="connectionString">The connection string</param>
		/// <param name="dataProvider">Data provider
		/// (i.e. "Oracle.DataAccess.Client")</param>
		/// <param name="user">Database user name (will replace the user place
		/// holder in the connection string if any,
		/// i.e. user=&lt;user&gt;)</param>
		/// <param name="pwd">Database password (will replace the password place
		/// holder in the connection string if any,
		/// i.e. password=&lt;pwd&gt;)</param>
		/// <param name="contextCd">The context to set in an Oracle database</param>
		/// <returns>Returns the new connection string</returns>
		public string SetConnectionString(string connectionString,
			string dataProvider, string user, string pwd, string contextCd)
		{
			_DataProvider = dataProvider;
			_DbContext = contextCd;
			_UserName = user;
			_Password = pwd;

			StringBuilder sb = new StringBuilder(connectionString);
			sb.Replace(UserPlaceHolder, _UserName).Replace(PwdPlaceHolder, _Password);
			_ConnectionString = sb.ToString();

			return _ConnectionString;
		}

		public void GetPropertiesFromConnectionString(out string user, out string pwd, out string dataSource)
		{
			DbConnectionStringBuilder dbsb = new DbConnectionStringBuilder();
			dbsb.ConnectionString = DbConnectionString;
			user = (dbsb.ContainsKey("User ID")) ? ClsConvert.ToString(dbsb["User ID"]) : null;
			pwd = (dbsb.ContainsKey("password")) ? ClsConvert.ToString(dbsb["password"]) : null;
			dataSource = (dbsb.ContainsKey("Data Source")) ? ClsConvert.ToString(dbsb["Data Source"]) : null;
		}
		#endregion		// #region Public connection string methods

		#region Public transaction methods

		/// <summary>Begins a transaction</summary>
		public void TransactionBegin()
		{
			if( theConnection == null ) Connect();
			theTransactions.Push(
				theConnection.BeginTransaction(_TransactionIsolationLevel));
			if( theTransactions.Count > 1 )
				ClsErrorHandler.LogError("Transaction count > 1: {0}", theTransactions.Count);
		}

		/// <summary>Commits a transaction</summary>
		public void TransactionCommit()
		{
			if( theConnection == null )
				throw new ApplicationException
					("Call to Commit without a call to Connect");
			if( theTransactions.Count <= 0 )
				throw new ApplicationException
					("Call to Commit without a call to BeginTransaction");
			theTransactions.Pop().Commit();
			if( theTransactions.Count < 1 ) Close();
		}

		/// <summary>Rollsback a transaction</summary>
		public void TransactionRollback()
		{
			if( theConnection == null )
				throw new ApplicationException
					("Call to Rollback without a call to Connect");
			if( theTransactions.Count <= 0 )
				throw new ApplicationException
					("Call to Rollback without a call to BeginTransaction");
			theTransactions.Pop().Rollback();
			if( theTransactions.Count < 1 ) Close();
		}

		/// <summary>Starts a transaction but only if one does not already exist</summary>
		/// <returns>True if a transaction was started, false if already in a transaction</returns>
		public bool TransactionStart()
		{
			if( IsInTransaction ) return false;

			TransactionBegin();
			return true;
		}

		/// <summary>Ends a transaction based on the passed value (use true to
		/// commit the transaction, use false to roll it back)</summary>
		/// <param name="commit">True if the transaction should be committed,
		/// false if the transaction should be rolled back</param>
		public void TransactionEnd(bool commit)
		{
			if( theConnection == null )
				throw new ApplicationException
					("Call to End without a call to Connect");
			if( theTransactions.Count <= 0 )
				throw new ApplicationException
					("Call to End without a call to BeginTransaction");
			if( commit == true )
				theTransactions.Pop().Commit();
			else
				theTransactions.Pop().Rollback();
			if( theTransactions.Count < 1 ) Close();
		}
		#endregion		// #region Public transaction methods

		#region Public Parameter methods

		/// <summary>Gets a DbParameter object based on the given name
		/// and value</summary>
		/// <param name="name">Parameter name without a prefix
		/// (i.e. no leading @ sign)</param>
		/// <param name="value">Parameter value</param>
		/// <returns>DbParameter object</returns>
		public DbParameter GetParameter(string name, object value)
		{
			DbParameter p = DbFactory.CreateParameter();
			p.Direction = ParameterDirection.Input;
			p.ParameterName = name;

			p.Value = ClsConvert.ToDbObject(value);

			return p;
		}

		/// <summary>Gets a DbParameter object based on the given name,
		/// value and direction</summary>
		/// <param name="name">Parameter name without a prefix
		/// (i.e. no leading @ sign)</param>
		/// <param name="value">Parameter value</param>
		/// <param name="direction">Parameter direction
		/// (i.e. Input, Output, etc.)</param>
		/// <param name="paramType">The type of parameter (generally not needed
		/// and most often used to specify a LOBs as DbType.Binary)</param>
		/// <returns>DbParameter object</returns>
		public DbParameter GetParameter(string name, object value,
			ParameterDirection direction, DbType paramType, int paramSize)
		{
			DbParameter p = DbFactory.CreateParameter();
			p.DbType = paramType;
			p.Direction = direction;
			p.ParameterName = name;

			if( paramType == DbType.Binary )
			{
				byte[] b = value as byte[];
				if( b != null ) p.Size = b.Length;
			}
			else
				p.Size = paramSize;

			p.Value = ClsConvert.ToDbObject(value);

			return p;
		}
		#endregion		// #region Public Parameter methods

		#region Public Run SQL methods

		/// <summary>Runs a SQL statement. Note: to run a stored procedure, use
		/// the overload of this method that accepts a command type
		/// (CommandType.StoredProcedure). To get DataTables or DataRows use
		/// the GetDataTable and GetDataRow methods</summary>
		/// <param name="sql">The SQL statement to run</param>
		/// <returns>Returns the result of the DbCommand.ExecuteNonQuery
		/// method</returns>
		public int RunSQL(string sql)
		{
			return RunSQL(sql, CommandType.Text, null);
		}

		/// <summary>Runs a SQL statement with a set of parameters. Note: to run
		/// a stored procedure, use the overload of this method that accepts a
		/// command type (CommandType.StoredProcedure). To get DataTables or
		/// DataRows use the GetDataTable and GetDataRow methods</summary>
		/// <param name="sql">The SQL statement to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>Returns the result of the DbCommand.ExecuteNonQuery
		/// method</returns>
		public int RunSQL(string sql, DbParameter[] parameters)
		{
			return RunSQL(sql, CommandType.Text, parameters);
		}

		/// <summary>Runs a SQL statement with a set of parameters. Note: to run
		/// a stored procedure, specify CommandType.StoredProcedure as the comand
		/// type. Also, to get DataTables or DataRows use the GetDataTable and
		/// GetDataRow methods</summary>
		/// <param name="sql">The SQL statement or stored procedure to run</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <returns>Returns the result of the DbCommand.ExecuteNonQuery
		/// method</returns>
		public int RunSQL(string sql, CommandType cmdType)
		{
			return RunSQL(sql, cmdType, null);
		}

		/// <summary>Runs a SQL statement with a set of parameters. Note: to run
		/// a stored procedure, specify CommandType.StoredProcedure as the comand
		/// type. To get DataTables or DataRows use the GetDataTable and
		/// GetDataRow methods</summary>
		/// <param name="sql">The SQL statement or stored procedure to run</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>Returns the result of the DbCommand.ExecuteNonQuery
		/// method</returns>
		public int RunSQL(string sql, CommandType cmdType,
			DbParameter[] parameters)
		{
            try
            {
                int iRC;
                Connect();

				using( DbCommand cmd = CreateCommand(sql, cmdType, parameters) )
				{
                    DateTime tStart = DateTime.Now;
					iRC = cmd.ExecuteNonQuery();
                    DateTime tEnd = DateTime.Now;
                    TimeSpan ts = tEnd - tStart;
					return iRC;
				}
            }
			finally
			{
				if( CurrentTransaction == null ) Close();
				_CurrentCommand = null;
			}
		}
		#endregion		// #region Public Run SQL methods

		#region Public Scalar methods

		/// <summary>Calls the command object's ExecuteScalar method with the
		/// given SQL statement. Note: to run a stored procedure, use
		/// the overload of this method that accepts a command type
		/// (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <returns>The object returned by the SQL statement</returns>
		public object GetScalar(string sql)
		{
			return GetScalar(sql, CommandType.Text, null);
		}

		/// <summary>Calls the command object's ExecuteScalar method with the
		/// given SQL statement and parameters. Note: to run a stored procedure,
		/// use the overload of this method that accepts a command type
		/// (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement to execute</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>The object returned by the SQL statement</returns>
		public object GetScalar(string sql, DbParameter[] parameters)
		{
			return GetScalar(sql, CommandType.Text, parameters);
		}

		/// <summary>Calls the command object's ExecuteScalar method with the
		/// given SQL statement. Note: to run a stored procedure, specify
		/// CommandType.StoredProcedure as the comand type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <returns>The object returned by the SQL statement</returns>
		public object GetScalar(string sql, CommandType cmdType)
		{
			return GetScalar(sql, cmdType, null);
		}

		/// <summary>Calls the command object's ExecuteScalar method with the
		/// given SQL statement and the given parameters. Note: to run a stored
		/// procedure, specify StoredProcedure as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>The object returned by the SQL statement</returns>
		public object GetScalar(string sql, CommandType cmdType,
			DbParameter[] parameters)
		{
			try
			{
				Connect();

				using( DbCommand cmd = CreateCommand(sql, cmdType, parameters) )
				{
					return cmd.ExecuteScalar();
				}
			}
			finally
			{
				if( CurrentTransaction == null ) Close();
				_CurrentCommand = null;
			}
		}
		#endregion		// #region Public Scalar methods

		#region Unique Identifier Methods

		/// <summary>Get the next value for a column in a table. The values for
		/// the column will consist of a prefix followed by a number. The prefix
		/// will be a combination of the 'startsWith' and type' parameters. The
		/// table is searched for the max value that begins with the prefix. The
		/// number portion is extracted from the max value and incremented by 1.
		/// The new number is appended to the prefix and returned.</summary>
		/// <param name="tabName">The table to examine</param>
		/// <param name="colName">The column to compute the value for</param>
		/// <param name="startsWith">Combined with the 'type' parameter to
		/// form the prefix.</param>
		/// <param name="tableIdentifier">Appended to the 'startsWith' parameter to form
		/// the prefix (identifies the table)</param>
		/// <param name="appIdentifier">Also appended to startsWith to form
		/// the prefix (identifies the application or contract)</param>
		/// <returns>The next value for a column in a table</returns>
		public string GetMaxNo(string tabName, string colName, string startsWith,
			char tableIdentifier, char appIdentifier)
		{
			StringBuilder sbPrefix = new StringBuilder(startsWith);
			if( tableIdentifier != 0 ) sbPrefix.Append(tableIdentifier);
			if( appIdentifier != 0 ) sbPrefix.Append(appIdentifier);

			string maxSql = string.Format
				("SELECT MAX({0}) FROM {1} WHERE {0} LIKE @PREFIX",
				colName, tabName);
			DbParameter[] p = new DbParameter[1];
			p[0] = GetParameter("@PREFIX", sbPrefix.ToString() + '%');

			string maxNo = ClsConvert.ToString(GetScalar(maxSql, p));

			uint currNo = 0;
			if( string.IsNullOrEmpty(maxNo) == false )
			{
				string suffix = maxNo.Substring(sbPrefix.Length);
				currNo = ClsConvert.ToUInt32(suffix);
			}

			currNo++;
			string sCurr = currNo.ToString();

			int suffixLen = 10 - sbPrefix.Length;
			string seqNo = sCurr.PadLeft(suffixLen, '0');

			sbPrefix.Append(seqNo);
			return sbPrefix.ToString();
		}

		/// <summary>Get the next value for a column in a table. The values for
		/// the column will consist of a prefix followed by a number. The prefix
		/// will be a combination of the 'startsWith' and type' parameters. The
		/// table is searched for the max value that begins with the prefix. The
		/// number portion is extracted from the max value and incremented by 1.
		/// The new number is appended to the prefix and returned.</summary>
		/// <param name="tabName">The table to examine</param>
		/// <param name="colName">The column to compute the value for</param>
		/// <param name="startsWith">Combined with the 'type' parameter to
		/// form the prefix.</param>
		/// <param name="tableIdentifier">Appended to the 'startsWith' parameter to form
		/// the prefix (identifies the table)</param>
		/// <param name="appIdentifier">Also appended to startsWith to form
		/// the prefix (identifies the application or contract)</param>
		/// <param name="whereClause">Optional where clause</param>
		/// <returns>The next value for a column in a table</returns>
		public string GetMaxNo(string tabName, string colName, string startsWith,
			char tableIdentifier, char appIdentifier, string whereClause)
		{
			StringBuilder sbPrefix = new StringBuilder(startsWith);
			if( tableIdentifier != 0 ) sbPrefix.Append(tableIdentifier);
			if( appIdentifier != 0 ) sbPrefix.Append(appIdentifier);

			string maxSql = string.Format
				("SELECT MAX({0}) FROM {1} WHERE {0} LIKE @PREFIX {2}",
				colName, tabName,
				( !string.IsNullOrEmpty(whereClause) ? " AND " + whereClause : null ));
			DbParameter[] p = new DbParameter[1];
			p[0] = GetParameter("@PREFIX", sbPrefix.ToString() + '%');

			string maxNo = ClsConvert.ToString(GetScalar(maxSql, p));

			uint currNo = 0;
			if( string.IsNullOrEmpty(maxNo) == false )
			{
				string suffix = maxNo.Substring(sbPrefix.Length);
				currNo = ClsConvert.ToUInt32(suffix);
			}

			currNo++;
			string sCurr = currNo.ToString();

			int suffixLen = 10 - sbPrefix.Length;
			string seqNo = sCurr.PadLeft(suffixLen, '0');

			sbPrefix.Append(seqNo);
			return sbPrefix.ToString();
		}

		/// <summary>Get the next value for a column in a table. The values for
		/// the column will consist of a prefix followed by a number. The prefix
		/// will be a combination of the 'startsWith' and type' parameters. The
		/// table is searched for the max value that begins with the prefix. The
		/// number portion is extracted from the max value and incremented by 1.
		/// The new number is appended to the prefix and returned.</summary>
		/// <param name="tabName">The table to examine</param>
		/// <param name="colName">The column to compute the value for</param>
		/// <param name="startsWith">Combined with the 'type' parameter to
		/// form the prefix.</param>
		/// <param name="tableIdentifier">Appended to the 'startsWith' parameter to form
		/// the prefix (identifies the table)</param>
		/// <param name="appIdentifier">Also appended to startsWith to form
		/// the prefix (identifies the application or contract)</param>
		/// <returns>The next value for a column in a table</returns>
		public string GetMaxNo(string tabName, string colName, string startsWith,
			byte fieldLength)
		{
			StringBuilder sbPrefix = new StringBuilder(startsWith);

			string maxSql = string.Format("SELECT MAX({0}) FROM {1} WHERE {0} LIKE @PREFIX",
				colName, tabName);
			DbParameter[] p = new DbParameter[1];
			p[0] = GetParameter("@PREFIX", sbPrefix.ToString() + '%');

			string maxNo = ClsConvert.ToString(GetScalar(maxSql, p));

			uint currNo = 0;
			if( string.IsNullOrEmpty(maxNo) == false )
			{
				string suffix = maxNo.Substring(sbPrefix.Length);
				currNo = ClsConvert.ToUInt32(suffix);
			}

			// what happens when we run out of unique keys?
			currNo++;
			string sCurr = currNo.ToString();

			int suffixLen = fieldLength - sbPrefix.Length;
			string seqNo = sCurr.PadLeft(suffixLen, '0');

			sbPrefix.Append(seqNo);
			return sbPrefix.ToString();
		}
		#endregion		// #region Unique Identifier Methods

		#region Public GetDataTable methods

		/// <summary>Select all rows from the specified DB table</summary>
		/// <param name="tableName">The name of the database table</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public DataTable GetTable(string tableName)
		{
			return GetDataTable(string.Format("SELECT * FROM {0}", tableName));
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public DataTable GetDataTable(string sql)
		{
			return GetDataTable(sql, CommandType.Text, null);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public DataTable GetDataTable(string sql, DbParameter[] parameters)
		{
			return GetDataTable(sql, CommandType.Text, parameters);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public DataTable GetDataTable(string sql, CommandType cmdType)
		{
			return GetDataTable(sql, cmdType, null);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public DataTable GetDataTable(string sql, CommandType cmdType,
			DbParameter[] parameters)
		{
			try
			{
				Connect();

				using( DbCommand cmd = CreateCommand(sql, cmdType, parameters) )
				{
					if( CommandTimeout != null && CommandTimeout >= 0 )
						cmd.CommandTimeout = CommandTimeout.Value;
					using( DbDataAdapter da = DbFactory.CreateDataAdapter() )
					{
						da.SelectCommand = cmd;

						DataTable dt = new DataTable("Table");
						da.Fill(dt);
						return dt;
					}
				}
			}
			finally
			{
				if( CurrentTransaction == null ) Close();
				_CurrentCommand = null;
			}
		}
		#endregion		// #region Public GetDataTable methods

		#region Public GetDataRow methods

		/// <summary>Gets a DataRow from the given SQL statement. Note: to get
		/// a DataRow from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		/// <remarks>This method is equivalent to running GetDataTable
		/// and selecting the first row of the resulting DataTable</remarks>
		public DataRow GetDataRow(string sql)
		{
			return GetDataRow(sql, CommandType.Text, null);
		}

		/// <summary>Gets a DataRow from the given SQL statement. Note: to get
		/// a DataRow from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		/// <remarks>This method is equivalent to running GetDataTable
		/// and selecting the first row of the resulting DataTable</remarks>
		public DataRow GetDataRow(string sql, DbParameter[] parameters)
		{
			return GetDataRow(sql, CommandType.Text, parameters);
		}

		/// <summary>Gets a DataRow from the given SQL statement. Note: to get
		/// a DataRow from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		/// <remarks>This method is equivalent to running GetDataTable
		/// and selecting the first row of the resulting DataTable</remarks>
		public DataRow GetDataRow(string sql, CommandType cmdType)
		{
			return GetDataRow(sql, cmdType, null);
		}

		/// <summary>Gets a DataRow from the given SQL statement. Note: to get
		/// a DataRow from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		/// <remarks>This method is equivalent to running GetDataTable
		/// and selecting the first row of the resulting DataTable</remarks>
		public DataRow GetDataRow(string sql, CommandType cmdType,
			DbParameter[] parameters)
		{
			DataTable dt = GetDataTable(sql, cmdType, parameters);
			return ( dt != null && dt.Rows.Count > 0 ) ? dt.Rows[0] : null;
		}

		/// <summary>Gets the row from the specified table with the given
		/// primary key value. Note: this overload is used when there is only
		/// one primary key column. Use the overload that expects arrays for
		/// both the primary key names and values when the table has a primary
		/// key consisting of multiple columns</summary>
		/// <param name="tableName">The name of the DB table</param>
		/// <param name="pkName">The name of the primary key column</param>
		/// <param name="pkValue">The primary key value</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		public DataRow GetDataRowUsingKey(string tableName, string pkName,
			object pkValue)
		{
			string sql = string.Format
				("SELECT * FROM {0} WHERE {1}=@{1}", tableName, pkName);

			DbParameter[] p = new DbParameter[1];
			p[0] = GetParameter(pkName, pkValue);

			DataTable dt = GetDataTable(sql, CommandType.Text, p);
			return ( dt != null && dt.Rows.Count > 0 ) ? dt.Rows[0] : null;
		}

		/// <summary>Gets the row from the specified table with the specified
		/// values of the primary key columns. Note: this overload is used when
		/// the primary key consists of multiple columns. Use the overload that
		/// expects one column name and one value when the primary key consists
		/// of only one column.</summary>
		/// <param name="tableName">The name of the DB table</param>
		/// <param name="pkName">An array of primary key column names</param>
		/// <param name="pkValue">An array of primary key values</param>
		/// <returns>A DataRow if a row was found, or null if not</returns>
		public DataRow GetDataRowUsingKey(string tableName, string[] pkNames,
			object[] pkValues)
		{
			if( pkNames.Length != pkValues.Length )
				throw new Exception(
					"Number of primary keys does not match " +
					"number of primary key values");

			DbParameter[] p = new DbParameter[pkValues.Length];

			StringBuilder sb = new StringBuilder();
			for( int i = 0; i < pkNames.Length; i++ )
			{
				p[i] = GetParameter(pkNames[i], pkValues[i]);
				sb.AppendFormat(" {0} {1}=@{1}",
					( i == 0 ? "WHERE" : "AND" ), pkNames[i]);
			}

			sb.Insert(0, tableName);
			sb.Insert(0, "SELECT * FROM ");

			DataTable dt = GetDataTable(sb.ToString(), CommandType.Text, p);
			return ( dt != null && dt.Rows.Count > 0 ) ? dt.Rows[0] : null;
		}
		#endregion		// #region Public get data methods

		#region Public miscellaneous methods

		/// <summary>Returns the system date from the database</summary>
		/// <returns>A DateTime object representing the time on the database
		/// server. An exception will be thrown if the method fails</returns>
		public DateTime GetSystemDate()
		{
			string sql = ( IsOracle == true ) ? OracleDateSQL : SqlDateSQL;
			object val = GetScalar(sql);
			DateTime? dtSys = ClsConvert.ToDateTimeNullable(val);
			if( dtSys == null ) throw new Exception("Unable to get system date");
			return dtSys.Value;
		}

		public static void AddColumns(DataTable dt, string cdCol, string dscCol)
		{
			if( dt == null ) return;

			dt.Columns.Add(cdCol + dscCol, typeof(string),
				string.Format("{0} + ' - ' + {1}", cdCol, dscCol));
			dt.Columns.Add(dscCol + cdCol, typeof(string),
				string.Format("{1} + ' - ' + {0}", cdCol, dscCol));
		}
		#endregion		// #region Public miscellaneous methods

		#region Public DataReader/List methods

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public List<T> GetList<T>(string sql)
			where T : ClsBaseTable, new()
		{
			return GetList<T>(sql, CommandType.Text, null);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure use the overload of this method
		/// that accepts a command type (CommandType.StoredProcedure).</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public List<T> GetList<T>(string sql, DbParameter[] parameters)
			where T : ClsBaseTable, new()
		{
			return GetList<T>(sql, CommandType.Text, parameters);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public List<T> GetList<T>(string sql, CommandType cmdType)
			where T : ClsBaseTable, new()
		{
			return GetList<T>(sql, cmdType, null);
		}

		/// <summary>Gets a DataTable from the given SQL statement. Note: to get
		/// a DataTable from a stored procedure specify
		/// (CommandType.StoredProcedure) as the command type.</summary>
		/// <param name="sql">The SQL statement</param>
		/// <param name="cmdType">The type of command to run</param>
		/// <param name="parameters">An array of parameters</param>
		/// <returns>A DataTable that may or may not contain any rows</returns>
		public List<T> GetList<T>(string sql, CommandType cmdType,
			DbParameter[] parameters) where T : ClsBaseTable, new()
		{
			DbDataReader reader = null;
			bool newTx = !IsInTransaction;
			try
			{
				if( newTx ) TransactionBegin();

				using( DbCommand cmd = CreateCommand(sql, cmdType, parameters) )
				{
					reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
					List<T> lst = new List<T>();
					while( reader.Read() == true )
					{
						T obj = new T();
						obj.LoadFromDataReader(reader);
						lst.Add(obj);
					}
					return lst;
				}
			}
			finally
			{
				if( reader != null )
				{
					reader.Close();
					reader.Dispose();
				}
				if( newTx ) TransactionRollback();
			}
		}
		#endregion		// #region Public DataReader/List methods

		#region Where clause builder

		public ClsConnection OpenParentheses(StringBuilder sb, string type)
		{
			sb.AppendFormat("\r\n\t {0} (", type);
			return this;
		}

		public void CloseParentheses(StringBuilder sb)
		{
			sb.Append(")");
		}

		public void AppendEqualClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, object val)
		{
			if( val == null ) return;

			sb.AppendFormat("\r\n\t {0} {1} = {2} ", type, col, var);
			p.Add(GetParameter(var, val));
		}

		public void AppendNotEqualClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, object val)
		{
			if( val == null ) return;

			sb.AppendFormat("\r\n\t {0} {1} <> {2} ", type, col, var);
			p.Add(GetParameter(var, val));
		}

		public void AppendGTClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, object val, bool orEqual)
		{
			if( val == null ) return;

			string op = ( orEqual ) ? ">=" : ">";
			sb.AppendFormat("\r\n\t {0} {1} {2} {3} ", type, col, op, var);
			p.Add(GetParameter(var, val));
		}

		public void AppendLTClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, object val, bool orEqual)
		{
			if( val == null ) return;

			string op = ( orEqual ) ? "<=" : "<";
			sb.AppendFormat("\r\n\t {0} {1} {2} {3} ", type, col, op, var);
			p.Add(GetParameter(var, val));
		}

		public void AppendLikeClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return;

			sb.AppendFormat("\r\n\t {0} {1} LIKE {2} ", type, col, var);
			p.Add(GetParameter(var, val.Replace('*', '%')));
		}

		public void AppendInClause(StringBuilder sb, 
			string type, string col, string val)
		{
			if (string.IsNullOrEmpty(val)) return;

			sb.AppendFormat("\r\n\t {0} {1} IN ({2}) ", type, col, val);
		}


		public void AppendInClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return;

			sb.AppendFormat("\r\n\t {0} {1} IN ({2}) ", type, col, var);
			sb.Replace(var, val);
		}

		public void AppendInClause2(StringBuilder sb, List<DbParameter> p,
			string type, string col, string paramName, string val)
		{
			if (string.IsNullOrWhiteSpace(val) == true) return;

			string[] inVals = val.Split(',');
			IEnumerable<string> trVals = inVals.Select(s => ((s != null) ? s.Trim() : null));
			string[] paramValues = trVals.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

			//string[] vals = rows.Select(dr => ClsConvert.ToString(dr["partner_request_cd"])).ToArray();

			//string[] tags = new string[] { "ruby", "rails", "scruffy", "rubyonrails" };
			//string cmdText = "SELECT * FROM Tags WHERE Name IN ({0})";

			string[] paramNames = paramValues.Select((s, i) => paramName + i.ToString()).ToArray();
			string inClause = string.Join(",", paramNames);
			for (int i = 0; i < paramNames.Length; i++)
			{
				p.Add(GetParameter(paramNames[i], paramValues[i]));
			}
			/*using (SqlCommand cmd = new SqlCommand(string.Format(cmdText, inClause)))
			{
				for (int i = 0; i < paramNames.Length; i++)
				{
					cmd.Parameters.AddWithValue(paramNames[i], tags[i]);
				}
			}*/

			//AppendInClause3(sb, p, "AND", "l.lo", "@ST_CD", new string[] { "1", "33", "333" });
			sb.AppendFormat("\r\n\t {0} {1} IN ({2}) ", type, col, inClause);
		}

		public void AppendInClause3<T>(StringBuilder sb, List<DbParameter> p,
			string type, string col, string paramName, T[] vals) where T : struct
		{
			if (vals == null || vals.Length <= 0) return;

			string[] paramNames = vals.Select((s, i) => paramName + i.ToString()).ToArray();
			string inClause = string.Join(",", paramNames);
			for (int i = 0; i < paramNames.Length; i++)
			{
				p.Add(GetParameter(paramNames[i], vals[i]));
			}

			sb.AppendFormat("\r\n\t {0} {1} IN ({2}) ", type, col, inClause);
		}

		public void AppendInClause3(StringBuilder sb, List<DbParameter> p,
			string type, string col, string paramName, string[] vals)
		{
			if (vals == null || vals.Length <= 0) return;

			string[] paramNames = vals.Select((s, i) => paramName + i.ToString()).ToArray();
			string inClause = string.Join(",", paramNames);
			for (int i = 0; i < paramNames.Length; i++)
			{
				p.Add(GetParameter(paramNames[i], vals[i]));
			}

			sb.AppendFormat("\r\n\t {0} {1} IN ({2}) ", type, col, inClause);
		}

		public void AppendInOrLike(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, string val)
		{
			if( string.IsNullOrEmpty(val) ) return;

			if( val.Contains(",") )
			{
				string tmp = val.Replace(" ", null);
				string newVal = ClsConvert.AddQuotes(tmp);
				AppendInClause(sb, p, type, col, var, newVal);
			}
			else
				AppendLikeClause(sb, p, type, col, var, val);
		}

		public void AppendNotInClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var, string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return;

			sb.AppendFormat("\r\n\t {0} {1} NOT IN ({2}) ", type, col, var);
			sb.Replace(var, val);
		}

		public void AppendDateClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var1, string var2, DateRange val)
		{
            AppendDateClause(sb, p, type, col, var1, var2, val, true);
		}

        public void AppendDateClause(StringBuilder sb, List<DbParameter> p,
            string type, string col, string var1, string var2, DateRange val, bool IncludeNullDates)
        {
            if (!IncludeNullDates)
                sb.AppendFormat(" and {0} is not null ", col);
            if (val.IsEmpty == true) return;

            sb.AppendFormat("\r\n\t {0} {1} ", type, val.WhereClause(col, var1, var2));

            if (val.From != null) p.Add(GetParameter(var1, val.From));
            if (val.To != null) p.Add(GetParameter(var2, val.To));


        }

		public void AppendDateClauseSqlServer(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var1, string var2, DateRange val)
		{
			if( val.IsEmpty == true ) return;

			sb.AppendFormat("\r\n\t {0} {1} ", type, val.WhereClauseSqlServer(col, var1, var2));

			if( val.From != null ) p.Add(GetParameter(var1, val.From));
			if( val.To != null ) p.Add(GetParameter(var2, val.To));
		}

		public void AppendRangeClause(StringBuilder sb, List<DbParameter> p,
			string type, string col, string var1, string var2, object valFrom, object valTo)
		{
			if( valFrom != null && valTo == null )
				AppendGTClause(sb, p, type, col, var1, valFrom, true);
			else if( valFrom == null && valTo != null )
				AppendLTClause(sb, p, type, col, var2, valTo, true);
			else
			{

				sb.AppendFormat("\r\n\t {0} {1} BETWEEN {2} AND {3} ", type, col, var1, var2);
				p.Add(GetParameter(var1, valFrom));
				p.Add(GetParameter(var2, valTo));
			}
		}
		#endregion		// #region Where clause builder
	}

	#region Connection Manager

	public class ClsConMgr
	{
		#region Fields

		private static ClsConMgr _mgr;
		private Dictionary<string, ClsConnection> Connections;

		#endregion		// #region Fields

		#region Properties

		/// <summary>Gets the connection manager</summary>
		public static ClsConMgr Manager
		{
			get
			{
				if( _mgr == null ) _mgr = new ClsConMgr();
				return _mgr;
			}
		}

		/// <summary>Gets the connection from the Manager that has the specified
		/// key name(C# indexer)</summary>
		/// <param name="index">The key name of the connection</param>
		/// <returns>The connection object if found, null if not</returns>
		public ClsConnection this[string index]
		{
			get
			{
				return ( Connections.ContainsKey(index) == true )
					? Connections[index] : null;
			}
		}

		/// <summary>Gets the number of connections in the Manager</summary>
		public int Count
		{
			get { return Connections.Count; }
		}
		#endregion		// #region Properties

		#region Constructors

		private ClsConMgr()
		{
			Connections = new Dictionary<string, ClsConnection>();
		}
		#endregion		// #region Constructors

		#region Public Methods

		public void AddConnection(ClsConnection cn)
		{
			Connections[cn.DbConnectionKey] = cn;
		}

		public ClsConnection AddNewConnection()
		{
			ClsConnection cn = new ClsConnection();
			cn.DbConnectionKey = ClsEnvironment.ConnectionKey;

			if( Connections.ContainsKey(cn.DbConnectionKey) == true )
			{
				ClsConnection ocn = Connections[cn.DbConnectionKey];
				if( ocn.DbConnectionString == cn.DbConnectionString ) return ocn;
			}

			Connections[cn.DbConnectionKey] = cn;
			return cn;
		}

		public ClsConnection AddConnection(string key, string providerType,
			string connectStr, string user, string pwd, string contract)
		{
			ClsConnection cn =
				new ClsConnection(connectStr, providerType, user, pwd, contract);
			if( Connections.ContainsKey(key) == true )
			{
				ClsConnection ocn = Connections[key];
				if( ocn.DbConnectionString == cn.DbConnectionString ) return ocn;
			}

			Connections.Add(key, cn);
			return cn;
		}

		public void RemoveConnection(string key)
		{
			Connections.Remove(key);
		}

		public bool ContainsKey(string key)
		{
			return Connections.ContainsKey(key);
		}
		#endregion		// #region Public Methods
	}
	#endregion		// #region Connection Manager
}