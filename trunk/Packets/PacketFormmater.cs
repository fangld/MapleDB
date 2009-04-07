using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Packets
{
    /// <summary>
    /// ����ʽ������
    /// </summary>
    public static class PacketFormmater
    {
        /// <summary>
        /// ÿ����ʾԪ�ص�����
        /// </summary>
        private const int itemNum = 16;

        /// <summary>
        /// ��ʽ����
        /// </summary>
        /// <param name="packet">����ʽ���İ�</param>
        /// <param name="endPoint">IP�˵�</param>
        /// <returns>��ʽ���İ�</returns>
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
        /// ��ȡÿ�а����ַ���
        /// </summary>
        /// <param name="packet">���ݰ�</param>
        /// <param name="offset">ƫ��λ��</param>
        /// <param name="count">Ҫת�����ֽ���</param>
        /// <returns>����ת������ַ���</returns>
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
