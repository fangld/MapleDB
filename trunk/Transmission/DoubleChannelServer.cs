using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BEncoding;
using Packets;

namespace Transmission
{
    /// <summary>
    /// 双通道服务器
    /// </summary>
    public class DoubleChannelServer : IPacketMonitor
    {
        #region Fields

        /// <summary>
        /// 连接的客户端
        /// </summary>
        private IDictionary<string, IPacketHandler> handlers;

        /// <summary>
        /// 监听端口
        /// </summary>
        private int port;

        /// <summary>
        /// 监听的套接字
        /// </summary>
        private Socket server;

        private bool isStop;

        private Semaphore semaphore;

        #endregion

        #region Delegates

        private delegate DoubleChannelClient AccepctClient();
        private AccepctClient acceptDelegate;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">待绑定的端口</param>
        public DoubleChannelServer(int port)
        {
            this.port = port;
            handlers = new Dictionary<string, IPacketHandler>();
            acceptDelegate = Accept;
            semaphore = new Semaphore(1, 1);
            isStop = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 启动监听
        /// </summary>
        public void Start(int backlog)
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, port);
                server.Bind(serverEndPoint);
                server.Listen(backlog);
                isStop = false;
            }
            catch (SocketException se)
            {
                throw new TransmissionException(se.Message, se);
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            isStop = true;
            semaphore.WaitOne();
            foreach (IPacketHandler handler in handlers.Values)
            {
                if (handler != null)
                {
                    handler.Close();
                }
            }
            handlers.Clear();
            semaphore.Release();
        }

        #region Accept Functions

        /// <summary>
        /// 接收客户端连接
        /// </summary>
        /// <returns></returns>
        public DoubleChannelClient Accept()
        {
            Socket client = server.Accept();
            DoubleChannelClient result = HandleClient(client);
            return result;
        }

        /// <summary>
        /// 开始异步接收连接
        /// </summary>
        /// <param name="callBack">在 BeginAccept 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginAccept(AsyncCallback callBack, object state)
        {
            return acceptDelegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// 处理异步接收连接的结束
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        /// <returns>返回连接的客户端</returns>
        public DoubleChannelClient EndAccept(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            DoubleChannelClient result = acceptDelegate.EndInvoke(ar);
            return result;
        }

        #endregion

        /// <summary>
        /// 处理客户端
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        private DoubleChannelClient HandleClient(Socket socket)
        {
            IPacketHandler channelHandler1 = new PacketHandler(socket);

            IPacketHandler channelHandler2 = null;

            try
            {
                byte[] openMsg = channelHandler1.ReceiveNormalPacket();
                byte[] key, iv;
                int clientPort;
                string error;
                bool openSuccess = CheckOpenPacket(openMsg, out key, out iv, out clientPort, out error);
                if (openSuccess)
                {
                    channelHandler1.SetKeyIV(key, iv);

                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress clientIPAddress = ((IPEndPoint) socket.RemoteEndPoint).Address;
                    client.Connect(clientIPAddress, clientPort);
                    channelHandler2 = new PacketHandler(client);
                    channelHandler2.SetKeyIV(key, iv);
                    byte[] handshakeMsg = HandshakeHandler.BuildHandshake(key, iv);
                    channelHandler2.SendSealedPacket(handshakeMsg);
                    byte[] rcvMsg = channelHandler1.ReceiveSealedPacket();
                    if (HandshakeHandler.VerifyHandshake(rcvMsg, key, iv) && !isStop)
                    {
                        Register(channelHandler1);
                        Register(channelHandler2);
                        return new DoubleChannelClient(channelHandler1, channelHandler2, this);
                    }
                    CloseHandler(channelHandler1);
                    CloseHandler(channelHandler2);
                    return null;
                }
                throw new TransmissionException(error);
            }

            catch (SocketException se)
            {
                CloseHandler(channelHandler1);
                CloseHandler(channelHandler2);
                throw new TransmissionException(se.Message, se);
            }

            catch (IOException ioe)
            {
                CloseHandler(channelHandler1);
                CloseHandler(channelHandler2);
                throw new TransmissionException(ioe.Message, ioe);
            }

            catch (PacketException pe)
            {
                CloseHandler(channelHandler1);
                CloseHandler(channelHandler2);
                throw new TransmissionException(pe.Message, pe);
            }

            catch (BEncodingException bee)
            {
                CloseHandler(channelHandler1);
                CloseHandler(channelHandler2);
                throw new TransmissionException(bee.Message, bee);
            }
        }

        /// <summary>
        /// 检验打开数据包
        /// </summary>
        /// <param name="packet">打开数据包</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="error">错误信息</param>
        /// <param name="port">端口</param>
        /// <returns>如果打开成功,返回ture,否则返回false</returns>
        private bool CheckOpenPacket(byte[] packet, out byte[] key, out byte[] iv, out int port, out string error)
        {
            key = null;
            iv = null;
            port = -1;
            error = string.Empty;
            try
            {
                DictNode node = (DictNode) BEncode.Decode(packet);
                if (node.ContainsKey("key") && node.ContainsKey("iv") && node.ContainsKey("port"))
                {
                    key = ((BytesNode) node["key"]).ByteArray;
                    iv = ((BytesNode) node["iv"]).ByteArray;
                    port = ((IntNode) node["port"]).Value;
                    return true;
                }
                error = "数据包不包含密钥或者初始化向量或者端口信息!";
            }
            catch (BEncodingException bee)
            {
                error = bee.Message;
            }
            return false;
        }

        /// <summary>
        /// 关闭所有客户端连接
        /// </summary>
        /// <param name="handler"></param>
        private void CloseHandler(IPacketHandler handler)
        {
            if (handler != null)
            {
                handler.Close();
            }
        }

        #endregion

        #region IHandlerMonitor 成员

        /// <summary>
        /// 注册数据包处理器
        /// </summary>
        /// <param name="handler">数据包处理器</param>
        public void Register(IPacketHandler handler)
        {
            semaphore.WaitOne();
            string key = handler.EndPoint.ToString();
            handlers.Add(key, handler);
            semaphore.Release();
        }

        /// <summary>
        /// 注销数据包处理器
        /// </summary>
        /// <param name="handler">数据包处理器</param>
        public void Unregister(IPacketHandler handler)
        {
            semaphore.WaitOne();
            string key = handler.EndPoint.ToString();
            handlers.Remove(key);
            semaphore.Release();
        }

        #endregion
    }
}
