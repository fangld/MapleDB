using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BEncoding;

namespace Packets
{
    public class SocketPacketLogger : PacketLogger
    {
        #region Fields

        private IPacketHandler handler;

        #endregion

        #region Constructors

        public SocketPacketLogger(Socket socket)
        {
            
        }

        #endregion

        /// <summary>
        /// 记录数据包
        /// </summary>
        /// <param name="buffer">待记录的数据包</param>
        /// <param name="endPoint">IP端点</param>
        public override void Write(byte[] buffer, IPEndPoint endPoint)
        {
            DictNode node = new DictNode();
            node.Add("buffer", buffer);
            node.Add("endpoint", endPoint.ToString());
            byte[] sndMsg = BEncode.ByteArrayEncode(node);
            handler.SendNormalPacket(sndMsg);
        }

        /// <summary>
        /// 关闭记录器
        /// </summary>
        public override void Close()
        {
            handler.Close();
        }
    }
}
