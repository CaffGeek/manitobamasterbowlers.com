//#define DB2
namespace ManitobaMasterBowlers_com
{
	internal sealed partial class Persistence : System.IDisposable {
		private System.Collections.Generic.Dictionary<string, System.Data.IDbConnection> mdConnectionStrings;

		private bool mbDeadlockDetection;
		private int miDeadlockRetriesMaximum;
		private int miDeadlockDelayBetweenRetries;

		#region GetSqlServerNames
		/// <summary>
		/// Retrieves unique SQL Server names.
		/// </summary>
		/// <returns>A list of unique SQL Server names.</returns>
		/// <exception cref="PersistenceException">Thrown when database connection information not valid.</exception>
		internal static string[] GetSqlServerNames() {
			System.Collections.Generic.LinkedList<string> llNames = new System.Collections.Generic.LinkedList<string>();
			try {
				foreach (
				 System.Configuration.ConnectionStringSettings cssCurrent in
				  System.Web.Configuration.WebConfigurationManager.ConnectionStrings
				)
					if (cssCurrent.ProviderName == string.Empty || cssCurrent.ProviderName == "System.Data.SqlClient") {
						System.Data.SqlClient.SqlConnectionStringBuilder csbCurrent;
						try {
							csbCurrent = new System.Data.SqlClient.SqlConnectionStringBuilder(cssCurrent.ConnectionString);
						}
						catch {
							csbCurrent = null;
						}
						if (! (csbCurrent == null))
							if (! (csbCurrent.DataSource == string.Empty)) {
								bool bDuplicate = false;
								foreach (string sCurrent in llNames)
									if (sCurrent.ToLower() == csbCurrent.DataSource.ToLower()) {
										bDuplicate = true;
										break;
									}
								if (! bDuplicate)
									llNames.AddLast(csbCurrent.DataSource);
							}
					}
			}
			catch (System.Configuration.ConfigurationErrorsException eCurrent) {
				throw new PersistenceException(
				 "The /configuration/connectionStrings section of Web.config is not valid.",
				 eCurrent
				);
			}

			string[] sNames = new string[llNames.Count];
			llNames.CopyTo(sNames, 0);

			return sNames;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		internal Persistence() {
			// Initialize the connection strings.
			mdConnectionStrings =
			 new System.Collections.Generic.Dictionary<string, System.Data.IDbConnection>(
			  1,
			  System.StringComparer.InvariantCultureIgnoreCase
			 );

			// Retrieve the deadlock detection value from Web.config at
			// /configuration/Configuration/DeadlockDetection/text() or default to true when unavailable or unparsable.
			string sDeadlockDetection =
			 System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection(
			  "Configuration/DeadlockDetection"
			 ) as string;
			if (! bool.TryParse(sDeadlockDetection, out mbDeadlockDetection))
				mbDeadlockDetection = true;

			// Retrieve the maximum deadlock retries value from Web.config at
			// /configuration/Configuration/DeadlockRetriesMaximum/text() or
			// default to 2 when unavailable, unparsable or out of the range of 1 to 10 (inclusive).
			string sDeadlockRetriesMaximum =
			 System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection(
			  "Configuration/DeadlockRetriesMaximum"
			 ) as string;
			if (
			 ! int.TryParse(sDeadlockRetriesMaximum, out miDeadlockRetriesMaximum) ||
			 miDeadlockRetriesMaximum < 1 || miDeadlockRetriesMaximum > 10
			)
				miDeadlockRetriesMaximum = 2;

			// Retrieve the deadlock delay between retries value from Web.config at
			// /configuration/Configuration/DeadlockDelayBetweenAttempts/text() or
			// default to 5000 (milliseconds) when unavailable, unparsable or out of the range of 1 to 60 (inclusive).
			string sDeadlockDelayBetweenRetries =
			 System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection(
			  "Configuration/DeadlockDelayBetweenRetries"
			 ) as string;
			if (
			 ! int.TryParse(sDeadlockDelayBetweenRetries, out miDeadlockDelayBetweenRetries) ||
			 miDeadlockDelayBetweenRetries < 1 || miDeadlockDelayBetweenRetries > 60
			)
				miDeadlockDelayBetweenRetries = 5;
			miDeadlockDelayBetweenRetries *= 1000;
		}
		#endregion

		#region GetConnection
		/// <summary>
		/// Opens a connection using the provider and connection string specified in Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"] and
		/// caches the open connection.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <returns>
		/// The previously cached open connection (when available) or
		/// an open connection using the provider and connection string specified in Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// Thrown when <paramref name="connectionStringName"/> is null, an empty string or unavailable in
		/// Web.config as /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// database connection information for <paramref name="connectionStringName"/> is invalid or
		/// specifies an unsupported provider.
		/// </exception>
		private System.Data.IDbConnection GetConnection(string connectionStringName) {
			if (connectionStringName == null)
				throw new System.ArgumentNullException("connectionStringName", "A connection string name is required.");

			if (connectionStringName == string.Empty)
				throw new System.ArgumentException("A connection string name is required.", "connectionStringName");

			System.Configuration.ConnectionStringSettings cssCurrent;
			try {
				cssCurrent =
				 System.Web.Configuration.WebConfigurationManager.ConnectionStrings[connectionStringName];
			}
			catch (System.Configuration.ConfigurationErrorsException eCurrent) {
				throw new PersistenceException(
				 "The /configuration/connectionStrings section of Web.config is not valid.",
				 eCurrent
				);
			}

			if (cssCurrent == null)
				throw new System.ArgumentException(
				 "Connection string name '" + connectionStringName + "' is not available.",
				 "connectionStringName"
				);

			System.Data.IDbConnection cCached;
			switch (cssCurrent.ProviderName) {
				case "":
					goto case "System.Data.SqlClient";
				case "System.Data.SqlClient":
					mdConnectionStrings.TryGetValue(connectionStringName, out cCached);
					if (! (cCached == null))
						return cCached;
					else {
						System.Data.SqlClient.SqlConnectionStringBuilder csbSelected;
						try {
							csbSelected = new System.Data.SqlClient.SqlConnectionStringBuilder(cssCurrent.ConnectionString);
						}
						catch (System.ArgumentException eCurrent) {
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}
						catch (System.FormatException eCurrent) {
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}
						catch (System.Collections.Generic.KeyNotFoundException eCurrent) {
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}
						csbSelected.ApplicationName = GetApplicationName();
						csbSelected.WorkstationID = GetWorkstationId();

						System.Data.SqlClient.SqlConnection cSelected = new System.Data.SqlClient.SqlConnection();
						try {
							cSelected.ConnectionString = csbSelected.ConnectionString;
						}
						catch (System.ArgumentException eCurrent) {
							cSelected.Dispose();
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}

						try {
							cSelected.Open();
						}
						catch (System.InvalidOperationException eCurrent) {
							cSelected.Dispose();
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}
						catch (System.Data.SqlClient.SqlException eCurrent) {
							/*
							eCurrent.Number	Description
							53						Server invalid
							4060 and 4064		Database invalid.
							18452 and 18456	Credentials invalid.
							*/
							cSelected.Dispose();
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid.",
							 eCurrent
							);
						}

						mdConnectionStrings.Add(connectionStringName, cSelected);

						return cSelected;
					}
				case "IBM.Data.DB2.iSeries":
#if DB2
					mdConnectionStrings.TryGetValue(connectionStringName, out cCached);
					if (! (cCached == null))
						return cCached;
					else {
						IBM.Data.DB2.iSeries.iDB2Connection cSelected = new IBM.Data.DB2.iSeries.iDB2Connection();
						try {
							cSelected.ConnectionString = cssCurrent.ConnectionString;
						}
						catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
							cSelected.Dispose();
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid. " +
							  eCurrent.MessageDetails,
							 eCurrent
							);
						}

						try {
							cSelected.Open();
						}
						catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
							cSelected.Dispose();
							throw new PersistenceException(
							 "Database connection information for connection string name " +
							  "'" + connectionStringName + "' " +
							  "is invalid. " +
							  eCurrent.MessageDetails,
							 eCurrent
							);
						}

						mdConnectionStrings.Add(connectionStringName, cSelected);

						return cSelected;
					}
#else
					throw new PersistenceException(
					 "Database connection information for connection string name '" + connectionStringName + "' " +
					  "specifies an unsupported provider '" + cssCurrent.ProviderName + "'."
					);
#endif
				default:
					throw new PersistenceException(
					 "Database connection information for connection string name '" + connectionStringName + "' " +
					  "specifies an unsupported provider '" + cssCurrent.ProviderName + "'."
					);
			}

		}
		#endregion

		#region GetApplicationName
		/// <summary>
		/// Retrieves the name of the Web site and virtual directory of this service.
		/// </summary>
		/// <returns>
		/// The name of the Web site and virtual directory of this service as follows: WebSite/VirtualDirectory;
		/// if the Web site is "Default Web Site" then only the name of the virtual directory.
		/// </returns>
		private string GetApplicationName() {
			return
			 (
			  System.Web.Hosting.HostingEnvironment.SiteName == "Default Web Site" ?
			   string.Empty :
			   System.Web.Hosting.HostingEnvironment.SiteName + "/"
			 ) +
			 System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath.TrimStart(new char[] {'/'});
		}
		#endregion

		#region GetWorkstationId
		/// <summary>
		/// Retrieves the NetBIOS name of this computer.
		/// </summary>
		/// <returns>The NetBIOS name of this computer or an empty string.</returns>
		private string GetWorkstationId() {
			try {
				return System.Environment.MachineName;
			}
			catch (System.InvalidOperationException) {
				return string.Empty;
			}
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes a stored procedure or query.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						dcCommand.ExecuteNonQuery();

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a stored procedure or query.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="recordsAffected">The number of records affected by <paramref name="command"/>.</param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out int recordsAffected) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						recordsAffected = dcCommand.ExecuteNonQuery();

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a stored procedure or query.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="value">
		/// The value of the first column of the first row of the first resultset returned by <paramref name="command"/>.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out object value) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						value = dcCommand.ExecuteScalar();

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a stored procedure or query.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="value">
		/// The string concatenation of the first column of the first row of
		/// all the resultsets returned by <paramref name="command"/>.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out string value) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						System.Text.StringBuilder sbValue = new System.Text.StringBuilder();
						using (System.Data.IDataReader drDataReader = dcCommand.ExecuteReader()) {
							do {
								while (drDataReader.Read()) {
									sbValue.Append(drDataReader.GetValue(0));
								}
							}
							while (drDataReader.NextResult());
						}
						
						value = sbValue.ToString();

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a stored procedure or query.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="resultsets">The resultsets returned by <paramref name="command"/>.</param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out System.Data.DataSet resultsets) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						resultsets = new System.Data.DataSet();
						if (dcCommand is System.Data.SqlClient.SqlCommand)
							using (
							 System.Data.SqlClient.SqlDataAdapter daDataAdapter =
							  new System.Data.SqlClient.SqlDataAdapter((System.Data.SqlClient.SqlCommand)dcCommand)
							) {
								daDataAdapter.Fill(resultsets);
							}
#if DB2
						if (dcCommand is IBM.Data.DB2.iSeries.iDB2Command)
							using (
							 IBM.Data.DB2.iSeries.iDB2DataAdapter daDataAdapter =
							  new IBM.Data.DB2.iSeries.iDB2DataAdapter((IBM.Data.DB2.iSeries.iDB2Command)dcCommand)
							) {
								daDataAdapter.Fill(resultsets);
							}
#endif

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a stored procedure or query which returns a resultset.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="resultset">The resultset returned by <paramref name="command"/>.</param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out System.Data.DataTable resultset) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						resultset = new System.Data.DataTable();
						if (dcCommand is System.Data.SqlClient.SqlCommand)
							using (
							 System.Data.SqlClient.SqlDataAdapter daDataAdapter =
							  new System.Data.SqlClient.SqlDataAdapter((System.Data.SqlClient.SqlCommand)dcCommand)
							) {
								daDataAdapter.Fill(resultset);
							}
#if DB2
						if (dcCommand is IBM.Data.DB2.iSeries.iDB2Command)
							using (
							 IBM.Data.DB2.iSeries.iDB2DataAdapter daDataAdapter =
							  new IBM.Data.DB2.iSeries.iDB2DataAdapter((IBM.Data.DB2.iSeries.iDB2Command)dcCommand)
							) {
								daDataAdapter.Fill(resultset);
							}
#endif

						return;
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}

		/// <summary>
		/// Executes a SQL Server stored procedure or query which returns an XML Document.
		/// </summary>
		/// <param name="connectionStringName">
		/// The name of a connection string which must be available from Web.config at
		/// /configuration/connectionStrings/add[@name="<paramref name="connectionStringName"/>"].
		/// </param>
		/// <param name="command">The name of a stored procedure or a query.</param>
		/// <param name="parameters">A collection of parameters processed by <paramref name="command"/>.</param>
		/// <param name="document">The XML document returned by <paramref name="command"/>.</param>
		/// <exception cref="System.ObjectDisposedException">Thrown when this instance has been disposed.</exception>
		/// <exception cref="PersistenceException">
		/// Thrown when the /configuration/connectionStrings section of Web.config is not valid,
		/// <paramref name="connectionStringName"/> is null, not available or
		/// specifies an invalid connection string or unsupported provider, or
		/// <paramref name="command"/> is null, invalid, returns an XML document which is not well-formed,
		/// is chosen as the deadlock victim or fails.
		/// </exception>
		internal void Execute(string connectionStringName,
		 string command,
		 ParameterCollection parameters,
		 out System.Xml.XmlDocument document) {

			// Verify not disposed.
			if (mdConnectionStrings == null)
				throw new System.ObjectDisposedException(GetType().FullName);

			// Open and cache a connection.
			System.Data.IDbConnection dcConnection;
			try {
				dcConnection = GetConnection(connectionStringName);
			}
			catch (System.ArgumentException eCurrent) {
				throw new PersistenceException(eCurrent);
			}

			// Validate connection string name.
			if (! (dcConnection is System.Data.SqlClient.SqlConnection))
				throw new PersistenceException(
				 new System.ArgumentException(
				  "The provider associated with connection string name '" + connectionStringName + "' " +
				   "is not supported at this time for this Execute overload.",
				  "connectionStringName"
				 )
				);

			// Validate command.
			if (command == null)
				throw new PersistenceException(
				 new System.ArgumentNullException("command", "A command is required.")
				);

			// Establish and execute the database command with deadlock detection and recovery.
			using (System.Data.IDbCommand dcCommand = dcConnection.CreateCommand()) {
				dcCommand.CommandText = command;
				dcCommand.CommandType =
				 (! command.Trim().Contains(" ") ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text);
				
				if (! (parameters == null))
					foreach (Parameter oParameterCurrent in parameters) {
						System.Data.IDataParameter dpParameter = dcCommand.CreateParameter();
						dpParameter.ParameterName = oParameterCurrent.Name;
						dpParameter.DbType = oParameterCurrent.Type;
						dpParameter.Direction = oParameterCurrent.Direction;
						dpParameter.Value = oParameterCurrent.Value;
						dcCommand.Parameters.Add(dpParameter);
						if (dpParameter is System.Data.SqlClient.SqlParameter) {
							System.Data.SqlClient.SqlParameter pCurrent =
							 (System.Data.SqlClient.SqlParameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#if DB2
						if (dpParameter is IBM.Data.DB2.iSeries.iDB2Parameter) {
							IBM.Data.DB2.iSeries.iDB2Parameter pCurrent =
							 (IBM.Data.DB2.iSeries.iDB2Parameter)dcCommand.Parameters[dcCommand.Parameters.Count - 1];
							pCurrent.Size = oParameterCurrent.Size;
							pCurrent.Precision = oParameterCurrent.Precision;
							pCurrent.Scale = oParameterCurrent.Scale;
							if (oParameterCurrent.Value == null || oParameterCurrent.Value == System.DBNull.Value)
								pCurrent.IsNullable = true;
						}
#endif
						oParameterCurrent.ParameterReference = dpParameter;
					}

				int iDeadlocks = 0;
				do {
					try {
						using (System.Xml.XmlReader rCommand = ((System.Data.SqlClient.SqlCommand)dcCommand).ExecuteXmlReader()) {
							document = new System.Xml.XmlDocument();
							document.Load(rCommand);
							rCommand.Close();

							return;
						}
					}
					catch (System.Data.SqlClient.SqlException eCurrent) {
						switch (eCurrent.Number) {
							case 50000:
								throw new System.ArgumentException(eCurrent.Message, eCurrent);
							case 1205:
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								throw new PersistenceException(
								 "Database command '" + dcCommand.CommandText + "' failed.",
								 eCurrent
								);
						}
					}
					catch (System.InvalidOperationException eCurrent) {
						throw new PersistenceException(
						 "XML document returned by '" + dcCommand.CommandText + "' is not well-formed.",
						 eCurrent
						);
					}
					catch (System.Reflection.TargetInvocationException eCurrent) {
						throw new PersistenceException("Database command '" + dcCommand.CommandText + "' failed.", eCurrent);
					}
#if DB2
					catch (IBM.Data.DB2.iSeries.iDB2Exception eCurrent) {
						switch (eCurrent.SqlState) {
							case "57033":
								if (mbDeadlockDetection && iDeadlocks < miDeadlockRetriesMaximum) {
									System.Web.HttpContext.Current.Trace.Warn(string.Empty, string.Empty, eCurrent);
									dcCommand.Cancel();
									iDeadlocks++;
									System.Threading.Thread.Sleep(miDeadlockDelayBetweenRetries);
									continue;
								}

								goto default;
							default:
								System.Text.RegularExpressions.Regex rSqlState =
								 new System.Text.RegularExpressions.Regex("^[7-9I-Z][0-9A-Z]{4}$");
								if (rSqlState.IsMatch(eCurrent.SqlState))
									throw new System.ArgumentException(eCurrent.Message, eCurrent);
								else
									throw new PersistenceException(
									 "Database command '" + dcCommand.CommandText + "' failed. " +
									  eCurrent.Message,
									 eCurrent
									);
						}
					}
#endif
				} while (true);
			}
		}
		#endregion

		#region Dispose
		/// <summary>
		/// Closes and disposes open cached connections.
		/// </summary>
		public void Dispose() {
			foreach (
			 System.Collections.Generic.KeyValuePair<string, System.Data.IDbConnection> kvpCurrent in mdConnectionStrings
			) {
				kvpCurrent.Value.Close();
				kvpCurrent.Value.Dispose();
			}

			mdConnectionStrings = null;
		}
		#endregion

		/// <summary>
		/// Represents a collection of parameters.
		/// </summary>
		internal sealed class ParameterCollection : System.Collections.Generic.IEnumerable<Parameter> {
			System.Collections.Generic.Dictionary<string, Parameter> mdParameters;

			#region Constructor
			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			internal ParameterCollection() {
				mdParameters = new System.Collections.Generic.Dictionary<string,Parameter>();
			}
			#endregion

			#region Count
			/// <summary>
			/// Gets the number of parameters in the collection of parameters.
			/// </summary>
			internal int Count {
				get {
					return mdParameters.Count;
				}
			}
			#endregion

			#region Item
			/// <summary>
			/// Gets the parameter with the specified name.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or not available.
			/// </exception>
			internal Parameter this[string parameterName] {
				get {
					if (parameterName == null)
						throw new PersistenceException(
						 new System.ArgumentNullException("parameterName", "A parameter name is required.")
						);

					Parameter oParameter;
					if (mdParameters.TryGetValue(parameterName, out oParameter))
						return oParameter;
					else
						throw new PersistenceException(
						 new System.ArgumentException(
						  "Parameter '" + parameterName + "' is not available.",
						  "parameterName"
						 )
						);
				}
			}

			/// <summary>
			/// Gets the parameter at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the parameter.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="index"/> is not available.
			/// </exception>
			internal Parameter this[int index] {
				get {
					if (index < 0 || index > mdParameters.Count - 1)
						throw new PersistenceException(
						 new System.ArgumentException(
						  "Parameter " +
						   "'" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "' " +
						   "is not available.",
						  "index"
						 )
						);

					int iIndex = 0;
					foreach (System.Collections.Generic.KeyValuePair<string, Parameter> oParameterCurrent in mdParameters) {
						if (iIndex == index)
							return oParameterCurrent.Value;
						iIndex++;
					}

					return null;
				}
			}
			#endregion

			#region Add
			/// <summary>
			/// Adds a parameter to the collection of parameters.
			/// </summary>
			/// <param name="parameter">The parameter to a database command.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameter"/> is null or
			/// specifies a name which already exists in the collection of parameters.
			/// </exception>
			public void Add(Parameter parameter) {
				if (parameter == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameter", "A parameter is required.")
					);

				if (mdParameters.ContainsKey(parameter.Name))
					throw new PersistenceException(
					 new System.ArgumentException(
					  "A parameter with the name '" + parameter.Name + "' already exists in this collection and " +
					   "duplicates are not allowed.",
					  "parameter"
					 )
					);

				mdParameters.Add(parameter.Name, parameter);
			}

			/// <summary>
			/// Adds a parameter to the collection of parameters.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or already exists in the collection of parameters.
			/// </exception>
			public void Add(string parameterName) {
				if (parameterName == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameterName", "A parameter name is required.")
					);

				if (mdParameters.ContainsKey(parameterName))
					throw new PersistenceException(
					 new System.ArgumentException(
					  "A parameter with the name '" + parameterName + "' already exists in this collection and " +
					   "duplicates are not allowed.",
					  "parameterName"
					 )
					);

				mdParameters.Add(parameterName, new Parameter(parameterName));
			}

			/// <summary>
			/// Adds a parameter to the collection of parameters.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <param name="parameterType">The data type of the parameter.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or already exists in the collection of parameters.
			/// </exception>
			public void Add(string parameterName,
			 System.Data.DbType parameterType) {

				if (parameterName == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameterName", "A parameter name is required.")
					);

				if (mdParameters.ContainsKey(parameterName))
					throw new PersistenceException(
					 new System.ArgumentException(
					  "A parameter with the name '" + parameterName + "' already exists in this collection and " +
					   "duplicates are not allowed.",
					  "parameterName"
					 )
					);

				mdParameters.Add(parameterName, new Parameter(parameterName, parameterType));
			}

			/// <summary>
			/// Adds a parameter to the collection of parameters.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <param name="parameterType">The data type of the parameter.</param>
			/// <param name="parameterSize">
			/// The maximum size of the parameter value in bytes for binary and string parameters.
			/// </param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or already exists in the collection of parameters.
			/// </exception>
			public void Add(string parameterName,
			 System.Data.DbType parameterType,
			 int parameterSize) {

				if (parameterName == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameterName", "A parameter name is required.")
					);

				if (mdParameters.ContainsKey(parameterName))
					throw new PersistenceException(
					 new System.ArgumentException(
					  "A parameter with the name '" + parameterName + "' already exists in this collection and " +
					   "duplicates are not allowed.",
					  "parameterName"
					 )
					);

				mdParameters.Add(parameterName, new Parameter(parameterName, parameterType, parameterSize));
			}

			/// <summary>
			/// Adds a parameter to the collection of parameters.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <param name="parameterType">The data type of the parameter.</param>
			/// <param name="parameterPrecision">
			/// The maximum number of digits of the parameter value for decimal parameters.
			/// </param>
			/// <param name="parameterScale">
			/// The maximum number of decimal places to which the parameter value is resolved for decimal parameters.
			/// </param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or already exists in the collection of parameters.
			/// </exception>
			public void Add(string parameterName,
			 System.Data.DbType parameterType,
			 byte parameterPrecision,
			 byte parameterScale) {

				if (parameterName == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameterName", "A parameter name is required.")
					);

				if (mdParameters.ContainsKey(parameterName))
					throw new PersistenceException(
					 new System.ArgumentException(
					  "A parameter with the name '" + parameterName + "' already exists in this collection and " +
					   "duplicates are not allowed.",
					  "parameterName"
					 )
					);

				mdParameters.Add(
				 parameterName,
				 new Parameter(parameterName, parameterType, parameterPrecision, parameterScale)
				);
			}
			#endregion

			#region Clear
			/// <summary>
			/// Removes all parameters from the collection of parameters.
			/// </summary>
			internal void Clear() {
				mdParameters.Clear();
			}
			#endregion

			#region Remove
			/// <summary>
			/// Removes the parameter with the specified name.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <exception cref="PersistenceException">
			/// Thrown when <paramref name="parameterName"/> is null or not available.
			/// </exception>
			internal void Remove(string parameterName) {
				if (parameterName == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("parameterName", "A parameter name is required.")
					);

				if (! mdParameters.Remove(parameterName))
					throw new PersistenceException(
					 new System.ArgumentException("Parameter '" + parameterName + "' is not available.", "parameterName")
					);
			}

			/// <summary>
			/// Removes the parameter at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the parameter.</param>
			/// <exception cref="PersistenceException">Thrown when <paramref name="index"/> is not available.</exception>
			internal void Remove(int index) {
				if (index < 0 || index > mdParameters.Count - 1)
					throw new PersistenceException(
					 new System.ArgumentException(
					  "Parameter '" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "' is not available.",
					  "index"
					 )
					);

				int iIndex = 0;
				foreach (System.Collections.Generic.KeyValuePair<string, Parameter> oParameterCurrent in mdParameters) {
					if (iIndex == index) {
						mdParameters.Remove(oParameterCurrent.Value.Name);
						return;
					}
					iIndex++;
				}
			}
			#endregion

			#region TryGetParameter
			/// <summary>
			/// Retrieves the parameter with the specified name, when available.
			/// </summary>
			/// <param name="parameterName">The name of the parameter.</param>
			/// <param name="parameter">
			/// Uninitialized; contains the parameter with the specified name, when available.
			/// </param>
			/// <returns>True when the collection of parameters contains a parameter with the specified name.</returns>
			internal bool TryGetParameter(string parameterName,
			 out Parameter parameter) {

				return mdParameters.TryGetValue(parameterName, out parameter);
			}

			/// <summary>
			/// Retrieves the parameter at the specified index, when available.
			/// </summary>
			/// <param name="index">The zero-based index of the parameter.</param>
			/// <param name="parameter">Uninitialized; contains the parameter at the specified index, when available.</param>
			/// <returns>True when the collection of parameters contains a parameter at the specified index.</returns>
			internal bool TryGetParameter(int index,
			 out Parameter parameter) {

				if (index < 0 || index > mdParameters.Count - 1) {
					parameter = null;
					return false;
				}

				int iIndex = 0;
				foreach (System.Collections.Generic.KeyValuePair<string, Parameter> oParameterCurrent in mdParameters) {
					if (iIndex == index) {
						parameter = oParameterCurrent.Value;
						return true;
					}
					iIndex++;
				}

				parameter = null;
				return false;
			}
			#endregion

			#region GetEnumerator
			/// <summary>
			/// Retrieves parameters from the collection of parameters.
			/// </summary>
			/// <returns>An enumerator.</returns>
			public System.Collections.Generic.IEnumerator<Parameter> GetEnumerator() {
				return mdParameters.Values.GetEnumerator();
			}

			/// <summary>
			/// Retrieves parameters from the collection of parameters.
			/// </summary>
			/// <returns>An enumerator.</returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			   return GetEnumerator();
			}
			#endregion
		}

		/// <summary>
		/// Represents a parameter to a database command.
		/// </summary>
		internal sealed class Parameter {
			private string msName;
			private System.Data.DbType mdtType;
			private int miSize;
			private byte mbPrecision;
			private byte mbScale;
			private System.Data.ParameterDirection mpdDirection;
			private object moValue;
			private object moParameter;

			#region Constructor
			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="name">The name of the parameter.</param>
			/// <exception cref="PersistenceException">Thrown when <paramref name="name"/> is null.</exception>
			internal Parameter(string name) {
				if (name == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("name", "A name is required.")
					);

				msName = name;
				mdtType = System.Data.DbType.String;
			}

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="name">The name of the parameter.</param>
			/// <param name="type">The data type of the parameter.</param>
			/// <exception cref="System.PersistenceException">Thrown when <paramref name="name"/> is null.</exception>
			internal Parameter(string name,
			 System.Data.DbType type) {

				if (name == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("name", "A name is required.")
					);

				msName = name;
				mdtType = type;
			}

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="name">The name of the parameter.</param>
			/// <param name="type">The data type of the parameter.</param>
			/// <param name="size">The maximum size of the parameter value in bytes for binary and string parameters.</param>
			/// <exception cref="System.PersistenceException">Thrown when <paramref name="name"/> is null.</exception>
			internal Parameter(string name,
			 System.Data.DbType type,
			 int size) {

				if (name == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("name", "A name is required.")
					);

				msName = name;
				mdtType = type;
				miSize = size;
			}

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="name">The name of the parameter.</param>
			/// <param name="type">The data type of the parameter.</param>
			/// <param name="precision">The maximum number of digits of the parameter value for decimal parameters.</param>
			/// <param name="scale">
			/// The maximum number of decimal places to which the parameter value is resolved for decimal parameters.
			/// </param>
			/// <exception cref="System.PersistenceException">Thrown when <paramref name="name"/> is null.</exception>
			internal Parameter(string name,
			 System.Data.DbType type,
			 byte precision,
			 byte scale) {

				if (name == null)
					throw new PersistenceException(
					 new System.ArgumentNullException("name", "A name is required.")
					);

				msName = name;
				mdtType = type;
				mbPrecision = precision;
				mbScale = scale;
			}
			#endregion

			#region Name
			/// <summary>
			/// Gets the name of the parameter.
			/// </summary>
			internal string Name {
				get {
					return msName;
				}
			}
			#endregion

			#region Type
			/// <summary>
			/// Gets or sets the data type of the parameter.
			/// </summary>
			internal System.Data.DbType Type {
				get {
					return mdtType;
				}
				set {
					mdtType = value;
				}
			}
			#endregion

			#region Size
			/// <summary>
			/// Gets or sets the maximum size of the parameter value in bytes for binary and string parameters.
			/// </summary>
			internal int Size {
				get {
					return miSize;
				}
				set {
					miSize = value;
				}
			}
			#endregion

			#region Precision
			/// <summary>
			/// Gets or sets the maximum number of digits of the parameter value for decimal parameters.
			/// </summary>
			internal byte Precision {
				get {
					return mbPrecision;
				}
				set {
					mbPrecision = value;
				}
			}
			#endregion

			#region Scale
			/// <summary>
			/// Gets or sets the maximum number of decimal places to which the parameter value is resolved for
			/// decimal parameters.
			/// </summary>
			internal byte Scale {
				get {
					return mbScale;
				}
				set {
					mbScale = value;
				}
			}
			#endregion

			#region Direction
			/// <summary>
			/// Gets or sets whether the parameter is input-only, output-only, bidirectional, or
			/// a stored procedure return value parameter.
			/// </summary>
			internal System.Data.ParameterDirection Direction {
				get {
					return mpdDirection;
				}
				set {
					mpdDirection = value;
				}
			}
			#endregion

			#region Value
			/// <summary>
			/// Gets or sets the value of the parameter.
			/// </summary>
			internal object Value {
				get {
					if (! (moParameter == null)) {
						if (moParameter is System.Data.SqlClient.SqlParameter)
							if (
							 ((System.Data.SqlClient.SqlParameter)moParameter).Direction == System.Data.ParameterDirection.InputOutput ||
							 ((System.Data.SqlClient.SqlParameter)moParameter).Direction == System.Data.ParameterDirection.Output ||
							 ((System.Data.SqlClient.SqlParameter)moParameter).Direction == System.Data.ParameterDirection.ReturnValue
							)
								return ((System.Data.SqlClient.SqlParameter)moParameter).Value;
							else
								return moValue;
#if DB2
						if (moParameter is IBM.Data.DB2.iSeries.iDB2Parameter)
							if (
							 ((IBM.Data.DB2.iSeries.iDB2Parameter)moParameter).Direction == System.Data.ParameterDirection.InputOutput ||
							 ((IBM.Data.DB2.iSeries.iDB2Parameter)moParameter).Direction == System.Data.ParameterDirection.Output ||
							 ((IBM.Data.DB2.iSeries.iDB2Parameter)moParameter).Direction == System.Data.ParameterDirection.ReturnValue
							)
								return ((IBM.Data.DB2.iSeries.iDB2Parameter)moParameter).Value;
							else
								return moValue;
#endif

						return moValue;
					}
					else
						return moValue;
				}
				set {
					moValue = value;
				}
			}
			#endregion

			#region ParameterReference
			/// <summary>
			/// Sets the parameter.
			/// </summary>
			internal object ParameterReference {
				set {
					moParameter = value;
				}
			}
			#endregion
		}

		/// <summary>
		/// The exception that is thrown when any processing problem occurs.
		/// </summary>
		internal sealed class PersistenceException : System.ApplicationException {
			#region Constructor
			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="message">A message that describes the error.</param>
			internal PersistenceException(string message) : base(message) {}

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="exception">The exception that is the cause of this exception.</param>
			internal PersistenceException(System.Exception exception) :
			  base(
			   ! (exception == null) ? exception.Message : null,
			   exception
			  ) {}

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="message">A message that describes the error.</param>
			/// <param name="exception">The exception that is the cause of this exception.</param>
			internal PersistenceException(string message,
			 System.Exception exception) :
			  base(
			   ! string.IsNullOrEmpty(message) ? message : ! (exception == null) ? exception.Message : null,
			   exception
			  ) {}
			#endregion
		}
	}
}