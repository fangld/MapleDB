using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using CRC;
using Tools;

namespace Packets
{
    /// <summary>
    /// �ײ㴫���������
    /// </summary>
    public class PacketHandler : IPacketHandler
    {
        #region Fields

        /// <summary>
        /// һ�㴫���ͷ�ĳ���
        /// </summary>
        private const int normalHeaderLength = 8;

        /// <summary>
        /// ��װ�����ͷ�ĳ���
        /// </summary>
        private const int sealedHeaderLength = 12;

        /// <summary>
        /// ��������
        /// </summary>
        private const int maxPacketLength = 1048576;

        /// <summary>
        /// һ�㴫���ͷ
        /// </summary>
        private byte[] normalHeader;

        /// <summary>
        /// ��װ�����ͷ
        /// </summary>
        private byte[] sealedHeader;

        /// <summary>
        /// �׽���
        /// </summary>
        private Socket socket;

        /// <summary>
        /// �����㷨
        /// </summary>
        private SymmetricAlgorithm crypter;

        /// <summary>
        /// ������
        /// </summary>
        private ICryptoTransform encryptor;

        /// <summary>
        /// ������
        /// </summary>
        private ICryptoTransform decryptor;

        #endregion

        #region Properties

        /// <summary>
        /// �������ӵ��׽���
        /// </summary>
        protected Socket Socket
        {
            get { return socket; }
        }

        /// <summary>
        /// ��ȡ���ݰ���Զ�̶˵�
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return (IPEndPoint)socket.LocalEndPoint; }
        }

        /// <summary>
        /// ��ȡ��Կ
        /// </summary>
        public byte[] Key
        {
            get { return crypter.Key; }
        }

        /// <summary>
        /// ��ȡ��ʼ������
        /// </summary>
        public byte[] IV
        {
            get { return crypter.IV; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="socket">�ͻ���Socket</param>
        public PacketHandler(Socket socket)
        {
            this.socket = socket;
            crypter = Rijndael.Create();
            encryptor = crypter.CreateEncryptor();
            decryptor = crypter.CreateDecryptor();
            normalHeader = new byte[normalHeaderLength];
            sealedHeader = new byte[sealedHeaderLength];
        }

        #endregion

        #region Private Methods

         /// <summary>
        /// ��֤һ�����ݴ����
        /// </summary>
        /// <param name="packet">һ�㴫���</param>
        /// <returns>�����֤�ɹ�����ture,���򷵻�false</returns>
        private bool CheckNormalPacket(byte[] packet)
        {
            uint packetCrc = Crc32.ComputeCrcUInt(packet, 0, packet.Length);
            uint headerCrc = BytesConvertor.BytesToUInt32(normalHeader, 4);
            return packetCrc == headerCrc;
        }

        /// <summary>
        /// ��֤��װ���ݴ����
        /// </summary>
        /// <param name="packet">��װ�����</param>
        /// <returns>�����֤�ɹ�����ture,���򷵻�false</returns>
        private bool CheckSealedPacket(byte[] packet)
        {
            uint packetCrc = Crc32.ComputeCrcUInt(packet, 0, packet.Length);
            uint headerCrc = BytesConvertor.BytesToUInt32(sealedHeader, 8);
            return packetCrc == headerCrc;
        }

        /// <summary>
        /// ����һ�㴫�����ͷ
        /// </summary>
        /// <param name="packet">һ�㴫���</param>
        /// <returns>����һ�㴫�����ͷ</returns>
        private byte[] ConstructNormalHeader(byte[] packet)
        {
            BytesConvertor.Int32ToBytes(packet.Length, normalHeader, 0);
            uint crc = Crc32.ComputeCrcUInt(packet, 0, packet.Length);
            BytesConvertor.UInt32ToBytes(crc, normalHeader, 4);
            return normalHeader;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// ������Կ�ͳ�ʼ������
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="iv">��ʼ������</param>
        public void SetKeyIV(byte[] key, byte[] iv)
        {
            if (key.Length != crypter.KeySize >> 3)
            {
                throw new PacketException("��Կ���Ȳ���ȷ!");
            }
            crypter.Key = key;
            crypter.IV = iv;
            encryptor.Dispose();
            decryptor.Dispose();
            encryptor = crypter.CreateEncryptor();
            decryptor = crypter.CreateDecryptor();
        }

        /// <summary>
        /// ����һ�㴫���
        /// </summary>
        /// <param name="packet">���͵�һ�㴫���</param>
        public void SendNormalPacket(byte[] packet)
        {
            if (!socket.Connected)
            {
                throw new PacketException("�ͻ��˻�û�д�!");
            }
            socket.Send(ConstructNormalHeader(packet), 0, normalHeaderLength, SocketFlags.None);
            socket.Send(packet, 0, packet.Length, SocketFlags.None);
        }

        /// <summary>
        /// ��ȡһ�㴫���
        /// </summary>
        /// <returns>���ض�ȡ��һ�㴫���</returns>
        public byte[] ReceiveNormalPacket()
        {
            if (!socket.Connected)
            {
                throw new PacketException("�ͻ��˻�û�д�!");
            }

            int offset = 0;
            Console.WriteLine(EndPoint);

            do
            {
                int length = socket.Receive(normalHeader, offset, normalHeaderLength - offset, SocketFlags.None);
                offset += length;
            } while (offset < normalHeaderLength);

            int packetLength = BytesConvertor.BytesToInt32(normalHeader, 0);

            if (packetLength > maxPacketLength || packetLength < 0)
            {
                throw new PacketException("�����ڴ��쳣!");
            }

            byte[] rcvMsg = new byte[packetLength];

            offset = 0;
            do
            {
                int length = socket.Receive(rcvMsg, offset, packetLength - offset, SocketFlags.None);
                offset += length;
            } while (offset < packetLength);

            bool isCorrect = CheckNormalPacket(rcvMsg);
            if (isCorrect)
            {
                return rcvMsg;
            }
            throw new PacketException("һ�㴫�����֤���ɹ�!");
        }

        /// <summary>
        /// ���ͷ�װ�����
        /// </summary>
        /// <param name="packet">���͵Ĵ����</param>
        public void SendSealedPacket(byte[] packet)
        {
            if (!socket.Connected)
            {
                throw new PacketException("�ͻ��˻�û�д�!");
            }

            MemoryStream ms = new MemoryStream();
            GZipStream compressStream = new GZipStream(ms, CompressionMode.Compress);
            CryptoStream encryptoStream = new CryptoStream(compressStream, encryptor, CryptoStreamMode.Write);

            encryptoStream.Write(packet, 0, packet.Length);
            encryptoStream.Flush();
            encryptoStream.Close();

            byte[] sealedPacket = ms.ToArray();
            encryptoStream.Close();
            ms.Dispose();
            compressStream.Dispose();
            encryptoStream.Dispose();

            socket.Send(ConstructSealedHeader(packet, sealedPacket), sealedHeaderLength, SocketFlags.None);
            socket.Send(sealedPacket, sealedPacket.Length, SocketFlags.None);
        }

        /// <summary>
        /// �����װ�������ͷ
        /// </summary>
        /// <param name="normalPacket">һ�㴫���</param>
        /// <param name="sealedPacket">��װ�����</param>
        /// <returns>����һ�㴫�����ͷ</returns>
        private byte[] ConstructSealedHeader(byte[] normalPacket, byte[] sealedPacket)
        {
            BytesConvertor.Int32ToBytes(normalPacket.Length, sealedHeader, 0);
            BytesConvertor.Int32ToBytes(sealedPacket.Length, sealedHeader, 4);
            uint crc = Crc32.ComputeCrcUInt(normalPacket, 0, normalPacket.Length);
            BytesConvertor.UInt32ToBytes(crc, sealedHeader, 8);
            return sealedHeader;
        }

        /// <summary>
        /// ��ȡ��װ�����
        /// </summary>
        /// <returns>���ض�ȡ�Ĵ����</returns>
        public byte[] ReceiveSealedPacket()
        {
            if (!socket.Connected)
            {
                throw new PacketException("�ͻ��˻�û�д�!");
            }
            int offset = 0;

            do
            {
                int length = socket.Receive(sealedHeader, offset, sealedHeaderLength - offset, SocketFlags.None);
                offset += length;
            } while (offset < sealedHeaderLength);

            int normalPacketLength = BytesConvertor.BytesToInt32(sealedHeader, 0);

            if (normalPacketLength > maxPacketLength || normalPacketLength < 0)
            {
                throw new PacketException("�����ڴ��쳣!");
            }

            int sealedPacketLength = BytesConvertor.BytesToInt32(sealedHeader, 4);

            if (sealedPacketLength > maxPacketLength || sealedPacketLength < 0)
            {
                throw new PacketException("�����ڴ��쳣!");
            }

            byte[] sealedPacket = new byte[sealedPacketLength];

            offset = 0;
            do
            {
                int length = socket.Receive(sealedPacket, offset, sealedPacketLength - offset, SocketFlags.None);
                offset += length;
            } while (offset < sealedPacketLength);

            MemoryStream ms = new MemoryStream(sealedPacket);
            GZipStream decompressStream = new GZipStream(ms, CompressionMode.Decompress);
            CryptoStream decryptoStream = new CryptoStream(decompressStream, decryptor, CryptoStreamMode.Read);
            byte[] result = new byte[normalPacketLength];
            decryptoStream.Read(result, 0, normalPacketLength);
            decryptoStream.Close();
            ms.Dispose();
            decompressStream.Dispose();
            decryptoStream.Dispose();

            bool isCorrect = CheckSealedPacket(result);
            if (isCorrect)
            {
                return result;
            }
            throw new PacketException("��װ�������֤���ɹ�!");
        }

        /// <summary>
        /// �رմ����������
        /// </summary>
        public virtual void Close()
        {
            if (socket != null)
            {
                if (socket.Connected)
                {
                    Console.WriteLine("Close socket!");
                    Console.WriteLine("Socket is null:{0}", socket == null);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
        }

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
