using System;
using System.Data;
using System.Data.Common;
using System.Net.Sockets;

namespace DbProxy.Client
{
    public sealed class DbProxyConnection : DbConnection, ICloneable
    {
        #region Field

        private bool disposed;
        private string connectionString;
        private string dataSource;
        private byte priority;
        private string deviceId;
        private int timeout;
        private ushort port;
        private string database;
        private string serverVersion;
        private ConnectionState state;
        private DbProxyDataReader dataReader;
        private ITdsConnection connection;
        private const ushort defaultPort = 1234;
        private const int defaultTimeout = 10000;

        #endregion

        #region Properties

        public override string ConnectionString
        {
            get
            {
                if (connectionString == null)
                    return string.Empty;
                return connectionString;
            }
            set
            {
                if (state == ConnectionState.Open)
                    throw new InvalidOperationException("Not Allowed to change ConnectionString property while Connection state is OPEN");
                SetConnectionString(value);
            }
        }

        public override string DataSource
        {
            get { return dataSource; }
        }

        public ushort Port
        {
            get { return port; }
        }

        public int Timeout
        {
            get { return timeout;}
        }

        public override string Database
        {
            get { return database; }
        }

        public override string ServerVersion
        {
            get { return serverVersion; }
        }

        public override ConnectionState State
        {
            get { return state; }
        }

        internal DbProxyDataReader DataReader
        {
            get { return dataReader; }
            set { dataReader = value; }
        }

        #endregion

        #region Constructors

        public DbProxyConnection()
        {
            port = defaultPort;
            timeout = defaultTimeout;
            state = ConnectionState.Closed;
        }

        public DbProxyConnection(string connectionString)
        {
            port = defaultPort;
            timeout = defaultTimeout;
            ConnectionString = connectionString.ToUpper();
            state = ConnectionState.Closed;
        }

        #endregion

        #region Methods

        private void SetConnectionString(string connString)
        {
            string[] items = connString.ToUpper().Split(';');
            foreach (string s in items)
            {
                string[] str = s.Split('=');

                switch (str[0])
                {
                    case "DATA SOURCE":
                        dataSource = str[1];
                        break;
                    case "PORT":
                        port = ushort.Parse(str[1]);
                        break;
                    case "DEVICE ID":
                        deviceId = str[1];
                        break;
                    case "PRIORITY":
                        priority = byte.Parse(str[1]);
                        break;
                    case "TIMEOUT":
                        timeout = int.Parse(str[1]);
                        break;
                }
            }
            connectionString = connString;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void ChangeDatabase(string databaseName)
        {
            database = databaseName;
        }

        public override void Close()
        {
            if (connection != null)
            {
                connection.Close();
            }

            if (dataReader != null)
            {
                dataReader = null;
            }

            ChangeState(ConnectionState.Closed);
        }

        protected override DbCommand CreateDbCommand()
        {
            DbCommand result = new DbProxyCommand();
            result.Connection = this;
            return result;
        }

        public override void Open()
        {
            if (state == ConnectionState.Open)
                throw new InvalidOperationException("The Connection is already Open (State=Open)");

            if (connectionString == null || connectionString.Trim().Length == 0)
                throw new InvalidOperationException("Connection string has not been initialized.");

            connection = TdsConnectionPool.GetConnection(dataSource, port, deviceId, priority, timeout);

            try
            {
                connection.Open();
            }
            catch (SocketException se)
            {
                throw new DbProxyException(se.Message, se);
            }

            disposed = false; // reset this, so using () would call Close ().
            ChangeState(ConnectionState.Open);
        }

        private void ChangeState(ConnectionState currentState)
        {
            ConnectionState originalState = state;
            state = currentState;
            OnStateChange(CreateStateChangeEvent(originalState, currentState));
        }

        private StateChangeEventArgs CreateStateChangeEvent(ConnectionState originalState, ConnectionState currentState)
        {
            return new StateChangeEventArgs(originalState, currentState);
        }

        public override DataTable GetSchema()
        {
            DataTable table = new DataTable();
            //table.Load();
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            try
            {
                if (disposing)
                {
                    if (State == ConnectionState.Open)
                        Close();
                    ConnectionString = string.Empty;
                }
            }
            finally
            {
                disposed = true;
                base.Dispose(disposing);
            }
        }

        internal void SendPacket(byte[] bytes)
        {
            connection.SendPacket(bytes);
        }

        internal byte[] ReceivePacket()
        {
            try
            {
                return connection.ReceivePacket();
            }
            catch (SocketException se)
            {
                Close();
                throw new DbProxyException(se.Message, se);
            }
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            return new DbProxyConnection(ConnectionString);
        }

        #endregion
    }
}
