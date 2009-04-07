using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 数据包记录器
    /// </summary>
    public abstract class PacketLogger
    {
        #region Methods

        /// <summary>
        /// 记录包数据
        /// </summary>
        /// <param name="buffer">记录的数据</param>
        /// <param name="endPoint">IP端点</param>
        public abstract void Write(byte[] buffer, IPEndPoint endPoint);

        /// <summary>
        /// 关闭记录器
        /// </summary>
        public abstract void Close();

        #endregion
    }
}
