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
    public class SingleChannelClient
    {
        #region Fields

        /// <summary>
        /// 数据包处理器监测器
        /// </summary>
        private IPacketMonitor monitor; 

        /// <summary>
        /// 服务器地址
        /// </summary>
        private string host;

        /// <summary>
        /// 服务器端口
        /// </summary>
        private int port;

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
        /// 通道的数据包处理器
        /// </summary>
        private IPacketHandler channelHandler;

        private SendDelegate sendDelegate;
        private ReceiveDelegate receiveDelegate;
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
        /// <param name="channelHandler">通道的数据包处理器</param>
        /// <param name="monitor">数据包处理器监测器</param>
        internal SingleChannelClient(IPacketHandler channelHandler, IPacketMonitor monitor)
        {
            this.channelHandler = channelHandler;
            isOpen = true;
            key = channelHandler.Key;
            iv = channelHandler.IV;
            this.monitor = monitor;
            connectDelegate = Connect;
            sendDelegate = Send;
            receiveDelegate = Receive;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SingleChannelClient()
        {
            isOpen = false;
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
            this.host = host;
            this.port = port;
            if (isOpen)
            {
                throw new TransmissionException("已经连接了服务器!");
            }
            try
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(host, port);
                channelHandler = new PacketHandler(client);
                key = channelHandler.Key;
                iv = channelHandler.IV;

                byte[] handshakeMsg = HandshakeHandler.BuildHandshake(key, iv);
                channelHandler.SendNormalPacket(handshakeMsg);
                byte[] ackBytes = channelHandler.ReceiveSealedPacket();
                if (HandshakeHandler.VerifyHandshake(ackBytes, key, iv))
                {
                    isOpen = true;
                }
                else
                {
                    channelHandler.Close();
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
        /// 发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        public void Send(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("还没有连接服务器!");
            }
        }

        /// <summary>
        /// 开始异步发送数据
        /// </summary>
        /// <param name="bytes">待发的数据</param>
        /// <param name="callBack">在 BeginSendByChannel1 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginSend(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendDelegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// 处理异步发送数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public void EndSend(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendDelegate.EndInvoke(ar);
        }

        #endregion

        #region Receive Functions

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns>返回接收的数据</returns>
        public byte[] Receive()
        {
            if (isOpen)
            {
                return channelHandler.ReceiveSealedPacket();
            }
            throw new TransmissionException("还没有连接服务器!");
        }

        /// <summary>
        /// 开始异步接收数据
        /// </summary>
        /// <param name="callBack">在 BeginSendByChannel1 完成时执行的 AsyncCallback 委托。</param>
        /// <param name="state">包含用户定义的任何附加数据的对象。 </param>
        /// <returns>表示异步调用的 IAsyncResult。 </returns>
        public IAsyncResult BeginReceive(AsyncCallback callBack, object state)
        {
            return receiveDelegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// 处理异步接收数据
        /// </summary>
        /// <param name="ar">一个表示异步调用的 IAsyncResult。</param>
        public byte[] EndReceive(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            return receiveDelegate.EndInvoke(ar);
        }
        
        #endregion

        public void Close()
        {
            if (channelHandler != null)
            {
                if (monitor != null)
                {
                    monitor.Unregister(channelHandler);
                }
                channelHandler.Close();
            }
        }

        #endregion
    }
}