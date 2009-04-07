using System;
using System.Net.Sockets;
using System.Threading;
using BEncoding;
using Packets;

namespace DbProxy.Client
{
    /// <summary>
    /// 底层连接类
    /// </summary>
    internal class TdsConnection : ITdsConnection
    {
        #region Fields

        /// <summary>
        /// DB Proxy Server地址
        /// </summary>
        private string url;

        /// <summary>
        /// DB Proxy Server端口
        /// </summary>
        private ushort port;

        /// <summary>
        /// 机构名
        /// </summary>
        private string deviceId;

        /// <summary>
        /// 优先级
        /// </summary>
        private byte priority;

        /// <summary>
        /// 连接是否已经打开
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// 超时时间
        /// </summary>
        private int timeout;

        /// <summary>
        /// 连接套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 传输包处理器
        /// </summary>
        private IPacketHandler handler;

        /// <summary>
        /// 关闭数据包
        /// </summary>
        private static byte[] closePacket;

        #endregion

        #region Properties

        /// <summary>
        /// 获取DB Proxy Server地址
        /// </summary>
        public string Url
        {
            get { return url; }
        }

        /// <summary>
        /// 获取DB Proxy Server端口
        /// </summary>
        public ushort Port
        {
            get { return port; }
        }

        /// <summary>
        /// 获取机构号
        /// </summary>
        public string DeviceId
        {
            get { return deviceId; }
        }

        /// <summary>
        /// 获取优先级
        /// </summary>
        public byte Priority
        {
            get { return priority; }
        }

        /// <summary>
        /// 设置和获取连接是否已经打开
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
            get { return timeout; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static TdsConnection()
        {
            closePacket = new byte[13];
            closePacket[0] = (byte)'[';
            closePacket[1] = (byte)'6';
            closePacket[2] = (byte)':';
            closePacket[3] = (byte)'a';
            closePacket[4] = (byte)'c';
            closePacket[5] = (byte)'t';
            closePacket[6] = (byte)'i';
            closePacket[7] = (byte)'o';
            closePacket[8] = (byte)'n';
            closePacket[9] = (byte)'i';
            closePacket[10] = (byte)'2';
            closePacket[11] = (byte)'e';
            closePacket[12] = (byte)']';
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">DB Proxy Server地址</param>
        /// <param name="port">DB Proxy Server端口</param>
        /// <param name="deviceId">设备号</param>
        /// <param name="priority">优先级</param>
        /// <param name="timeout">超时时间</param>
        internal TdsConnection(string url, ushort port, string deviceId, byte priority, int timeout)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SendBufferSize = 262144;
            socket.ReceiveBufferSize = 262144;
            this.url = url;
            this.port = port;
            this.deviceId = deviceId;
            this.priority = priority;
            this.timeout = timeout;
            isOpen = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 检验打开数据包
        /// </summary>
        /// <param name="packet">打开数据包</param>
        /// <returns>如果打开成功,返回ture,否则返回false</returns>
        private bool CheckOpenPacket(byte[] packet)
        {
            DictNode openPacket = (DictNode)BEncode.Decode(packet);
            int code = ((IntNode)openPacket["code"]).Value;

            switch (code)
            {
                case 1:
                    byte[] key = ((BytesNode)openPacket["key"]).ByteArray;
                    byte[] iv = ((BytesNode)openPacket["iv"]).ByteArray;
                    handler.SetKeyIV(key, iv);
                    return true;
                case 6:
                    string messag = ((BytesNode)openPacket["message"]).StringText;
                    throw new DbProxyException(messag);
                default:
                    throw new DbProxyException("错误的消息代码");
            }
        }

        /// <summary>
        /// 建立打开DB Proxy Server包
        /// </summary>
        /// <returns>返回数据包</returns>
        private byte[] BuildOpenPacket()
        {
            DictNode rootNode = new DictNode();
            rootNode.Add("action", 1);
            rootNode.Add("deviceid", deviceId);
            rootNode.Add("priority", priority);
            return BEncode.ByteArrayEncode(rootNode);
        }

        /// <summary>
        /// 建立关闭DB Proxy Server包
        /// </summary>
        /// <returns>返回数据包</returns>
        private static byte[] BuildClosePacket()
        {
            return closePacket;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 打开连接
        /// </summary>
        public void Open()
        {
            if (isOpen)
            {
                throw new DbProxyException("连接已经打开!");
            }
            socket.Connect(url, port);
            socket.ReceiveTimeout = timeout;
            handler = new PacketHandler(socket);
            byte[] sndMsg = BuildOpenPacket();
            handler.SendNormalPacket(sndMsg);
            byte[] rcvMsg = handler.ReceiveNormalPacket();
            isOpen = CheckOpenPacket(rcvMsg);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet">数据包</param>
        public void SendPacket(byte[] packet)
        {
            handler.SendSealedPacket(packet);
        }

        /// <summary>
        /// 接收数据包
        /// </summary>
        /// <returns>返回的数据包</returns>
        public byte[] ReceivePacket()
        {
            try
            {
                return handler.ReceiveSealedPacket();
            }
            catch (SocketException se)
            {
                isOpen = false;
                throw new SocketException(se.ErrorCode);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (isOpen)
            {
                byte[] closeMsg = BuildClosePacket();
                handler.SendSealedPacket(closeMsg);
                handler.ReceiveSealedPacket();
                handler.Close();
                isOpen = false;
            }
            else
            {
                handler.Close();
            }
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回哈希值</returns>
        public override int GetHashCode()
        {
            int result = 0;

            result += priority*13;

            result += port*37;

            result += timeout*73;

            for (int i = 0; i < url.Length; i++)
            {
                result += url[i]*103;
            }

            for (int i = 0; i < deviceId.Length; i++)
            {
                result += deviceId[i]*137;
            }

            return result;
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
