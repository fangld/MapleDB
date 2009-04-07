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
    /// 底层传输包处理类
    /// </summary>
    public class PacketHandler : IPacketHandler
    {
        #region Fields

        /// <summary>
        /// 一般传输包头的长度
        /// </summary>
        private const int normalHeaderLength = 8;

        /// <summary>
        /// 封装传输包头的长度
        /// </summary>
        private const int sealedHeaderLength = 12;

        /// <summary>
        /// 最大包长度
        /// </summary>
        private const int maxPacketLength = 1048576;

        /// <summary>
        /// 一般传输包头
        /// </summary>
        private byte[] normalHeader;

        /// <summary>
        /// 封装传输包头
        /// </summary>
        private byte[] sealedHeader;

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 加密算法
        /// </summary>
        private SymmetricAlgorithm crypter;

        /// <summary>
        /// 加密器
        /// </summary>
        private ICryptoTransform encryptor;

        /// <summary>
        /// 解密器
        /// </summary>
        private ICryptoTransform decryptor;

        #endregion

        #region Properties

        /// <summary>
        /// 返回连接的套接字
        /// </summary>
        protected Socket Socket
        {
            get { return socket; }
        }

        /// <summary>
        /// 获取数据包的远程端点
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return (IPEndPoint)socket.LocalEndPoint; }
        }

        /// <summary>
        /// 获取密钥
        /// </summary>
        public byte[] Key
        {
            get { return crypter.Key; }
        }

        /// <summary>
        /// 获取初始化向量
        /// </summary>
        public byte[] IV
        {
            get { return crypter.IV; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">客户端Socket</param>
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
        /// 验证一般数据传输包
        /// </summary>
        /// <param name="packet">一般传输包</param>
        /// <returns>如果验证成功返回ture,否则返回false</returns>
        private bool CheckNormalPacket(byte[] packet)
        {
            uint packetCrc = Crc32.ComputeCrcUInt(packet, 0, packet.Length);
            uint headerCrc = BytesConvertor.BytesToUInt32(normalHeader, 4);
            return packetCrc == headerCrc;
        }

        /// <summary>
        /// 验证封装数据传输包
        /// </summary>
        /// <param name="packet">封装传输包</param>
        /// <returns>如果验证成功返回ture,否则返回false</returns>
        private bool CheckSealedPacket(byte[] packet)
        {
            uint packetCrc = Crc32.ComputeCrcUInt(packet, 0, packet.Length);
            uint headerCrc = BytesConvertor.BytesToUInt32(sealedHeader, 8);
            return packetCrc == headerCrc;
        }

        /// <summary>
        /// 构造一般传输包的头
        /// </summary>
        /// <param name="packet">一般传输包</param>
        /// <returns>返回一般传输包的头</returns>
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
        /// 设置密钥和初始化向量
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        public void SetKeyIV(byte[] key, byte[] iv)
        {
            if (key.Length != crypter.KeySize >> 3)
            {
                throw new PacketException("密钥长度不正确!");
            }
            crypter.Key = key;
            crypter.IV = iv;
            encryptor.Dispose();
            decryptor.Dispose();
            encryptor = crypter.CreateEncryptor();
            decryptor = crypter.CreateDecryptor();
        }

        /// <summary>
        /// 发送一般传输包
        /// </summary>
        /// <param name="packet">发送的一般传输包</param>
        public void SendNormalPacket(byte[] packet)
        {
            if (!socket.Connected)
            {
                throw new PacketException("客户端还没有打开!");
            }
            socket.Send(ConstructNormalHeader(packet), 0, normalHeaderLength, SocketFlags.None);
            socket.Send(packet, 0, packet.Length, SocketFlags.None);
        }

        /// <summary>
        /// 读取一般传输包
        /// </summary>
        /// <returns>返回读取的一般传输包</returns>
        public byte[] ReceiveNormalPacket()
        {
            if (!socket.Connected)
            {
                throw new PacketException("客户端还没有打开!");
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
                throw new PacketException("申请内存异常!");
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
            throw new PacketException("一般传输包验证不成功!");
        }

        /// <summary>
        /// 发送封装传输包
        /// </summary>
        /// <param name="packet">发送的传输包</param>
        public void SendSealedPacket(byte[] packet)
        {
            if (!socket.Connected)
            {
                throw new PacketException("客户端还没有打开!");
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
        /// 构造封装传输包的头
        /// </summary>
        /// <param name="normalPacket">一般传输包</param>
        /// <param name="sealedPacket">封装传输包</param>
        /// <returns>返回一般传输包的头</returns>
        private byte[] ConstructSealedHeader(byte[] normalPacket, byte[] sealedPacket)
        {
            BytesConvertor.Int32ToBytes(normalPacket.Length, sealedHeader, 0);
            BytesConvertor.Int32ToBytes(sealedPacket.Length, sealedHeader, 4);
            uint crc = Crc32.ComputeCrcUInt(normalPacket, 0, normalPacket.Length);
            BytesConvertor.UInt32ToBytes(crc, sealedHeader, 8);
            return sealedHeader;
        }

        /// <summary>
        /// 读取封装传输包
        /// </summary>
        /// <returns>返回读取的传输包</returns>
        public byte[] ReceiveSealedPacket()
        {
            if (!socket.Connected)
            {
                throw new PacketException("客户端还没有打开!");
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
                throw new PacketException("申请内存异常!");
            }

            int sealedPacketLength = BytesConvertor.BytesToInt32(sealedHeader, 4);

            if (sealedPacketLength > maxPacketLength || sealedPacketLength < 0)
            {
                throw new PacketException("申请内存异常!");
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
            throw new PacketException("封装传输包验证不成功!");
        }

        /// <summary>
        /// 关闭传输包处理器
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
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
