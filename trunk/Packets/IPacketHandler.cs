using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 数据包处理器接口
    /// </summary>
    public interface IPacketHandler : IDisposable
    {
        #region Properties

        /// <summary>
        /// 获取数据包的远程端点
        /// </summary>
        IPEndPoint EndPoint { get;}

        /// <summary>
        /// 获取密钥
        /// </summary>
        byte[] Key{ get;}

        /// <summary>
        /// 获取初始化向量
        /// </summary>
        byte[] IV { get;}

        #endregion

        #region Methods

        /// <summary>
        /// 设置密钥和初始化向量
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        void SetKeyIV(byte[] key, byte[] iv);

        /// <summary>
        /// 发送一般传输包
        /// </summary>
        /// <param name="packet">发送的一般传输包</param>
        void SendNormalPacket(byte[] packet);

        /// <summary>
        /// 读取一般传输包
        /// </summary>
        /// <returns>返回读取的一般传输包</returns>
        byte[] ReceiveNormalPacket();

        /// <summary>
        /// 发送封装传输包
        /// </summary>
        /// <param name="packet">发送的传输包</param>
        void SendSealedPacket(byte[] packet);

        /// <summary>
        /// 读取封装传输包
        /// </summary>
        /// <returns>返回读取的传输包</returns>
        byte[] ReceiveSealedPacket();

        /// <summary>
        /// 关闭传输包处理器
        /// </summary>
        void Close();

        #endregion
    }
}
