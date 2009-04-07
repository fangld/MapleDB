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
        /// ���ݰ������������
        /// </summary>
        private IPacketMonitor monitor; 

        /// <summary>
        /// ��������ַ
        /// </summary>
        private string host;

        /// <summary>
        /// �������˿�
        /// </summary>
        private int port;

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
        /// ͨ�������ݰ�������
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
        /// ���캯��
        /// </summary>
        /// <param name="channelHandler">ͨ�������ݰ�������</param>
        /// <param name="monitor">���ݰ������������</param>
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
        /// ���캯��
        /// </summary>
        public SingleChannelClient()
        {
            isOpen = false;
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
            this.host = host;
            this.port = port;
            if (isOpen)
            {
                throw new TransmissionException("�Ѿ������˷�����!");
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
        /// ��������
        /// </summary>
        /// <param name="bytes">����������</param>
        public void Send(byte[] bytes)
        {
            if (isOpen)
            {
                channelHandler.SendSealedPacket(bytes);
            }
            else
            {
                throw new TransmissionException("��û�����ӷ�����!");
            }
        }

        /// <summary>
        /// ��ʼ�첽��������
        /// </summary>
        /// <param name="bytes">����������</param>
        /// <param name="callBack">�� BeginSendByChannel1 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginSend(byte[] bytes, AsyncCallback callBack, object state)
        {
            return sendDelegate.BeginInvoke(bytes, callBack, state);
        }

        /// <summary>
        /// �����첽��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
        public void EndSend(IAsyncResult ar)
        {
            if (ar == null)
                throw new NullReferenceException("Argument ar can't be null");
            sendDelegate.EndInvoke(ar);
        }

        #endregion

        #region Receive Functions

        /// <summary>
        /// ��������
        /// </summary>
        /// <returns>���ؽ��յ�����</returns>
        public byte[] Receive()
        {
            if (isOpen)
            {
                return channelHandler.ReceiveSealedPacket();
            }
            throw new TransmissionException("��û�����ӷ�����!");
        }

        /// <summary>
        /// ��ʼ�첽��������
        /// </summary>
        /// <param name="callBack">�� BeginSendByChannel1 ���ʱִ�е� AsyncCallback ί�С�</param>
        /// <param name="state">�����û�������κθ������ݵĶ��� </param>
        /// <returns>��ʾ�첽���õ� IAsyncResult�� </returns>
        public IAsyncResult BeginReceive(AsyncCallback callBack, object state)
        {
            return receiveDelegate.BeginInvoke(callBack, state);
        }

        /// <summary>
        /// �����첽��������
        /// </summary>
        /// <param name="ar">һ����ʾ�첽���õ� IAsyncResult��</param>
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