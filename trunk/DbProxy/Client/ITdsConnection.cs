using System;
using System.Collections.Generic;
using System.Text;

namespace DbProxy.Client
{
    /// <summary>
    /// 底层连接类接口
    /// </summary>
    internal interface ITdsConnection: IDisposable
    {
        #region Properties

        /// <summary>
        /// 获取DB Proxy Server地址
        /// </summary>
        string Url { get;}

        /// <summary>
        /// 获取DB Proxy Server端口
        /// </summary>
        ushort Port { get;}

        /// <summary>
        /// 获取机构号
        /// </summary>
        string DeviceId { get;}

        /// <summary>
        /// 获取优先级
        /// </summary>
        byte Priority { get;}

        /// <summary>
        /// 获取连接是否已经打开
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 获取超时时间
        /// </summary>
        int Timeout { get;}

        #endregion

        #region Methods

        /// <summary>
        /// 打开连接
        /// </summary>
        void Open();

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet">数据包</param>
        void SendPacket(byte[] packet);

        /// <summary>
        /// 接收数据包
        /// </summary>
        /// <returns>返回的数据包</returns>
        byte[] ReceivePacket();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        #endregion
    }
}
