using System;
using System.Collections.Generic;
using System.Text;

namespace Packets
{
    /// <summary>
    /// ���ݰ������������
    /// </summary>
    public interface IPacketMonitor
    {
        /// <summary>
        /// ע�����ݰ�������
        /// </summary>
        /// <param name="handler">���ݰ�������</param>
        void Register(IPacketHandler handler);

        /// <summary>
        /// ע�����ݰ�������
        /// </summary>
        /// <param name="handler">���ݰ�������</param>
        void Unregister(IPacketHandler handler);
    }
}
