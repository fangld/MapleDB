using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// ���ݰ��������ӿ�
    /// </summary>
    public interface IPacketHandler : IDisposable
    {
        #region Properties

        /// <summary>
        /// ��ȡ���ݰ���Զ�̶˵�
        /// </summary>
        IPEndPoint EndPoint { get;}

        /// <summary>
        /// ��ȡ��Կ
        /// </summary>
        byte[] Key{ get;}

        /// <summary>
        /// ��ȡ��ʼ������
        /// </summary>
        byte[] IV { get;}

        #endregion

        #region Methods

        /// <summary>
        /// ������Կ�ͳ�ʼ������
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        void SetKeyIV(byte[] key, byte[] iv);

        /// <summary>
        /// ����һ�㴫���
        /// </summary>
        /// <param name="packet">���͵�һ�㴫���</param>
        void SendNormalPacket(byte[] packet);

        /// <summary>
        /// ��ȡһ�㴫���
        /// </summary>
        /// <returns>���ض�ȡ��һ�㴫���</returns>
        byte[] ReceiveNormalPacket();

        /// <summary>
        /// ���ͷ�װ�����
        /// </summary>
        /// <param name="packet">���͵Ĵ����</param>
        void SendSealedPacket(byte[] packet);

        /// <summary>
        /// ��ȡ��װ�����
        /// </summary>
        /// <returns>���ض�ȡ�Ĵ����</returns>
        byte[] ReceiveSealedPacket();

        /// <summary>
        /// �رմ����������
        /// </summary>
        void Close();

        #endregion
    }
}
