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
        /// ��¼���ݰ�
        /// </summary>
        /// <param name="buffer">����¼�����ݰ�</param>
        /// <param name="endPoint">IP�˵�</param>
        public override void Write(byte[] buffer, IPEndPoint endPoint)
        {
            DictNode node = new DictNode();
            node.Add("buffer", buffer);
            node.Add("endpoint", endPoint.ToString());
            byte[] sndMsg = BEncode.ByteArrayEncode(node);
            handler.SendNormalPacket(sndMsg);
        }

        /// <summary>
        /// �رռ�¼��
        /// </summary>
        public override void Close()
        {
            handler.Close();
        }
    }
}
