using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BEncoding;
using Packets;

namespace DbProxy.Server
{
    /// <summary>
    /// Db Proxy服务器端
    /// </summary>
    public class DbProxyServer : IPacketMonitor
    {
        #region Fields

        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        private Semaphore stopped;

        private Semaphore semaphore;

        private const int connectionNum = 150;

        private bool isStop;

        private Thread thread;

        private Socket server;

        /// <summary>
        /// 数据连接字符串
        /// </summary>
        private string connectionString;

        /// <summary>
        /// 超时时间
        /// </summary>
        private int timeout;

        private IDictionary<string, IPacketHandler> clients;

        #endregion

        #region Properties

        /// <summary>
        /// 获取和设置数据连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据连接字符串</param>
        /// <param name="timeout">超时时间</param>
        public DbProxyServer(string connectionString, int timeout)
        {
            this.connectionString = connectionString;
            this.timeout = timeout;
            ThreadPool.SetMaxThreads(100, 200);
            isStop = false;
            stopped = new Semaphore(0, 1);
            semaphore = new Semaphore(1,1);
            clients = new Dictionary<string, IPacketHandler>(connectionNum);
        }

        #endregion

        #region Methods

        private int port;

        /// <summary>
        /// 启动函数
        /// </summary>
        public void Start()
        {
            port = 1234;
            thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Name = "DbProxyServer";
            thread.Start();
        }

        public void Listen()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EndPoint serverPoint = new IPEndPoint(IPAddress.Any, port);
                server.SendBufferSize = 16777216;
                server.ReceiveBufferSize = 16777216;
                server.Bind(serverPoint);
                server.Listen(connectionNum);
                do
                {
                    if (isStop)
                    {
                        Console.WriteLine("Listen stop!");
                        Close();
                        stopped.Release();
                        return;
                    }
                    tcpClientConnected.Reset();
                    server.BeginAccept(DoAcceptTcpClientCallback, server);
                    tcpClientConnected.WaitOne();
                } while (true);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 停止函数
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stop start!");
            isStop = true;
            tcpClientConnected.Set();
            stopped.WaitOne();
        }

        public void Close()
        {
            semaphore.WaitOne();
            foreach (KeyValuePair<string, IPacketHandler> pair in clients)
            {
                pair.Value.Close();
            }
            clients.Clear();
            server.Close();
            semaphore.Release();
        }

        /// <summary>
        /// 异步处理Accept函数
        /// </summary>
        /// <param name="ar">异步操作</param>
        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            Socket server = (Socket) ar.AsyncState;

            try
            {
                Socket client = server.EndAccept(ar);

                tcpClientConnected.Set();
                if (isStop)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    return;
                }

                if (client.Connected)
                {
                    HandleClient(client);
                }
                else
                {
                    Console.WriteLine("Client connect error!");
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// 处理客户端
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        private void HandleClient(Socket socket)
        {
            IPacketHandler handler = new PacketHandler(socket);
            socket.ReceiveTimeout = timeout;

            try
            {
                byte[] rcvMsg = handler.ReceiveNormalPacket();

                string error;
                string ipAddress = ((IPEndPoint) socket.RemoteEndPoint).Address.ToString();
                bool openSuccess = HandleOpenPacket(rcvMsg, ipAddress, out error);

                if (openSuccess)
                {
                    TdsClient client = new TdsClient(handler, this, connectionString);
                    byte[] sndMsg = BuildSuccessOpenPacket(handler.Key, handler.IV);
                    handler.SendNormalPacket(sndMsg);
                    ThreadPool.QueueUserWorkItem(client.Exchange);
                    socket.ReceiveTimeout = 0;
                }
                else
                {
                    byte[] sndMsg = BuildFailOpenPacket(error);
                    handler.SendNormalPacket(sndMsg);
                    handler.Close();
                }
            }

            catch (SocketException se)
            {
                Console.WriteLine("SocketException:{0}", se.Message);
                handler.Close();
            }

            catch (IOException ioe)
            {
                Console.WriteLine("IOException:{0}", ioe.Message);
                handler.Close();
            }

            catch (PacketException pe)
            {
                Console.WriteLine("PacketException:{0}", pe.Message);
                handler.Close();
            }

            catch (BEncodingException bee)
            {
                Console.WriteLine("BEncoding Exception:{0}", bee.Message);
                handler.Close();
            }
        }

        /// <summary>
        /// 建立成功打开数据包
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>返回数据包</returns>
        private byte[] BuildSuccessOpenPacket(byte[] key, byte[] iv)
        {
            DictNode result = new DictNode();
            result.Add("code", 1);
            result.Add("key", key);
            result.Add("iv", iv);
            return BEncode.ByteArrayEncode(result);
        }

        /// <summary>
        /// 建立失败打开数据包
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <returns>返回数据包</returns>
        private byte[] BuildFailOpenPacket(string message)
        {
            DictNode result = new DictNode();
            result.Add("code", 6);
            result.Add("message", message);
            return BEncode.ByteArrayEncode(result);
        }

        /// <summary>
        /// 处理打开数据包
        /// </summary>
        /// <param name="packet">打开数据包</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="error">错误信息</param>
        /// <returns>返回是否成功打开连接</returns>
        private bool HandleOpenPacket(byte[] packet, string ipAddress, out string error)
        {
            string deviceId = string.Empty;
            DictNode openNode = (DictNode)BEncode.Decode(packet);

            if (openNode.ContainsKey("deviceid"))
            {
                deviceId = ((BytesNode)openNode["deviceid"]).StringText;
            }

            else
            {
                error = "不包含机构号";
                return false;
            }

            if (openNode.ContainsKey("priority"))
            {
                int priority = (byte)((IntNode)openNode["priority"]).Value;
                if (priority > 4 || priority < 0)
                {
                    error = "优先级错误";
                    return false;
                }
            }
            else
            {
                error = "不包含优先级";
                return false;
            }

            bool result = Validate(deviceId, ipAddress, out error);
            return result;
        }

        /// <summary>
        /// 验证连接的有效性
        /// </summary>
        /// <param name="deviceId">设备号</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="error">错误信息</param>
        /// <returns>验证成功，返回true；否则，返回false。</returns>
        private bool Validate(string deviceId, string ipAddress, out string error)
        {
            error = string.Empty;
            return true;
        }

        #endregion

        #region IPacketMonitor 成员

        public void Register(IPacketHandler handler)
        {
            semaphore.WaitOne();
            clients.Add(handler.EndPoint.ToString(), handler);
            semaphore.Release();
        }

        public void Unregister(IPacketHandler handler)
        {
            semaphore.WaitOne();
            clients.Remove(handler.EndPoint.ToString());
            semaphore.Release();
        }

        #endregion
    }
}
