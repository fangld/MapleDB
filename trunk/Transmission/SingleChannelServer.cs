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
    /// ��ͨ��������
    /// </summary>
    public class SingleChannelServer : IPacketMonitor
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

        private delegate SingleChannelClient AccepctClient();
        private AccepctClient acceptDelegate;

        #endregion

        #region Constructors

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="port">���󶨵Ķ˿�</param>
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
        public SingleChannelClient Accept()
        {
            Socket client = server.Accept();
            SingleChannelClient result = HandleClient(client);
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
        public SingleChannelClient EndAccept(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            SingleChannelClient result = acceptDelegate.EndInvoke(ar);
            return result;
        }

        #endregion

        /// <summary>
        /// ����ͻ���
        /// </summary>
        /// <param name="socket">�ͻ����׽���</param>
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
        /// ��������ݰ�
        /// </summary>
        /// <param name="packet">�����ݰ�</param>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        /// <param name="error">������Ϣ</param>
        /// <returns>����򿪳ɹ�,����ture,���򷵻�false</returns>
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
                error = "���ݰ���������Կ���߳�ʼ���������߶˿���Ϣ!";
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
