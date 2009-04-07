using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Packets
{
    /// <summary>
    /// 带日志功能的底层传输包处理类
    /// </summary>
    public class LoggedPacketHandler : IPacketHandler
    {
        #region Fields

        /// <summary>
        /// 日志流
        /// </summary>
        private IList<PacketLogger> loggers;

        /// <summary>
        /// 底层传输包处理类
        /// </summary>
        private IPacketHandler handler;

        #endregion

        #region Properties

        /// <summary>
        /// 获取数据包的远程端点
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return handler.EndPoint; }
        }

        /// <summary>
        /// 获取密钥
        /// </summary>
        public byte[] Key
        {
            get { return handler.Key; }
        }

        /// <summary>
        /// 获取初始化向量
        /// </summary>
        public byte[] IV
        {
            get { return handler.IV; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client">连接套接字</param>
        public LoggedPacketHandler(Socket client)
        {
            handler = new PacketHandler(client);
            loggers = new List<PacketLogger>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 发送一般传输包
        /// </summary>
        /// <param name="packet">发送的一般传输包</param>
        public void SendNormalPacket(byte[] packet)
        {
            handler.SendNormalPacket(packet);
            foreach (PacketLogger logger in loggers)
            {
                logger.Write(packet, EndPoint);
            }
        }

        /// <summary>
        /// 读取一般传输包
        /// </summary>
        /// <returns>返回读取的一般传输包</returns>
        public byte[] ReceiveNormalPacket()
        {
            byte[] result = handler.ReceiveNormalPacket();
            foreach (PacketLogger logger in loggers)
            {
                logger.Write(result, EndPoint);
            }
            return result;
        }

        /// <summary>
        /// 发送封装传输包
        /// </summary>
        /// <param name="packet">发送的传输包</param>
        public void SendSealedPacket(byte[] packet)
        {
            handler.SendSealedPacket(packet);
            foreach (PacketLogger logger in loggers)
            {
                logger.Write(packet, EndPoint);
            }
        }

        /// <summary>
        /// 读取封装传输包
        /// </summary>
        /// <returns>返回读取的传输包</returns>
        public byte[] ReceiveSealedPacket()
        {
            byte[] result = handler.ReceiveSealedPacket();
            foreach (PacketLogger logger in loggers)
            {
                logger.Write(result, EndPoint);
            }
            return result;
        }

        #endregion

        #region IPacketHandler 成员

        public void SetKeyIV(byte[] key, byte[] iv)
        {
            handler.SetKeyIV(key, iv);
        }

        public void Close()
        {
            handler.Close();
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            handler.Dispose();
        }

        #endregion
    }
}