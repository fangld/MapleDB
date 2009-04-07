using System;
using System.Collections.Generic;
using System.Text;

namespace DbProxy.Client
{
    /// <summary>
    /// �ײ�������ӿ�
    /// </summary>
    internal interface ITdsConnection: IDisposable
    {
        #region Properties

        /// <summary>
        /// ��ȡDB Proxy Server��ַ
        /// </summary>
        string Url { get;}

        /// <summary>
        /// ��ȡDB Proxy Server�˿�
        /// </summary>
        ushort Port { get;}

        /// <summary>
        /// ��ȡ������
        /// </summary>
        string DeviceId { get;}

        /// <summary>
        /// ��ȡ���ȼ�
        /// </summary>
        byte Priority { get;}

        /// <summary>
        /// ��ȡ�����Ƿ��Ѿ���
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// ��ȡ��ʱʱ��
        /// </summary>
        int Timeout { get;}

        #endregion

        #region Methods

        /// <summary>
        /// ������
        /// </summary>
        void Open();

        /// <summary>
        /// �������ݰ�
        /// </summary>
        /// <param name="packet">���ݰ�</param>
        void SendPacket(byte[] packet);

        /// <summary>
        /// �������ݰ�
        /// </summary>
        /// <returns>���ص����ݰ�</returns>
        byte[] ReceivePacket();

        /// <summary>
        /// �ر�����
        /// </summary>
        void Close();

        #endregion
    }
}
