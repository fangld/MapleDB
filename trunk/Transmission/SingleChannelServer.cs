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
    /// 单通道服务器
    /// </summary>
    public class SingleChannelServer : IPacketMonitor
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

        private delegate SingleChannelClient AccepctClient();
        private AccepctClient acceptDelegate;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">待绑定的端口</param>
        public SingleChannelServer(int port)
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
        public SingleChannelClient Accept()
        {
            Socket client = server.Accept();
            SingleChannelClient result = HandleClient(client);
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
        public SingleChannelClient EndAccept(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            SingleChannelClient result = acceptDelegate.EndInvoke(ar);
            return result;
        }

        #endregion

        /// <summary>
        /// 处理客户端
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        private SingleChannelClient HandleClient(Socket socket)
        {
            IPacketHandler channelHandler = new PacketHandler(socket);

            try
            {
                byte[] rcvMsg = channelHandler.ReceiveNormalPacket();
                byte[] key, iv;
                string error;
                bool openSuccess = CheckOpenPacket(rcvMsg, out key, out iv, out error);
                if (openSuccess)
                {
                    channelHandler.SetKeyIV(key, iv);
                    byte[] ackMessage = BuildAckMessage(key, iv);
                    channelHandler.SendSealedPacket(ackMessage);
                    if (!isStop)
                    {
                        Register(channelHandler);
                        return new SingleChannelClient(channelHandler, this);
                    }
                    CloseHandler(channelHandler);
                    return null;
                }
                throw new TransmissionException(error);
            }

            catch (SocketException se)
            {
                CloseHandler(channelHandler);
                throw new TransmissionException(se.Message, se);
            }

            catch (IOException ioe)
            {
                CloseHandler(channelHandler);
                throw new TransmissionException(ioe.Message, ioe);
            }

            catch (PacketException pe)
            {
                CloseHandler(channelHandler);
                throw new TransmissionException(pe.Message, pe);
            }

            catch (BEncodingException bee)
            {
                CloseHandler(channelHandler);
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
        /// <returns>如果打开成功,返回ture,否则返回false</returns>
        private bool CheckOpenPacket(byte[] packet, out byte[] key, out byte[] iv, out string error)
        {
            key = null;
            iv = null;
            port = -1;
            error = string.Empty;
            try
            {
                DictNode node = (DictNode)BEncode.Decode(packet);
                if (node.ContainsKey("key") && node.ContainsKey("iv"))
                {
                    key = ((BytesNode)node["key"]).ByteArray;
                    iv = ((BytesNode)node["iv"]).ByteArray;
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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        private byte[] BuildAckMessage(byte[] key, byte[] iv)
        {
            DictNode node = new DictNode();
            node.Add("key", key);
            node.Add("iv", iv);
            return BEncode.ByteArrayEncode(node);
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
