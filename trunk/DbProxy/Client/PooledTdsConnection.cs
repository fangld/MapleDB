using System;

namespace DbProxy.Client
{
    /// <summary>
    /// 底层连接的缓冲类
    /// </summary>
    internal class PooledTdsConnection : ITdsConnection
    {
        #region Fields

        /// <summary>
        /// 底层连接类
        /// </summary>
        private ITdsConnection connection;

        /// <summary>
        /// 连接是否已经打开
        /// </summary>
        private bool isOpen;

        #endregion

        #region Properties

        /// <summary>
        /// 获取DB Proxy Server地址
        /// </summary>
        public string Url
        {
            get { return connection.Url; }
        }

        /// <summary>
        /// 获取DB Proxy Server端口
        /// </summary>
        public ushort Port
        {
            get { return connection.Port; }
        }

        /// <summary>
        /// 获取机构号
        /// </summary>
        public string DeviceId
        {
            get { return connection.DeviceId; }
        }

        /// <summary>
        /// 获取优先级
        /// </summary>
        public byte Priority
        {
            get { return connection.Priority; }
        }

        /// <summary>
        /// 获取和设置连接是否已经打开
        /// </summary>
        public bool IsOpen
        {
            get { return isOpen; }
        }

        /// <summary>
        /// 获取超时时间
        /// </summary>
        public int Timeout
        {
            get { return connection.Timeout; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">底层连接类</param>
        internal PooledTdsConnection(ITdsConnection connection)
        {
            this.connection = connection;
            isOpen = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 打开连接
        /// </summary>
        public void Open()
        {
            if (!connection.IsOpen)
            {
                connection.Open();
            }
            isOpen = true;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (isOpen)
            {
                isOpen = false;
                TdsConnectionPool.PutConnection(connection);
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet">数据包</param>
        public void SendPacket(byte[] packet)
        {
            connection.SendPacket(packet);
        }

        /// <summary>
        /// 接收数据包
        /// </summary>
        /// <returns>返回的数据包</returns>
        public byte[] ReceivePacket()
        {
            return connection.ReceivePacket();
        }

        #endregion

        #region ITdsConnection 成员

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
