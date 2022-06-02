using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

using System.IO;

namespace FileSystemMonitor.Base
{
    using static FileSystemMonitor.Base.LogHelper;
    #region SQLite
    public class SQLiteUtil : IDisposable
    {
        #region Variable
        private bool disposedValue;
        public SQLiteConnection ConSQLite { get; set; }
        public string DBName { get; set; }
        #endregion

        #region Constructor
        public SQLiteUtil()
        {
            this.InitSQLiteDB(string.Empty);
        }

        public SQLiteUtil(string DName)
        {
            this.InitSQLiteDB(DName);
        }
        #endregion

        #region InitSQLiteDB
        public void InitSQLiteDB(string DBName)
        {
            string tempDName = string.IsNullOrEmpty(DBName) ? "LocalDB" : DBName;
            string BasePath = AppDomain.CurrentDomain.BaseDirectory + @"\" + tempDName + ".db";
            try
            {
                if (File.Exists(BasePath))
                    return;
                else
                    using (var f = File.Create(BasePath)) { f.Close(); }
                using (SQLiteConnection scn = new SQLiteConnection("Data Source=" + BasePath))
                {
                    scn.Open();
                    scn.Close();
                }
                LogTrace("Iiitialize SQLite DataBase: [" + tempDName + "], Path: [" + BasePath + "]");
            }
            catch (Exception ex)
            {
                LogTrace("Iiitialize SQLite DataBase Fail. " + ex.Message);
            }
            finally
            {
                this.DBName = BasePath;
            }
        }
        #endregion

        #region Open
        public SQLiteConnection OpenConnection(string DBName)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = new SQLiteConnection()
                {
                    ConnectionString = "Data Source=" + DBName + ";New=False;Compress=True;"
                };
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                connection.Open();
                LogTrace("Open Connection Pass. ");
            }
            catch (Exception ex)
            {
                LogTrace("Open Connection Fail. " + ex.Message);
            }
            return connection;
        }
        #endregion

        #region Close
        public void CloseConnection()
        {
            try
            {
                if (this.ConSQLite.State == ConnectionState.Open)
                {
                    this.ConSQLite.Close();
                    LogTrace("Close Connection Pass. ");
                }
            }
            catch (Exception ex)
            {
                LogTrace("Close Connection Fail. " + ex.Message);
            }
        }
        #endregion

        #region Query
        public DataTable Query(string SqlCMD)
        {
            DataTable dt = new DataTable();
            try
            {
                this.ConSQLite = this.OpenConnection(this.DBName);
                using (var cnSqlite = new SQLiteConnection(this.ConSQLite))
                {
                    using (SQLiteCommand cmd = cnSqlite.CreateCommand())
                    {
                        cmd.Connection = cnSqlite;
                        cmd.CommandText = SqlCMD;
                        cmd.CommandTimeout = 600;
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter())
                        {
                            dt.Clear();
                            adapter.AcceptChangesDuringFill = false;
                            adapter.Fill(dt);
                            adapter.Dispose();
                        }
                    }
                    cnSqlite.Close();
                }
                LogTrace("Query Pass: {" + SqlCMD + "}.");
            }
            catch (Exception ex)
            {
                LogTrace("Query Fail: {" + SqlCMD + "}. [" + ex.Message + "]");
            }
            finally
            {
                this.CloseConnection();
            }
            return dt;
        }
        #endregion

        #region NonQuery
        public void NonQuery(string SqlCMD)
        {
            SQLiteTransaction tran = null;
            try
            {
                this.ConSQLite = this.OpenConnection(this.DBName);
                using (var cnSqlite = new SQLiteConnection(this.ConSQLite))
                {
                    using (tran = cnSqlite.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = cnSqlite.CreateCommand())
                        {
                            cmd.Connection = cnSqlite;
                            cmd.CommandText = SqlCMD;
                            cmd.CommandTimeout = 600;
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    cnSqlite.Close();
                }
                LogTrace("NonQuery Pass: {" + SqlCMD + "}.");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                LogTrace("NonQuery Fail: {" + SqlCMD + "}. [" + ex.Message + "]");
            }
            finally
            {
                this.CloseConnection();
            }
        }
        #endregion

        #region LogTrace     
        public void LogTrace(string Message)
        {
            LogHelper.LogTrace("SQLite", Message);
        }
        #endregion

        #region Disposed
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                    this.CloseConnection();
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
                this.ConSQLite = null;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~SQLiteUtil()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    #endregion

    #region MsSQL
    public class MsSQLUtil : IDisposable
    {
        #region Variable
        private bool disposedValue;
        public SqlConnection ConMSSQL { get; set; }
        public string ConnectionString { get; set; }
        public string ServerName { get; set; }
        public string DBName { get; set; }
        public string UserID { get; set; }
        public string UserPWD { get; set; }
        public string MinPool { get; set; }
        public string MaxPool { get; set; }
        public string TimeOut { get; set; }
        #endregion

        #region Constructor
        public MsSQLUtil(string ServerName, string DBName, string UserID, string UserPWD)
        {
            this.ServerName = ServerName;
            this.DBName = DBName;
            this.UserID = UserID;
            this.UserPWD = UserPWD;
            this.MinPool = "1";
            this.MaxPool = "5";
            this.TimeOut = "20";
            this.InitMsSQDB(this.ServerName, this.DBName, this.UserID, this.UserPWD, this.MinPool, this.MaxPool, this.TimeOut);
        }
        public MsSQLUtil(string ServerName, string DBName, string UserID, string UserPWD, string MinPool, string MaxPool, string TimeOut)
        {
            this.ServerName = ServerName;
            this.DBName = DBName;
            this.UserID = UserID;
            this.UserPWD = UserPWD;
            this.MinPool = MinPool;
            this.MaxPool = MaxPool;
            this.TimeOut = TimeOut;
            this.InitMsSQDB(this.ServerName, this.DBName, this.UserID, this.UserPWD, this.MinPool, this.MaxPool, this.TimeOut);
        }
        #endregion

        #region InitMsSQDB
        public void InitMsSQDB(string ServerName, string DBName, string UserID, string UserPWD, string MinPool, string MaxPool, string TimeOut)
        {
            string tmpConnectionString = string.Empty;
            try
            {
                tmpConnectionString = "Data Source=" + ServerName + ";Initial Catalog=" + DBName + ";Integrated Security=False;User ID=" + UserID + ";Password=" + UserPWD + ";Min Pool Size=" + MinPool + ";Max Pool Size=" + MaxPool + ";Connect Timeout=" + TimeOut + ";";
                using (var connection = new SqlConnection(tmpConnectionString))
                {
                    connection.Open();
                    connection.Close();
                }
                LogTrace("Iiitialize MSSQL DataBase Pass. ");
            }
            catch (Exception ex)
            {
                LogTrace("Iiitialize MSSQL DataBase Fail. " + ex.Message);
            }
            finally
            {
                this.ConnectionString = tmpConnectionString;
            }
        }
        #endregion

        #region Open
        public SqlConnection OpenConnection()
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection()
                {
                    ConnectionString = this.ConnectionString
                };
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                connection.Open();
                LogTrace("Open Connection Pass. ");
            }
            catch (Exception ex)
            {
                LogTrace("Open Connection Fail. " + ex.Message);
            }
            return connection;
        }
        #endregion

        #region Close
        public void CloseConnection()
        {
            try
            {
                if (this.ConMSSQL.State == ConnectionState.Open)
                {
                    this.ConMSSQL.Close();
                    LogTrace("Close Connection Pass. ");
                }
            }
            catch (Exception ex)
            {
                LogTrace("Close Connection Fail. " + ex.Message);
            }
        }
        #endregion

        #region Query
        public DataTable Query(string SqlCMD)
        {
            DataTable dt = new DataTable();
            try
            {
                this.ConMSSQL = this.OpenConnection();
                using (var cnMssql = this.ConMSSQL)
                {
                    using (SqlCommand cmd = cnMssql.CreateCommand())
                    {
                        cmd.Connection = cnMssql;
                        cmd.CommandText = SqlCMD;
                        cmd.CommandTimeout = 600;
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            dt.Clear();
                            adapter.AcceptChangesDuringFill = false;
                            adapter.Fill(dt);
                            adapter.Dispose();
                        }
                    }
                    cnMssql.Close();
                }
                LogTrace("Query Pass: {" + SqlCMD + "}.");
            }
            catch (Exception ex)
            {
                LogTrace("Query Fail: {" + SqlCMD + "}. [" + ex.Message + "]");
            }
            finally
            {
                this.CloseConnection();
            }
            return dt;
        }
        #endregion

        #region NonQuery
        public void NonQuery(string SqlCMD)
        {
            SqlTransaction tran = null;
            try
            {
                this.ConMSSQL = this.OpenConnection();
                using (var cnMssql = this.ConMSSQL)
                {
                    using (tran = cnMssql.BeginTransaction())
                    {
                        using (SqlCommand cmd = cnMssql.CreateCommand())
                        {
                            cmd.Connection = cnMssql;
                            cmd.CommandText = SqlCMD;
                            cmd.CommandTimeout = 600;
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    cnMssql.Close();
                }
                LogTrace("Query Pass: {" + SqlCMD + "}.");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                LogTrace("Query Fail: {" + SqlCMD + "}. [" + ex.Message + "]");
            }
            finally
            {
                this.CloseConnection();
            }
        }
        #endregion

        #region LogTrace
        public void LogTrace(string Message)
        {
            LogHelper.LogTrace("MSSQL", Message);
        }
        #endregion

        #region Disposed
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                    this.CloseConnection();
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
                this.ConMSSQL = null;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~MsSQLUtil()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    #endregion
}
