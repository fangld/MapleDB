using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// 包格式化工具
    /// </summary>
    public static class PacketFormmater
    {
        /// <summary>
        /// 每行显示元素的数量
        /// </summary>
        private const int itemNum = 16;

        /// <summary>
        /// 格式化包
        /// </summary>
        /// <param name="packet">待格式化的包</param>
        /// <param name="endPoint">IP端点</param>
        /// <returns>格式化的包</returns>
        public static byte[] Format(byte[] packet, IPEndPoint endPoint)
        {
            StringBuilder sb = new StringBuilder();
            DateTime dateTime = DateTime.Now;
            sb.AppendFormat("======================{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}:{6:0000} ======================\r\n", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
            sb.AppendFormat("Receive From IP EndPoint:{0}, Length:{1}\r\n", endPoint, packet.Length);
            int offset = 0;
            int count = packet.Length;
            do
            {
                int remain = count - offset;
                if (remain > itemNum)
                {
                    string str = GetHexFormat(packet, offset, itemNum);
                    sb.Append(str);
                    offset += itemNum;
                }
                else
                {
                    string str = GetHexFormat(packet, offset, remain);
                    sb.Append(str);
                    break;
                }
            } while (true);
            sb.AppendLine();
            return Encoding.Default.GetBytes(sb.ToString());
        }

        /// <summary>
        /// 获取每行包的字符串
        /// </summary>
        /// <param name="packet">数据包</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">要转换的字节数</param>
        /// <returns>返回转换后的字符串</returns>
        private static string GetHexFormat(byte[] packet, int offset, int count)
        {
            StringBuilder sb = new StringBuilder(71);
            StringBuilder hexSb = new StringBuilder(count);
            sb.AppendFormat("{0:X4} ", offset);
            for (int i = 0; i < count; i++)
            {
                byte b = packet[offset + i];
                sb.AppendFormat("{0:X2} ", b);
                hexSb.Append(b >= 32 && b <= 126 ? (char) b : '.');
            }
            sb.Append(' ', (itemNum - count) * 3);
            sb.Append(hexSb.ToString());
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
