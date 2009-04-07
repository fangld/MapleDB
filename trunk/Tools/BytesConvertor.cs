using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    /// <summary>
    /// �ֽ���ת����
    /// </summary>
    public static class BytesConvertor
    {
        /// <summary>
        /// ��32λ�з�������д���ֽ���
        /// </summary>
        /// <param name="value">��Ҫд���32λ�з�������</param>
        /// <param name="buffer">��д����ֽ���</param>
        /// <param name="startIndex">д���ֽ�����λ��</param>
        public static void Int32ToBytes(int value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }

        /// <summary>
        /// ���ֽ���ת��Ϊ32λ�з�������
        /// </summary>
        /// <param name="buffer">��������ֽ���</param>
        /// <param name="startOffset">�������ֽ�����λ��</param>
        /// <returns>����32λ�з�������</returns>
        public static int BytesToInt32(byte[] buffer, int startOffset)
        {
            int result = 0x0;
            result |= buffer[startOffset] << 24;
            result |= buffer[++startOffset] << 16;
            result |= buffer[++startOffset] << 8;
            result |= buffer[++startOffset];
            return result;
        }

        /// <summary>
        /// ��32λ�޷�������д���ֽ���
        /// </summary>
        /// <param name="value">��Ҫд���32λ�޷�������</param>
        /// <param name="buffer">��д����ֽ���</param>
        /// <param name="startIndex">д���ֽ�����λ��</param>
        public static void UInt32ToBytes(uint value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }

        /// <summary>
        /// ���ֽ���ת��Ϊ32λ�޷�������
        /// </summary>
        /// <param name="buffer">��������ֽ���</param>
        /// <param name="startOffset">�������ֽ�����λ��</param>
        /// <returns>����32λ�޷�������</returns>
        public static uint BytesToUInt32(byte[] buffer, int startOffset)
        {
            uint result = 0x0;
            result |= (uint)buffer[startOffset] << 24;
            result |= (uint)buffer[++startOffset] << 16;
            result |= (uint)buffer[++startOffset] << 8;
            result |= buffer[++startOffset];
            return result;
        }
    }
}
