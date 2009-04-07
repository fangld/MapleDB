using System;
using System.Collections.Generic;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 数据包处理器监测器
    /// </summary>
    public interface IPacketMonitor
    {
        /// <summary>
        /// 注册数据包处理器
        /// </summary>
        /// <param name="handler">数据包处理器</param>
        void Register(IPacketHandler handler);

        /// <summary>
        /// 注销数据包处理器
        /// </summary>
        /// <param name="handler">数据包处理器</param>
        void Unregister(IPacketHandler handler);
    }
}
