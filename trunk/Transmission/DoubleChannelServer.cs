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
    /// ˫ͨ��������
    /// </summary>
    public class DoubleChannelServer : IPacketMonitor
    {
        #region Fields

        /// <summary>
        /// ���ӵĿͻ���
        /// </summary>
        private IDictionary<string, IPacketHandler> handlers;

        /// <summary>
        /// �����˿�
        /// </summary>
        private int port;

        /// <summary>
        /// �������׽���
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
        /// ���캯��
        /// </summary>
        /// <param name="port">���󶨵Ķ˿�</param>
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
        /// ��������
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
        /// ֹͣ����
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
        /// ���տͻ�������
        /// </summary>
        /// <returns></returns>
        public DoubleChannelClient Accept()
        {
            Socket client = server.Accept();
            DoubleChannelClient result = HandleClient(client);
            return result;
        }

        /// <summary>
        /// ��ʼ�첽��������
        /// </summary>
        /// <param name="callBack">�� BeginAccept ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginAccept(AsyncCallback callBack, object state)
        {
            return acceptDelegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// �����첽�������ӵĽ���
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        /// <returns>�������ӵĿͻ���</returns>
        public DoubleChannelClient EndAccept(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            DoubleChannelClient result = acceptDelegate.EndInvoke(ar);
            return result;
        }

        #endregion

        /// <summary>
        /// ����ͻ���
        /// </summary>
        /// <param name="socket">�ͻ����׽���</param>
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
        /// ��������ݰ�
        /// </summary>
        /// <param name="packet">�����ݰ�</param>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        /// <param name="error">������Ϣ</param>
        /// <param name="port">�˿�</param>
        /// <returns>����򿪳ɹ�,����ture,���򷵻�false</returns>
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
                error = "���ݰ���������Կ���߳�ʼ���������߶˿���Ϣ!";
            }
            catch (BEncodingException bee)
            {
                error = bee.Message;
            }
            return false;
        }

        /// <summary>
        /// �ر����пͻ�������
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

        #region IHandlerMonitor ��Ա

        /// <summary>
        /// ע�����ݰ�������
        /// </summary>
        /// <param name="handler">���ݰ�������</param>
        public void Register(IPacketHandler handler)
        {
            semaphore.WaitOne();
            string key = handler.EndPoint.ToString();
            handlers.Add(key, handler);
            semaphore.Release();
        }

        /// <summary>
        /// ע�����ݰ�������
        /// </summary>
        /// <param name="handler">���ݰ�������</param>
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
