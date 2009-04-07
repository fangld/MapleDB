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
    /// ˫ͨ���ͻ���
    /// </summary>
    public class DoubleChannelClient
    {
        #region Fields

        /// <summary>
        /// �ͻ��˶˿�
        /// </summary>
        private const int clientPort = 9200;

        /// <summary>
        /// ���ݰ������������
        /// </summary>
        private IPacketMonitor monitor;

        /// <summary>
        /// ͨ�ų�ʼ������
        /// </summary>
        private byte[] iv;

        /// <summary>
        /// ͨ����Կ
        /// </summary>
        private byte[] key;

        /// <summary>
        /// �Ƿ��Ѿ���
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// ͨ��1�����ݰ�������
        /// </summary>
        private IPacketHandler channelHandler1;

        /// <summary>
        /// ͨ��2�����ݰ�������
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
        /// ���캯��
        /// </summary>
        /// <param name="channelHandler1">ͨ��1�����ݰ�������</param>
        /// <param name="channelHandler2">ͨ��2�����ݰ�������</param>
        /// <param name="monitor">���ݰ������������</param>
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
        /// ���캯��
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
        /// ���Ӻ���
        /// </summary>
        /// <param name="host">��������ַ</param>
        /// <param name="port">�������˿�</param>
        public void Connect(string host, int port)
        {
            if (isOpen)
            {
                throw new TransmissionException("�Ѿ������˷�����!");
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
        /// ��ʼ�첽����
        /// </summary>
        /// <param name="host">��������ַ</param>
        /// <param name="port">�������˿�</param>
        /// <param name="callBack">�� BeginConnect ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callBack, object state)
        {
            return connectDelegate.BeginInvoke(host, port, callBack, state);
        }

        /// <summary>
        /// �����첽�������ӵĽ���
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public void EndConnect(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            connectDelegate.EndInvoke(ar);
        }

        #endregion

        #region Send Functions

        /// <summary>
        /// ��ͨ��1��������
        /// </summary>
        /// <param name="bytes">����������</param>
        public void SendByChannel1(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler1.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("��û�����ӷ�����!");
            }
        }

        /// <summary>
        /// ��ʼ�첽��ͨ��1��������
        /// </summary>
        /// <param name="bytes">����������</param>
        /// <param name="callBack">�� BeginSendByChannel1 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginSendByChannel1(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendByChannel1Delegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// �����첽��ͨ��1��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public void EndSendByChannel1(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendByChannel1Delegate.EndInvoke(ar);
        }

        /// <summary>
        /// ��ͨ��2��������
        /// </summary>
        /// <param name="bytes">����������</param>
        public void SendByChannel2(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler2.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("��û�����ӷ�����!");
            }
        }

        /// <summary>
        /// ��ʼ�첽��ͨ��2��������
        /// </summary>
        /// <param name="bytes">����������</param>
        /// <param name="callBack">�� BeginSendByChannel2 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginSendByChannel2(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendByChannel2Delegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// �����첽��ͨ��2��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public void EndSendByChannel2(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendByChannel2Delegate.EndInvoke(ar);
        }

        #endregion

        #region Receive Functions

        /// <summary>
        /// ��ͨ��1��������
        /// </summary>
        /// <returns>���ؽ��յ�����</returns>
        public byte[] ReceiveByChannel1()
        {
            if (isOpen)
            {
                return channelHandler1.ReceiveSealedPacket();
            }
            throw new TransmissionException("��û�����ӷ�����!");
        }

        /// <summary>
        /// ��ʼ�첽��ͨ��1��������
        /// </summary>
        /// <param name="callBack">�� BeginSendByChannel1 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginReceiveByChannel1(AsyncCallback callBack, object state)
        {
            return receiveByChannel1Delegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// �����첽��ͨ��1��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public byte[] EndReceiveByChannel1(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            return receiveByChannel1Delegate.EndInvoke(ar);
        }

        /// <summary>
        /// ��ͨ��2��������
        /// </summary>
        /// <returns>���ؽ��յ�����</returns>
        public byte[] ReceiveByChannel2()
        {
            if (isOpen)
            {
                return channelHandler2.ReceiveSealedPacket();
            }
            throw new TransmissionException("��û�����ӷ�����!");
        }

        /// <summary>
        /// ��ʼ�첽��ͨ��2��������
        /// </summary>
        /// <param name="callBack">�� BeginSendByChannel2 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginReceiveByChannel2(AsyncCallback callBack, object state)
        {
            return receiveByChannel2Delegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// �����첽��ͨ��1��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public byte[] EndReceiveByChannel2(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            return receiveByChannel2Delegate.EndInvoke(ar);
        }

        #endregion

        /// <summary>
        /// �������Ӱ�
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
