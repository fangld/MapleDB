using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BEncoding;
using Packets;
using Tools;

namespace Transmission
{
    /// <summary>
    /// 双通道客户端
    /// </summary>
    public class DoubleChannelClient
    {
        #region Fields

        /// <summary>
        /// 客户端端口
        /// </summary>
        private const int clientPort = 9200;

        /// <summary>
        /// 数据包处理器监测器
        /// </summary>
        private IPacketMonitor monitor;

        /// <summary>
        /// 通信初始化向量
        /// </summary>
        private byte[] iv;

        /// <summary>
        /// 通信密钥
        /// </summary>
        private byte[] key;

        /// <summary>
        /// 是否已经打开
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// 通道1的数据包处理器
        /// </summary>
        private IPacketHandler channelHandler1;

        /// <summary>
        /// 通道2的数据包处理器
        /// </summary>
        private IPacketHandler channelHandler2;

        private SendDelegate sendByChannel1Delegate;
        private SendDelegate sendByChannel2Delegate;
        private ReceiveDelegate receiveByChannel1Delegate;
        private ReceiveDelegate receiveByChannel2Delegate;
        private ConnectDelegate connectDelegate;

        #endregion

        #region Delegates

        private delegate void SendDelegate(byte[] bytes);
        private delegate byte[] ReceiveDelegate();
        private delegate void ConnectDelegate(string host, int port);

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channelHandler1">通道1的数据包处理器</param>
        /// <param name="channelHandler2">通道2的数据包处理器</param>
        /// <param name="monitor">数据包处理器监测器</param>
        internal DoubleChannelClient(IPacketHandler channelHandler1, IPacketHandler channelHandler2, IPacketMonitor monitor)
        {
            this.channelHandler1 = channelHandler1;
            this.channelHandler2 = channelHandler2;
            isOpen = true;
            key = channelHandler1.Key;
            iv = channelHandler1.IV;
            this.monitor = monitor;
            connectDelegate = Connect;
            sendByChannel1Delegate = SendByChannel1;
            sendByChannel2Delegate = SendByChannel2;
            receiveByChannel1Delegate = ReceiveByChannel1;
            receiveByChannel2Delegate = ReceiveByChannel2;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DoubleChannelClient()
        {
            isOpen = false;
            connectDelegate = Connect;
            sendByChannel1Delegate = SendByChannel1;
            sendByChannel2Delegate = SendByChannel2;
            receiveByChannel1Delegate = ReceiveByChannel1;
            receiveByChannel2Delegate = ReceiveByChannel2;
        }

        #endregion

        #region Methods

        #region Connect Functions

        /// <summary>
        /// 连接函数
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口</param>
        public void Connect(string host, int port)
        {
            if (isOpen)
            {
                throw new TransmissionException("已经连接了服务器!");
            }
            try
            {
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, clientPort);
                server.Bind(serverEndPoint);
                server.Listen(1);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(host, port);
                channelHandler1 = new PacketHandler(client);
                key = channelHandler1.Key;
                iv = channelHandler1.IV;

                byte[] openBytes = BuildHandshakeMessage();
                channelHandler1.SendNormalPacket(openBytes);

                Socket socket = server.Accept();
                channelHandler2 = new PacketHandler(socket);
                channelHandler2.SetKeyIV(key, iv);
                byte[] ackMsg = channelHandler2.ReceiveSealedPacket();
                if (HandshakeHandler.VerifyHandshake(ackMsg, key, iv))
                {
                    isOpen = true;
                    channelHandler1.SendSealedPacket(ackMsg);
                }
                else
                {
                    channelHandler1.Close();
                    channelHandler2.Close();
                }
            }
            catch (SocketException se)
            {
                throw new TransmissionException(se.Message, se);
            }
        }

        /// <summary>
        /// 开始异步连接
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口</param>
        /// <param name="callBack">在 BeginConnect 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callBack, object state)
        {
            return connectDelegate.BeginInvoke(host, port, callBack, state);
        }

        /// <summary>
        /// 处理异步接收连接的结束
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public void EndConnect(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            connectDelegate.EndInvoke(ar);
        }

        #endregion

        #region Send Functions

        /// <summary>
        /// 从通道1发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        public void SendByChannel1(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler1.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("还没有连接服务器!");
            }
        }

        /// <summary>
        /// 开始异步从通道1发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        /// <param name="callBack">在 BeginSendByChannel1 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginSendByChannel1(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendByChannel1Delegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// 处理异步从通道1发送数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public void EndSendByChannel1(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendByChannel1Delegate.EndInvoke(ar);
        }

        /// <summary>
        /// 从通道2发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        public void SendByChannel2(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler2.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("还没有连接服务器!");
            }
        }

        /// <summary>
        /// 开始异步从通道2发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        /// <param name="callBack">在 BeginSendByChannel2 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginSendByChannel2(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendByChannel2Delegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// 处理异步从通道2发送数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public void EndSendByChannel2(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendByChannel2Delegate.EndInvoke(ar);
        }

        #endregion

        #region Receive Functions

        /// <summary>
        /// 从通道1接收数据
        /// </summary>
        /// <returns>返回接收的数据</returns>
        public byte[] ReceiveByChannel1()
        {
            if (isOpen)
            {
                return channelHandler1.ReceiveSealedPacket();
            }
            throw new TransmissionException("还没有连接服务器!");
        }

        /// <summary>
        /// 开始异步从通道1接收数据
        /// </summary>
        /// <param name="callBack">在 BeginSendByChannel1 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginReceiveByChannel1(AsyncCallback callBack, object state)
        {
            return receiveByChannel1Delegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// 处理异步从通道1接收数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public byte[] EndReceiveByChannel1(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            return receiveByChannel1Delegate.EndInvoke(ar);
        }

        /// <summary>
        /// 从通道2接收数据
        /// </summary>
        /// <returns>返回接收的数据</returns>
        public byte[] ReceiveByChannel2()
        {
            if (isOpen)
            {
                return channelHandler2.ReceiveSealedPacket();
            }
            throw new TransmissionException("还没有连接服务器!");
        }

        /// <summary>
        /// 开始异步从通道2接收数据
        /// </summary>
        /// <param name="callBack">在 BeginSendByChannel2 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginReceiveByChannel2(AsyncCallback callBack, object state)
        {
            return receiveByChannel2Delegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// 处理异步从通道1接收数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public byte[] EndReceiveByChannel2(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            return receiveByChannel2Delegate.EndInvoke(ar);
        }

        #endregion

        /// <summary>
        /// 建立连接包
        /// </summary>
        /// <returns></returns>
        private byte[] BuildHandshakeMessage()
        {
            DictNode node = new DictNode();
            node.Add("port", clientPort);
            node.Add("key", key);
            node.Add("iv", iv);
            return BEncode.ByteArrayEncode(node);
        }

        public void Close()
        {
            if (channelHandler1 != null)
            {
                if (monitor != null)
                {
                    monitor.Unregister(channelHandler1);
                }
                channelHandler1.Close();
            }

            if (channelHandler2 != null)
            {
                if (monitor != null)
                {
                    monitor.Unregister(channelHandler1);
                }
                channelHandler2.Close();
            }
        }

        #endregion
    }
}
