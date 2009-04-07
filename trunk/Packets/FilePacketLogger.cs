using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 数据包流记录器
    /// </summary>
    public class FilePacketLogger : PacketLogger
    {
        #region Fields

        private Stream stream;

        #endregion

        #region Constructors

        public FilePacketLogger(string path)
        {
            stream = File.Open(path, FileMode.OpenOrCreate);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 记录数据包
        /// </summary>
        /// <param name="buffer">待记录的数据包</param>
        /// <param name="endPoint">IP端点</param>
        public override void Write(byte[] buffer, IPEndPoint endPoint)
        {
            byte[] writeBytes = PacketFormmater.Format(buffer, endPoint);
            stream.Write(writeBytes, 0, writeBytes.Length);
        }

        /// <summary>
        /// 关闭记录器
        /// </summary>
        public override void Close()
        {
            stream.Flush();
            stream.Close();
        }

        #endregion
    }
}
